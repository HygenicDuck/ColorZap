using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
	public class TableViewCellArray
	{
		private List<TableViewCell> m_cells = new List<TableViewCell> ();

		public int IndexOfSortedObject(TableViewCell cell)
		{
			int idx = 0;
			if (cell != null)
			{
				// FIXME: need to use binary search to improve performance
				int uPrevObjectID = 0;
				int uOfSortObjectID = cell.ID;

				foreach (TableViewCell iCell in m_cells)
				{
					int uCurObjectID = iCell.ID;

					if ((uOfSortObjectID == uCurObjectID)
					    || (uOfSortObjectID >= uPrevObjectID && uOfSortObjectID < uCurObjectID))
					{
						break;
					}

					uPrevObjectID = uCurObjectID;
					idx++;
				}
			}
			else
			{
				idx = TableView.INVALID_INDEX;
			}
			return idx;
		}

		public int Count()
		{
			return m_cells.Count;
		}

		public TableViewCell ObjectAtIndex(int idx)
		{
			return m_cells[idx];
		}

		//	TODO: More efficient / elegant way of doing this.
		//
		//	Also does m_cells need to be an array rather than a list?
		//	...depends how often this is called and how often insertSortedObject is called
		public TableViewCell ObjectWithObjectID(int id)
		{
			TableViewCell ret = null;

			foreach (TableViewCell cell in m_cells)
			{
				if (cell.ID == id)
				{
					ret = cell;
					break;
				}
			}

			return ret;
		}


		public void InsertSortedObject(TableViewCell cell)
		{
			int idx;
			idx = IndexOfSortedObject(cell);

			//insertObject(cell, idx);
			m_cells.Insert(idx, cell);
		}

		public void AddObject(TableViewCell cell)
		{
			m_cells.Add(cell);
		}

		public void RemoveSortedObject(TableViewCell cell)
		{
			m_cells.Remove(cell);
		}

		public void RemoveObjectAtIndex(int idx)
		{
			m_cells.RemoveAt(idx);
		}

		public TableViewCell LastObject()
		{
			TableViewCell ret = m_cells[m_cells.Count - 1];
			return ret;
		}

		public void Release()
		{
			m_cells.Clear();
		}
	}
}