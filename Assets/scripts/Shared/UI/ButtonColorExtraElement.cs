using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using Utils;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(EventTrigger))]
public class ButtonColorExtraElement : Core.BaseBehaviour
{

	[System.Serializable]
	protected class ExtraElementColor
	{
		[SerializeField] private Graphic m_targetGraphic;
		[SerializeField] private float m_fadeDuration = 0.1f; //default Unity value for button

		[SerializeField] private Color m_normalColor;
		[SerializeField] private Color m_highlightedColor;
		[SerializeField] private Color m_pressedColor;
		[SerializeField] private Color m_disabledColor;

		private bool m_enabled;
		private MonoBehaviour m_owner;


		ExtraElementColor()
		{
			m_normalColor = Color.white;
			m_highlightedColor = Color.white;
			m_pressedColor = Color.white;
			m_disabledColor = Color.white;
		}
		public void SetOwner(MonoBehaviour owner)
		{
			m_owner = owner;
		}


		public void OnHiglight()
		{
			SetColor(m_highlightedColor);
		}

		public void OnDisabled()
		{
			SetColor(m_disabledColor);
			m_enabled = false;
		}

		public void OnPointerDown()
		{
			SetColor(m_pressedColor);
		}

		public void OnPointerUp()
		{
			SetColor(m_normalColor);
		}

		public void OnPointerExit()
		{
			SetColor(m_normalColor);
		}

		public void OnEnable()
		{
			m_enabled = true;
			SetColor(m_normalColor);
		}

		void SetColor(Color color)
		{
			if (m_enabled && m_owner.isActiveAndEnabled)
			{
//				m_owner.StartCoroutine(GraphicUtils.ChangeColorInTime(m_targetGraphic, m_targetGraphic.color, color, Ease.Type.LINEAR, m_fadeDuration));
			}
		}
	}

	[SerializeField]private ExtraElementColor[] m_extraElementsToColor = new ExtraElementColor[0];
	private EventTrigger m_eventTrigger;
	private Button m_button;
	private bool m_lastInteractState = true;

	protected override void Awake()
	{
		base.Awake();
		foreach (ExtraElementColor element in m_extraElementsToColor)
		{
			element.SetOwner(this);
		}
	}

	void Start()
	{
		m_eventTrigger = GetComponent<EventTrigger>();
		m_button = GetComponent<Button>();

		m_eventTrigger.AddEvent(EventTriggerType.PointerDown, OnPointerDown);
		m_eventTrigger.AddEvent(EventTriggerType.PointerExit, OnPointerExit);
		m_eventTrigger.AddEvent(EventTriggerType.PointerUp, OnPointerUp);
		m_eventTrigger.AddEvent(EventTriggerType.PointerEnter, OnHiglight);
	}

	void Update()
	{
		if (m_lastInteractState != m_button.IsInteractable())
		{
			if (m_button.IsInteractable())
			{
				OnEnable();
			}
			else
			{
				OnDisable();
			}
				
			m_lastInteractState = m_button.IsInteractable();
		}
	}

	public void OnPointerDown(BaseEventData data)
	{
		foreach (ExtraElementColor element in m_extraElementsToColor)
		{
			element.OnPointerDown();
		}
	}

	public void OnPointerUp(BaseEventData data)
	{
		foreach (ExtraElementColor element in m_extraElementsToColor)
		{
			element.OnPointerUp();
		}
	}

	public void OnPointerExit(BaseEventData data)
	{
		foreach (ExtraElementColor element in m_extraElementsToColor)
		{
			element.OnPointerExit();
		}
	}

	public void OnHiglight(BaseEventData data)
	{
		foreach (ExtraElementColor element in m_extraElementsToColor)
		{
			element.OnHiglight();
		}
	}

	public void OnDisable()
	{
		foreach (ExtraElementColor element in m_extraElementsToColor)
		{
			element.OnDisabled();
		}
	}

	public void OnEnable()
	{
		foreach (ExtraElementColor element in m_extraElementsToColor)
		{
			element.OnEnable();
		}
	}
}
