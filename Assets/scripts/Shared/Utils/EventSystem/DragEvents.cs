using System;
using Utils;
using UnityEngine;

namespace Utils
{
	public class DragEvents
	{
		public delegate void DragPickedUpDelegate (int groupID, DraggableWidget.Payload payload);
		public static event DragPickedUpDelegate DragPickedUp;
		public static void SendDragPickedUpEvent(int groupID, DraggableWidget.Payload payload)
		{
			if (DragPickedUp != null)
			{
				Debugger.Log("Event DragPickedUp sent", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.DRAG_AND_DROP);
				DragPickedUp(groupID, payload);
			}
		}

		public delegate void DragMovedDelegate (int groupID, Vector2 position, DraggableWidget.Payload payload);
		public static event DragMovedDelegate DragMoved;
		public static void SendDragMovedEvent(int groupID, Vector2 position, DraggableWidget.Payload payload)
		{
			if (DragMoved != null)
			{
				DragMoved(groupID, position, payload);
			}
		}

		public delegate void DragDroppedDelegate (int groupID, DraggableWidget.Payload payload);
		public static event DragDroppedDelegate DragDropped;
		public static void SendDragDroppedEvent(int groupID, DraggableWidget.Payload payload)
		{
			if (DragDropped != null)
			{
				Debugger.Log("Event DragDropped sent", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.DRAG_AND_DROP);
				DragDropped(groupID, payload);
			}
		}

		public delegate void DragDroppedWithinTargetDelegate (int groupID, int targetID, RectTransform targetRectTrans);
		public static event DragDroppedWithinTargetDelegate DragDroppedWithinTarget;
		public static void SendDragDroppedWithinTargetEvent(int groupID, int targetID, RectTransform targetRectTrans)
		{
			if (DragDroppedWithinTarget != null)
			{
				Debugger.Log("Event DragDroppedWithinTarget sent", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.DRAG_AND_DROP);
				DragDroppedWithinTarget(groupID, targetID, targetRectTrans);
			}
		}
	}
}