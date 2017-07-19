using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Core;
using Utils;


namespace UI
{
	public interface ITableViewDelegate
	{
		void TableCellTouched(TableViewCell cell);
		void ScrollViewDidScroll();
	}

	public interface ITableViewDataSource
	{
		TableViewCell TableCellAtIndex(int idx);
		int NumberOfCellsInTableView();
		Vector2 TableCellSizeForIndex(int idx);
	}

	/// <summary>
	/// Table view.
	///	This is class is pretty much a straight port from the TableView c++ implementation in cocos 2d-x
	/// </summary>

	public class TableView : BaseBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
	{
		public interface ICustomDelegate
		{
			void DidSnapToNextCell(TableViewCell cell);
			void CentreCellChanged(TableViewCell cell, bool newCell);
		}

		public const int INVALID_INDEX = -1;
		private const float SNAP_TO_CELL_ANIM_DURATION = 0.5f;
		private const float SNAP_TO_NEXT_CELL_VELOCITY_THRESHOLD = 1000f;

		private enum FillOrder
		{
			FILL_TOP_DOWN,
			FILL_BOTTOM_UP,
			FILL_LEFT_RIGHT,
			FILL_RIGHT_LEFT
		}

		[SerializeField] private GameObject m_viewObject;
		[SerializeField] private GameObject m_contentObject;
		[SerializeField] private ScrollRect m_scrollRect;
		[SerializeField] private FillOrder m_fillOrder;
		[SerializeField] private bool m_snapToNextCell;
		[SerializeField] private TouchInputHandler m_touchInputHandler;

		// Carousel. Maybe this should be in a separate behaviour?
		[SerializeField] private bool m_isCarousel;
		[SerializeField] private float m_carouselInterpolationRate = 1.0f;
		[SerializeField] private bool m_carouselScaleCells = true;
		[SerializeField] private float m_carouselScaleEnd = 0.55f;
		[SerializeField] private float m_carouselScaleMiddle = 1.0f;

		//	Stop on cell
		[SerializeField] private bool m_stopOnCell = false;
		[SerializeField] private float m_stopOnCellVelocityThreshold = 100.0f;
		[SerializeField] private Vector2 m_contentSizeExtraMargin = Vector2.zero;

		//	Setting this to true can be quite expensive on device so use with caution
		[SerializeField] private bool m_disableCellsOnRecycle = false;

		private float m_timeToAutoScroll = 0.5f;

		private ITableViewDelegate m_delegate;
		private ITableViewDataSource m_dataSource;
		private ICustomDelegate m_customDelegate;
		private GameObject[] m_cells;
		private Vector2 m_viewSize = Vector2.zero;
		private Vector2 m_viewRectTransformOffsetMin = Vector2.zero;
		private Vector2 m_viewRectTransformOffsetMax = Vector2.zero;
		private RectTransform m_viewTrans;
		private Vector2 m_contentSize;
		private float[] m_cellsPositions;
		private TableViewCellArray m_cellsUsed;
		private TableViewCellArray m_cellsFreed;
		private HashSet<int> m_indices;
		private Vector2 m_minContainerOffsetAdjustment = Vector2.zero;
		private Vector2 m_maxContainerOffsetAdjustment = Vector2.zero;
		private EventTrigger m_eventTrigger;
		private int m_lastIndex;
		private int m_firstIndex;
		private int m_currentCentreIdx = -1;
		private int m_currentSnapToNextCellIndex = -1;
		private bool m_viewSizeSetup = false;
		private int m_numberOfCells = 0;

		// Auto Scroll
		private Vector2 m_originalAutoScrollOffset;
		private Vector2 m_destinationAutoScrollOffset;
		private float m_currentAutoScrollTime = 0;
		private bool m_autoScrollInProgress = false;

		//	Stop on cell
		private bool m_isOnCell;

		private bool m_isDragging;

		//Visual options
		private bool m_reloading = false;

		// Leave as false unless parents also need OnDragBegin, OnDrag and OnDragEnd events
		private bool m_propagateDragEvents = false;
		public bool PropagateDragEvents
		{
			get
			{
				return m_propagateDragEvents;
			}
			set
			{
				m_propagateDragEvents = value;
			}
		}

		public GameObject ContentObject
		{
			get
			{
				return m_contentObject;
			}
		}

		public bool IsDragging
		{
			get
			{
				return m_isDragging;
			}
		}

		public Vector2 NormalisedPosition()
		{
			return m_scrollRect.normalizedPosition;
		}

		protected override void Awake()
		{
			base.Awake();

			m_cellsUsed = new  TableViewCellArray ();
			m_cellsFreed = new  TableViewCellArray ();
			m_indices = new HashSet<int> ();
			m_minContainerOffsetAdjustment = Vector2.zero;
			m_maxContainerOffsetAdjustment = Vector2.zero;
		}

