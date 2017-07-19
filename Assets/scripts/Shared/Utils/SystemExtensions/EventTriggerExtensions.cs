using UnityEngine.EventSystems;

namespace Utils
{

	static class EventTriggerExtensions
	{
		public static void AddEvent(this EventTrigger eventTrigger, EventTriggerType triggerType, UnityEngine.Events.UnityAction<BaseEventData> function)
		{
			EventTrigger.Entry entry = new EventTrigger.Entry ();
			entry.eventID = triggerType;
			EventTrigger.TriggerEvent callback = new EventTrigger.TriggerEvent ();
			callback.AddListener(function);
			entry.callback = callback;
			eventTrigger.triggers.Add(entry);
		}
	}
}