using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using System;
using Utils;

[RequireComponent(typeof(Slider))]
public class SliderColorExtraElement : Core.BaseBehaviour
{

	[System.Serializable]
	protected class ExtraElementColor
	{
		[SerializeField] private Graphic m_targetGraphic;
		[SerializeField] private float m_fadeDuration = 0.1f; //default Unity value for button

		[SerializeField] private Color m_normalColor;
		[SerializeField] private Color m_disabledColor;

		private bool m_enabled;
		private MonoBehaviour m_owner;
	
		ExtraElementColor()
		{
			m_normalColor = Color.white;
			m_disabledColor = Color.white;
		}
		public void SetOwner(MonoBehaviour owner)
		{
			m_owner = owner;
		}


		public void OnDisabled()
		{
			SetColor(m_disabledColor);
			m_enabled = false;
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

	private Slider m_lider;
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
		m_lider = GetComponent<Slider>();
	}

	void Update()
	{
		if (m_lastInteractState != m_lider.IsInteractable())
		{
			if (m_lider.IsInteractable())
			{
				OnEnable();
			}
			else
			{
				OnDisable();
			}

			m_lastInteractState = m_lider.IsInteractable();
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