		private void Start()
		{
			SetupViewSize();

			if (m_touchInputHandler != null)
			{
				m_touchInputHandler.TouchUpInsideOccured += HandleTouchUpInside;
			}

			if (m_snapToNextCell)
			{
				SetupForSnapToNextCell();
			}
		}

		private void SetupForSnapToNextCell()
		{
			m_scrollRect.inertia = false;
			m_currentSnapToNextCellIndex = 0;
			m_eventTrigger = gameObject.GetComponent<EventTrigger>();
			if (m_eventTrigger == null)
			{
				m_eventTrigger = gameObject.AddComponent<EventTrigger>();
			}

			Debugger.Assert(m_eventTrigger != null, "Trying to use 'snap to cell' but we do not have a triggerEvent component");
			if (m_eventTrigger != null)
			{
				// NO EVENTS
			}
		}

		public void OnBeginDrag(PointerEventData data)
		{
			if (m_scrollRect.enabled)
			{
				if (m_stopOnCell || m_snapToNextCell)
				{
					m_scrollRect.inertia = true;
				}

				m_isDragging = true;
				m_isOnCell = false;
				m_autoScrollInProgress = false;

				if (m_propagateDragEvents)
				{
					ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, data, ExecuteEvents.beginDragHandler);
				}
			}
		}

		public void OnEndDrag(PointerEventData data)
		{
			m_isDragging = false;

			if (m_scrollRect.enabled)
			{
				if (m_snapToNextCell)
				{
					float vel = GetIsHorizontal() ? m_scrollRect.velocity.x : m_scrollRect.velocity.y;

					int index = 0;
					if (Mathf.Abs(vel) >= SNAP_TO_NEXT_CELL_VELOCITY_THRESHOLD)
					{
						if (vel < 0)
						{
							int maxIndex = Mathf.Max(0, m_cellsPositions.Length - 2);
							index = Math.Min(m_currentSnapToNextCellIndex + 1, maxIndex);
						}
						else
						{
							index = Math.Max(m_currentSnapToNextCellIndex - 1, 0);
						}
					}
					else
					{
						index = GetIndexForCenterCell();
					}

					m_scrollRect.inertia = false;
					m_currentSnapToNextCellIndex = index;
					ScrollToIndex(m_currentSnapToNextCellIndex, true, SNAP_TO_CELL_ANIM_DURATION);
				}

				if (m_propagateDragEvents)
				{
					ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, data, ExecuteEvents.endDragHandler);
				}
			}
		}

		public void OnDrag(PointerEventData data)
		{
			if (m_propagateDragEvents)
			{
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, data, ExecuteEvents.dragHandler);
			}
		}

		public void ManuallySnapToNextCell()
		{
			if (m_snapToNextCell)
			{
				m_currentSnapToNextCellIndex++;
				m_scrollRect.inertia = false;
				ScrollToIndex(m_currentSnapToNextCellIndex, true, SNAP_TO_CELL_ANIM_DURATION);
			}
		}

		public void EnableScroll(bool enable)
		{
			m_scrollRect.enabled = enable;
		}

		public void IterateAllActiveCells(Action<TableViewCell> activeCellCallback)
		{
			for (int i = 0; i < m_cellsUsed.Count(); i++)
			{
				TableViewCell cell = m_cellsUsed.ObjectAtIndex(i);
				activeCellCallback(cell);
			}
		}

		public bool HasViewSize()
		{
			RectTransform viewTrans = m_viewObject.GetRectTransform();
			Vector2 size = viewTrans.GetSize();

			return (size.magnitude > 0f);
		}

		public void SetupViewSize(bool forceReset = false)
		{
			if (!m_viewSizeSetup || forceReset)
			{
				m_viewSizeSetup = true;

				RectTransform viewTrans = m_viewObject.GetRectTransform();

				float viewSizeX = viewTrans.rect.size.x;
				float viewSizeY = viewTrans.rect.size.y;
				m_viewSize = new Vector2 (viewSizeX, viewSizeY);

				m_viewRectTransformOffsetMin = viewTrans.offsetMin;
				m_viewRectTransformOffsetMax = viewTrans.offsetMax;

				if (m_minContainerOffsetAdjustment != Vector2.zero)
				{
					viewTrans.offsetMin = new Vector2 (m_viewRectTransformOffsetMin.x + m_minContainerOffsetAdjustment.x, m_viewRectTransformOffsetMin.y + m_minContainerOffsetAdjustment.y);
				}

				if (m_maxContainerOffsetAdjustment != Vector2.zero)
				{
					viewTrans.offsetMax = new Vector2 (m_viewRectTransformOffsetMax.x + m_maxContainerOffsetAdjustment.x, m_viewRectTransformOffsetMax.y + m_maxContainerOffsetAdjustment.y);
				}
			}
		}

		public void InitForEditor()
		{
			m_cellsUsed = new  TableViewCellArray ();
			m_cellsFreed = new  TableViewCellArray ();
			m_indices = new HashSet<int> ();
			m_minContainerOffsetAdjustment = Vector2.zero;

			RectTransform viewTrans = (RectTransform)m_viewObject.transform;
			m_viewSize = viewTrans.rect.size;
		}

		public void SetMovementType(ScrollRect.MovementType movementType)
		{
			m_scrollRect.movementType = movementType;
		}

		private void Update()
		{
			if (m_autoScrollInProgress)
			{
				RectTransform contentTransform = (RectTransform)m_contentObject.transform;

				m_currentAutoScrollTime += Time.deltaTime;

				float movementX = Ease.EaseOutCubic(m_currentAutoScrollTime, m_originalAutoScrollOffset.x, m_destinationAutoScrollOffset.x - m_originalAutoScrollOffset.x, m_timeToAutoScroll);
				float movementY = Ease.EaseOutCubic(m_currentAutoScrollTime, m_originalAutoScrollOffset.y, m_destinationAutoScrollOffset.y - m_originalAutoScrollOffset.y, m_timeToAutoScroll);
				contentTransform.anchoredPosition = new Vector2 (movementX, movementY);

				if (m_currentAutoScrollTime >= m_timeToAutoScroll)
				{
					contentTransform.anchoredPosition = m_destinationAutoScrollOffset;

					m_autoScrollInProgress = false;

					m_isOnCell = true;

					if (m_customDelegate != null)
					{
						SetSnapToNextCellIndex();

						TableViewCell cell = CellAtIndex(m_currentSnapToNextCellIndex);
						if (cell != null)
						{
							m_customDelegate.DidSnapToNextCell(CellAtIndex(m_currentSnapToNextCellIndex));
						}
					}
				}
			}
		}

		private void SetSnapToNextCellIndex()
		{
			if (m_snapToNextCell)
			{
				RectTransform rectTrans = (RectTransform)transform;
				m_currentSnapToNextCellIndex = IndexFromViewPosition(new Vector2 (rectTrans.rect.size.x * 0.5f, 0f));
			}
		}


		public void SetDelegate(ITableViewDelegate tableViewDelegate)
		{
			m_delegate = tableViewDelegate;
		}

		public void SetDataSource(ITableViewDataSource dataSource)
		{
			m_dataSource = dataSource;

			UpdateCellPositions();
		}

		public void SetCustomDelegate(ICustomDelegate myDelegate)
		{
			m_customDelegate = myDelegate;
		}

		public void SetMinContainerOffsetAdjustment(Vector2 offset)
		{
			m_minContainerOffsetAdjustment = offset;

			if (m_viewSizeSetup)
			{
				RectTransform viewTrans = m_viewObject.GetRectTransform();
				viewTrans.offsetMin = new Vector2 (m_viewRectTransformOffsetMin.x + m_minContainerOffsetAdjustment.x, m_viewRectTransformOffsetMin.y + m_minContainerOffsetAdjustment.y);
			}
		}

		public void SetMaxContainerOffsetAdjustment(Vector2 offset)
		{
			m_maxContainerOffsetAdjustment = offset;

			if (m_viewSizeSetup)
			{
				RectTransform viewTrans = m_viewObject.GetRectTransform();
				viewTrans.offsetMax = new Vector2 (m_viewRectTransformOffsetMax.x + m_maxContainerOffsetAdjustment.x, m_viewRectTransformOffsetMax.y + m_maxContainerOffsetAdjustment.y);
			}
		}

		public bool GetIsHorizontal()
		{
			return m_scrollRect.horizontal;
		}

		public Vector2 GetScrollVelocity()
		{
			return m_scrollRect.velocity;
		}

		public TableViewCell DequeueCell()
		{
			TableViewCell cell = null;

			if (m_cellsFreed.Count() != 0)
			{
				for (int i = 0; i < m_cellsFreed.Count(); i++)
				{
					TableViewCell freeCell = m_cellsFreed.ObjectAtIndex(i);
					if (freeCell.RecycleCell)
					{
						cell = freeCell;
						m_cellsFreed.RemoveSortedObject(freeCell);
						break;
					}
				}
			}

			return cell;
		}

		public void SetRecycleCell(TableViewCell cell, bool recycle)
		{
			cell.RecycleCell = recycle;

			if (recycle)
			{
				for (int i = 0; i < m_cellsFreed.Count(); i++)
				{
					TableViewCell freeCell = m_cellsFreed.ObjectAtIndex(i);
					if (freeCell.RecycleCell && m_disableCellsOnRecycle)
					{
						freeCell.gameObject.SetActive(false);	
					}
				}		
			}
		}

		public void ReloadDataWithOffset()
		{
			Vector2 offset = GetContentOffset();
			ReloadData();

			SetContentOffset(offset);
		}

		public void ReloadData()
		{
			m_reloading = true;
			if (m_scrollRect.horizontal && m_scrollRect.vertical)
			{
				Debugger.Log("TableView cannot have both horizontal and vertical scrolling enabled.", Debugger.Severity.ERROR);
				return;
			}

			m_numberOfCells = m_dataSource.NumberOfCellsInTableView();

			int numCells = m_cellsUsed.Count();
			for (int i = 0; i < numCells; i++)
			{
				TableViewCell cell = m_cellsUsed.ObjectAtIndex(i);

				m_cellsFreed.AddObject(cell);
				cell.Reset();
			}

			int numFreeCells = m_cellsFreed.Count();
			for (int i = 0; i < numFreeCells; i++)
			{
				TableViewCell cell = m_cellsFreed.ObjectAtIndex(i);

				if (cell.RecycleCell)
				{
					cell.gameObject.SetActive(false);	
				}
			}

			m_indices.Clear();
			m_cellsUsed.Release();

			UpdateCellPositions();
			UpdateContentSize();

			if (m_numberOfCells > 0)
			{
				OnScrollViewDidScroll();
			}

			m_reloading = false;
		}

		private void UpdateContentSize()
		{
			Vector2 size = Vector2.zero;
			int cellsCount = m_numberOfCells;

			if (cellsCount > 0)
			{
				float maxPosition = 0;

				Vector2 cellSize;
				for (int i = 0; i < cellsCount; i++)
				{				
					cellSize = m_dataSource.TableCellSizeForIndex(i);

					if (m_scrollRect.horizontal)
					{
						maxPosition += cellSize.x;
					}
					else
					{
						maxPosition += cellSize.y;
					}
				}

				if (m_scrollRect.horizontal)
				{
					size = new Vector2 (maxPosition, m_viewSize.y);
				}
				else
				{
					size = new Vector2 (m_viewSize.x, maxPosition);
				}		
			}

			SetContentSize(size);

			SetContentOffset(new Vector2 (0, 0));
		}

		private void SetContentSize(Vector2 size)
		{
			RectTransform contentTrans = (RectTransform)m_contentObject.transform; 

			contentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
			contentTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

			m_contentSize = new Vector2 (Math.Max(size.x, m_viewSize.x), Math.Max(size.y, m_viewSize.y));
		}

		public Vector2 GetContentSize()
		{
			return m_contentSize;
		}

		public Vector2 GetViewSize()
		{
			return m_viewSize;
		}

		private Vector2 GetContentOffsetInternal()
		{
			RectTransform contentTransform = (RectTransform)m_contentObject.transform;
			Vector2 offset = contentTransform.anchoredPosition;

			return offset;
		}

		/*
		 * use ONLY FROM OUTSIDE the class, inside class use GetContentOffsetInternal
		 * */
		public Vector2 GetContentOffset()
		{
			Vector2 offset = GetContentOffsetInternal();

			if (m_fillOrder == FillOrder.FILL_TOP_DOWN)
			{
				offset.y = MinContainerOffset().y - offset.y;
			}
			else if (m_fillOrder == FillOrder.FILL_RIGHT_LEFT)
			{
				offset.x = MinContainerOffset().x - offset.x;
			}
			return offset;
		}

		public void SetContentOffset(Vector2 offset, bool animated = false, float animationTime = 0.5f)
		{
			if (m_fillOrder == FillOrder.FILL_TOP_DOWN)
			{
				offset.y = MinContainerOffset().y - offset.y;
			}
			else if (m_fillOrder == FillOrder.FILL_RIGHT_LEFT)
			{
				offset.x = MinContainerOffset().x - offset.x;
			}

			if (offset.x > 0)
			{
				offset.x = 0;
			}
			if (offset.y > 0)
			{
				offset.y = 0;
			}

			if (animated)
			{ 			
				m_timeToAutoScroll = animationTime;
				m_originalAutoScrollOffset = GetContentOffsetInternal();
				m_destinationAutoScrollOffset = offset;

				m_autoScrollInProgress = true;
				m_currentAutoScrollTime = 0;
			}
			else
			{ 
				//set the container position directly

				RectTransform contentTransform = (RectTransform)m_contentObject.transform;
				contentTransform.anchoredPosition = offset;

				if (m_numberOfCells > 0)
				{
					OnScrollViewDidScroll();
				}

				SetSnapToNextCellIndex();
			}
		}

		Vector2 MinContainerOffset()
		{
			return new Vector2 (m_viewSize.x - m_contentSize.x,
				m_viewSize.y - m_contentSize.y);

		}

		void UpdateCellPositions()
		{
			int cellsCount = m_numberOfCells;
			Array.Resize(ref m_cellsPositions, cellsCount + 1);
			Array.Clear(m_cellsPositions, 0, m_cellsPositions.Length);

			if (cellsCount > 0)
			{
				float currentPos = 0;
				Vector2 cellSize;

				for (int i = 0; i < cellsCount; i++)
				{
					cellSize = m_dataSource.TableCellSizeForIndex(i);

					if (m_scrollRect.horizontal)
					{
						m_cellsPositions[i] = currentPos;
						currentPos += cellSize.x;
					}
					else
					{
						m_cellsPositions[i] = currentPos;	
						currentPos += cellSize.y;
					}
				}
				m_cellsPositions[cellsCount] = currentPos;//1 extra value allows us to get right/bottom of the last cell
			}
		}

		public void OnScrollViewDidScroll()
		{
			if (m_dataSource == null)
			{
				return;
			}

			int uCountOfItems = m_numberOfCells;

			if (0 == uCountOfItems)
			{
				return;
			}

			if (m_delegate != null)
			{
				m_delegate.ScrollViewDidScroll();
			}

			UpdateCentreCell();

			int startIdx = 0, endIdx = 0, idx = 0, maxIdx = 0;

			Vector2 offset = GetContentOffsetInternal();

			maxIdx = Math.Max(uCountOfItems - 1, 0);

			if (m_fillOrder == FillOrder.FILL_TOP_DOWN)
			{
				offset.y = m_contentSize.y - (m_viewSize.y - offset.y);

				offset.y -= m_minContainerOffsetAdjustment.y;
			}
			else if (m_fillOrder == FillOrder.FILL_BOTTOM_UP)
			{
				offset.y = m_viewSize.y - offset.y;
			}
			else if (m_fillOrder == FillOrder.FILL_RIGHT_LEFT)
			{
				offset.x = m_contentSize.x - (m_viewSize.x - offset.x);
			}
			else if (m_fillOrder == FillOrder.FILL_LEFT_RIGHT)
			{
				offset.x = m_viewSize.x - offset.x;

				offset.x -= m_minContainerOffsetAdjustment.x;
			}

			offset.y -= m_contentSizeExtraMargin.y;
			offset.x -= m_contentSizeExtraMargin.x;

			startIdx = IndexFromOffset(offset);

			offset.y += 2f * m_contentSizeExtraMargin.y;
			offset.x += 2f * m_contentSizeExtraMargin.x;

			if (startIdx == INVALID_INDEX)
			{
				startIdx = uCountOfItems - 1;
			}

			if (m_fillOrder == FillOrder.FILL_TOP_DOWN)
			{
				
				offset.y = offset.y + m_viewSize.y;
			}
			else if (m_fillOrder == FillOrder.FILL_BOTTOM_UP)
			{
				float offsetAdjustment = Math.Max(offset.y - m_contentSize.y, 0);
				offset.y = offset.y - m_viewSize.y - offsetAdjustment;
			}
			else if (m_fillOrder == FillOrder.FILL_RIGHT_LEFT)
			{
				offset.x = offset.x + m_viewSize.x;
			}
			else if (m_fillOrder == FillOrder.FILL_LEFT_RIGHT)
			{
				float offsetAdjustment = Math.Max(offset.x - m_contentSize.x, 0);
				offset.x = offset.x - m_viewSize.x - offsetAdjustment;
			}

			endIdx = IndexFromOffset(offset);

			if (m_fillOrder == FillOrder.FILL_BOTTOM_UP || m_fillOrder == FillOrder.FILL_LEFT_RIGHT)
			{
				int tempStart = startIdx;
				startIdx = endIdx;
				endIdx = tempStart;
			}

			if (endIdx == INVALID_INDEX)
			{
				endIdx = uCountOfItems - 1;
			}

			if (m_cellsUsed.Count() > 0)
			{
				TableViewCell cell = m_cellsUsed.ObjectAtIndex(0);

				idx = cell.ID;
				while (idx < startIdx)
				{
					MoveCellOutOfSight(cell);
					if (m_cellsUsed.Count() > 0)
					{
						cell = m_cellsUsed.ObjectAtIndex(0);
						idx = cell.ID;
					}
					else
					{
						break;
					}
				}
			}

			if (m_cellsUsed.Count() > 0)
			{
				TableViewCell cell = m_cellsUsed.LastObject();
				idx = cell.ID;

				while (idx <= maxIdx && idx > endIdx)
				{
					MoveCellOutOfSight(cell);
					if (m_cellsUsed.Count() > 0)
					{
						cell = m_cellsUsed.LastObject();
						idx = cell.ID;

					}
					else
					{
						break;
					}
				}
			}

			m_lastIndex = endIdx;
			m_firstIndex = startIdx;

			for (int i = startIdx; i <= endIdx; i++)
			{
				if (m_indices.Contains(i))
				{
					continue;
				}

				UpdateCellAtIndex(i, startIdx);
			}

			if (m_isCarousel)
			{
				UpdateCarousel();
			}

			if (m_stopOnCell)
			{
				UpdateStopOnCell();
			}
		}

		private void UpdateCentreCell()
		{
			if (m_customDelegate != null)
			{
				int index = GetIndexForCenterCell();

				if (index != m_currentCentreIdx)
				{
					TableViewCell cell = CellAtIndex(m_currentCentreIdx);
					if (cell != null)
					{
						m_customDelegate.CentreCellChanged(cell, false);
					}

					TableViewCell newCell = CellAtIndex(index);
					if (newCell != null)
					{
						m_customDelegate.CentreCellChanged(newCell, true);

						m_currentCentreIdx = index;
					}
				}
			}
		}

		public int GetFirstIndex()
		{  
			return m_firstIndex;
		}
		public int GetLastIndex()
		{  
			return m_lastIndex;
		}


		void MoveCellOutOfSight(TableViewCell cell)
		{   
			m_cellsFreed.AddObject(cell);
			m_cellsUsed.RemoveSortedObject(cell);

			m_indices.Remove(cell.ID);
			cell.Reset();

			if (cell.RecycleCell && m_disableCellsOnRecycle)
			{
				cell.gameObject.SetActive(false);
			}
		}

		void UpdateCellAtIndex(int idx, int startIdx)
		{
			if (idx == INVALID_INDEX)
			{
				return;
			}

			int uCountOfItems = m_numberOfCells;
			if (0 == uCountOfItems || idx > uCountOfItems - 1)
			{
				return;
			}

			TableViewCell cell = CellAtIndex(idx);
			if (cell)
			{
				MoveCellOutOfSight(cell);
			}
			cell = m_dataSource.TableCellAtIndex(idx);
			SetIndexForCell(idx, cell);
			AddCellIfNecessary(cell);
		}

		public TableViewCell CellAtIndex(int idx)
		{
			TableViewCell found = null;

			if (m_indices.Contains(idx))
			{
				found = m_cellsUsed.ObjectWithObjectID(idx);
			}

			return found;
		}

		void SetIndexForCell(int index, TableViewCell cell)
		{
			RectTransform trans = (RectTransform)cell.gameObject.transform;
			Vector2 offset = OffsetFromIndex(index);
			trans.anchoredPosition = offset;
			cell.ID = index;
		}

		void AddCellIfNecessary(TableViewCell cell)
		{
			cell.gameObject.SetActive(true);
			m_cellsUsed.InsertSortedObject(cell);
			m_indices.Add(cell.ID);
		}

		public Vector2 OffsetFromIndex(int index)
		{
			Vector2 offset = CellPosFromIndex(index);
			Vector2 cellSize = m_dataSource.TableCellSizeForIndex(index);

			if (m_fillOrder == FillOrder.FILL_TOP_DOWN)
			{
				offset.y = (m_contentSize.y) - offset.y - cellSize.y;
			}
			else if (m_fillOrder == FillOrder.FILL_RIGHT_LEFT)
			{
				offset.x = (m_contentSize.x) - offset.x - cellSize.x;
			}

			return offset;
		}

		Vector2 CellPosFromIndex(int index)
		{
			Vector2 offset;

			if (m_scrollRect.horizontal)
			{
				offset = new Vector2 (m_cellsPositions[index], 0);
			}
			else
			{
				offset = new Vector2 (0, m_cellsPositions[index]);
			}

			return offset;
		}

		private int IndexFromOffset(Vector2 offset)
		{
			int index = 0;
			int maxIdx = m_numberOfCells - 1;

			index = IndexFromFillOrderAdjustedOffset(offset);

			if (index != -1)
			{
				index = Math.Max(0, index);
				if (index > maxIdx)
				{
					index = INVALID_INDEX;
				}
			}

			return index;
		}

		public int GetIndexForCenterCell()
		{
			RectTransform rectTrans = (RectTransform)transform;
			int index = IndexFromViewPosition(new Vector2 (rectTrans.rect.size.x * 0.5f, 0f));
			return index;
		}

		public int IndexFromViewPosition(Vector2 viewPos)
		{
			Vector2 curPos = viewPos;
			Vector2 contentOffset = GetContentOffsetInternal();

			if (m_fillOrder == FillOrder.FILL_TOP_DOWN)
			{
				curPos.y = m_viewSize.y - curPos.y;

				contentOffset.y = -(m_contentSize.y - m_viewSize.y) - contentOffset.y;

				contentOffset.y -= m_minContainerOffsetAdjustment.y;
			}
			else if (m_fillOrder == FillOrder.FILL_RIGHT_LEFT)
			{
				curPos.x = m_viewSize.x - curPos.x;

				contentOffset.x = -(m_contentSize.x - m_viewSize.x) - contentOffset.x;

				contentOffset.x -= m_minContainerOffsetAdjustment.x;
			}

			curPos.x -= contentOffset.x;
			curPos.y -= contentOffset.y;

			int index = IndexFromOffset(curPos);
			return index;
		}

		private int IndexFromFillOrderAdjustedOffset(Vector2 offset)
		{
			int low = 0;
			int high = m_numberOfCells - 1;
			float search;

			if (m_scrollRect.horizontal)
			{
				search = offset.x;
			}
			else
			{
				search = offset.y;
			}

			while (high >= low)
			{
				int index = low + (high - low) / 2;

				float cellStart = m_cellsPositions[index];
				float cellEnd = m_cellsPositions[index + 1];

				if (search >= cellStart && search <= cellEnd)
				{
					return index;
				}
				else if (search < cellStart)
				{
					high = index - 1;
				}
				else
				{
					low = index + 1;
				}
			}

			if (low <= 0)
			{
				return 0;
			}

			return high;
		}

		public void ScrollToIndex(int index, bool animate = true, float scrollTime = SNAP_TO_CELL_ANIM_DURATION)
		{
			Vector3 newOffset = (OffsetFromIndex(0) - OffsetFromIndex(index));
			SetContentOffset(newOffset, animate, scrollTime);
		}

		private void HandleTouchUpInside(Vector2 position)
		{
			Vector3 touchPos = Camera.main.ScreenToWorldPoint(position);
			Vector2 viewPos = new Vector2 (touchPos.x, touchPos.y);

			// Get index of cell
			int index = IndexFromViewPosition(viewPos);
			TableViewCell tappedCell = CellAtIndex(index);

			// Check that the point is actually within the bounds
			RectTransform rectTransform = tappedCell.transform as RectTransform;
			if (rectTransform && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, position, Camera.main))
			{
				m_delegate.TableCellTouched(tappedCell);
			}
		}

		public bool IsReloading()
		{
			return m_reloading;
		}

		private void UpdateCarousel()
		{
			if (m_cellsUsed.Count() > 0)
			{
				float viewSizeDist = m_scrollRect.vertical ? m_viewSize.y : m_viewSize.x;
				float contentSizeDist = m_scrollRect.vertical ? m_contentSize.y : m_contentSize.x;

				Vector2 contentOffset = GetContentOffsetInternal();

				contentOffset.x += m_minContainerOffsetAdjustment.x;

				float offsetDist = m_scrollRect.vertical ? contentOffset.y : contentOffset.x;
				float maxOffset = viewSizeDist;
				float minOffset = 0;

				int uTotalCells = m_numberOfCells;

				// iterate over these to position them
				for (int i = 0; i < m_cellsUsed.Count(); ++i)
				{
					TableViewCell cell = m_cellsUsed.ObjectAtIndex(i);
					int idx = cell.ID;

					int halfCount = (int)(m_cellsUsed.Count() * 0.5f);
					int zOrder = i;
					if (i >= halfCount)
					{
						zOrder = m_cellsUsed.Count() - i;
					}

					cell.transform.SetSiblingIndex(zOrder);

					Vector2 cellPos = cell.gameObject.transform.localPosition;

					float shouldBePos = m_cellsPositions[idx];
					Vector2 cellSize = m_dataSource.TableCellSizeForIndex(idx);
					float cellSizeDist = m_scrollRect.vertical ? cellSize.y : cellSize.x;

					if (i == 0)
					{
						maxOffset = viewSizeDist - (cellSizeDist * 0.5f);
						minOffset = (cellSizeDist * 0.5f);
					}

					float convertedShouldBePos = m_scrollRect.vertical ? contentSizeDist - shouldBePos - cellSizeDist : shouldBePos;
					float screenRelPos = convertedShouldBePos + (cellSizeDist * 0.5f) + /*-*/ offsetDist;
					float screenSpaceSize = maxOffset - minOffset;

					if (idx <= 1 & screenRelPos > viewSizeDist * 0.5f)
					{
						// THIS IS VERY SPECIFC, NEEDS TO BE GENERALISED
						zOrder = (int)(Mathf.Ceil(m_cellsUsed.Count() * 0.5f)) + (2 - idx);
						cell.transform.SetSiblingIndex(zOrder);
					}
					if (idx == uTotalCells - 1 && screenRelPos < viewSizeDist * 0.5f)
					{
						cell.transform.SetSiblingIndex(m_cellsUsed.Count());
					}

					if (screenRelPos >= maxOffset)
					{
						float toPosScalar = maxOffset - (cellSizeDist * 0.5f) - offsetDist;
						Vector2 toPos = m_scrollRect.vertical ? new Vector2 (cellPos.x, toPosScalar) : new Vector2 (toPosScalar, cellPos.y);

						cell.gameObject.transform.localPosition = toPos;
					}
					else if (screenRelPos <= minOffset)
					{
						float toPosScalar = minOffset - (cellSizeDist * 0.5f) - offsetDist;
						Vector2 toPos = m_scrollRect.vertical ? new Vector2 (cellPos.x, toPosScalar) : new Vector2 (toPosScalar, cellPos.y);

						cell.gameObject.transform.localPosition = toPos;

					}
					else
					{
						float adjustedRelPos = screenRelPos - minOffset;
						float time = adjustedRelPos / screenSpaceSize;
						time *= 2;
						float timeI2 = 0.0f;
						if (time < 1)
						{
							timeI2 = 0.5f * Mathf.Pow(time, m_carouselInterpolationRate);
						}
						else
						{
							timeI2 = 1.0f - 0.5f * Mathf.Pow(2 - time, m_carouselInterpolationRate);
						}
						screenRelPos = (screenSpaceSize * timeI2) + minOffset;

						float toPosScalar = screenRelPos - (cellSizeDist * 0.5f) - offsetDist;
						Vector2 toPos = m_scrollRect.vertical ? new Vector2 (cellPos.x, toPosScalar) : new Vector2 (toPosScalar, cellPos.y);

						cell.gameObject.transform.localPosition = toPos;
					}


					if (m_carouselScaleCells)
					{
						UpdateCarouselCellScale(cell, screenRelPos, minOffset, screenSpaceSize);
					}
				}
			}
		}

		private void UpdateCarouselCellScale(TableViewCell cell, float screenRelPos, float minOffset, float screenSpaceSize)
		{
			float adjustedRelPos = screenRelPos - minOffset;
			float xPosRatio = adjustedRelPos / screenSpaceSize;
			float timeT = xPosRatio * 2.0f;
			float scale = 1.0f;
			float EndScale = m_carouselScaleEnd;
			float MiddleScale = m_carouselScaleMiddle;
			float DiffScale = MiddleScale - EndScale;
			if (timeT < 1.0f)
			{
				scale = (DiffScale * timeT) + EndScale;
			}
			else
			{
				scale = (DiffScale * (2.0f - timeT)) + EndScale;
			}

			for (int i = 0; i < cell.gameObject.transform.childCount; i++)
			{
				Transform child = cell.gameObject.transform.GetChild(i);
				child.localScale = new Vector3 (scale, scale, 1.0f);	
			}
		}

		private void UpdateStopOnCell()
		{
			if (!m_isDragging && !m_isOnCell && !m_autoScrollInProgress && !m_reloading)
			{
				float velocity = m_scrollRect.vertical ? m_scrollRect.velocity.y : m_scrollRect.velocity.x;

				if (Mathf.Abs(velocity) < m_stopOnCellVelocityThreshold)
				{
					m_scrollRect.inertia = false;

					int index = GetIndexForCenterCell();

					Vector3 newOffset = (OffsetFromIndex(0) - OffsetFromIndex(index));
					Vector2 currentOffset = GetContentOffset();

					float newOffsetDist = m_scrollRect.vertical ? newOffset.y : newOffset.x;
					float currentOffsetDist = m_scrollRect.vertical ? currentOffset.y : currentOffset.x;

					if (currentOffsetDist < newOffsetDist && velocity < 0.0f)
					{
						int numCells = m_numberOfCells;
						if (index < (numCells - 1))
						{
							index++;
						}
					}
					else if (currentOffsetDist > newOffsetDist && velocity > 0.0f)
					{
						if (index > 0)
						{
							index--;	
						}
					}

					ScrollToIndex(index, true, SNAP_TO_CELL_ANIM_DURATION);	
				}
			}
		}
	}
}