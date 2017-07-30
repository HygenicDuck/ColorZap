using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class Visibility : MonoBehaviour
    {
		public enum EffectType
		{
			Pulse,
			Fade,
			PulseFade,
			SineScale,
			SineFade
		}

		private float _timer = 0f;
		private float _effectPeriod = 0f;
		private float _pulseMaxScale = 1.0f;
		private float _startScale;
		private float _initialAlpha = 1.0f;
		private EffectType _effect;
		private float _alpha,_startAlpha,_endAlpha,_pulseAlpha;
		private float _midAlpha,_alphaRange;

		// Use this for initialization
		private void Start()
		{
		}

		private void Awake()
		{
			_timer = 0f;
			_pulseMaxScale = 1f;
			//SetAlpha (1);
		}

		public void Hide ()
		{
			SetAlpha (0);
		}

		public void Show ()
		{
			SetAlpha (_initialAlpha);
		}

		public void SetInitialAlpha(float alpha)
		{
			_initialAlpha = alpha;
		}
		
		public void FadeOut (float time)
		{
			_startAlpha = _alpha = _initialAlpha;
			_endAlpha = 0.0f;
			_effectPeriod = _timer = time;
			_effect = EffectType.Fade;
		}

		public void FadeIn (float time)
		{
			_startAlpha = _alpha = 0.0f;
			_endAlpha = _initialAlpha;
			_effectPeriod = _timer = time;
			_effect = EffectType.Fade;
		}
		
		public void FadeIn (float startTime, float endTime, float currentTime)
		{
			Debug.Log ("Fadin startTime "+startTime+" endTime "+endTime+" currentTime "+currentTime);
			_startAlpha = _alpha = 0.0f;
			_endAlpha = _initialAlpha;

			if (currentTime >= endTime)
			{
				SetAlpha(_endAlpha);
				return;
			}

			float alpha;
			if (currentTime < startTime)
				alpha = _startAlpha;
			else
				alpha = _startAlpha + ((_endAlpha - _startAlpha)*(currentTime - startTime)/(endTime - startTime));

			SetAlpha (alpha);
			_startAlpha = alpha;
			_effectPeriod = _timer = endTime - currentTime;
			_effect = EffectType.Fade;
			Debug.Log ("Fadin2 _startAlpha "+_startAlpha+" _endAlpha "+_endAlpha+" _effectPeriod "+_effectPeriod);

		}
		
		public void PulseFadeIn (float time, float pulseFactor)
		{
			// fade that overshoots

			_startAlpha = _alpha = 0.0f;
			_endAlpha = _initialAlpha;
			_pulseAlpha = _endAlpha * pulseFactor;
			_effectPeriod = _timer = time;
			_effect = EffectType.PulseFade;
		}

		public void PulseFade (float time, float pulseFactor)
		{
			// fade that overshoots
			
			_startAlpha = _alpha = _endAlpha = _initialAlpha;
			_pulseAlpha = _endAlpha * pulseFactor;
			_effectPeriod = _timer = time;
			_effect = EffectType.PulseFade;
		}

		public void PulseFade (float time, float startAlpha, float endAlpha, float peakAlpha)
		{
			_startAlpha = _alpha = startAlpha;
			_endAlpha = endAlpha;
			_pulseAlpha = peakAlpha;
			_effectPeriod = _timer = time;
			_effect = EffectType.PulseFade;
		}

		public void ShowMomentarily (float time)
		{
			StartCoroutine(ShowMomentarilyCoroutine(time));
		}

		IEnumerator ShowMomentarilyCoroutine(float time) 
		{
			SetAlpha (_initialAlpha);
			yield return new WaitForSeconds(time);
			SetAlpha (0);
		}

		public void SetAlpha(float a)
		{
			Text text = gameObject.GetComponent<Text>();
			RawImage rawImage = gameObject.GetComponent<RawImage>();
			//Image image = gameObject.GetComponent<Image>();
			Button button = gameObject.GetComponent<Button>();
			if (text)
			{
				Color c = text.color;
				c.a = a;
				text.color = c;
			}
			else if (rawImage)
			{
				Color c = rawImage.color;
				c.a = a;
				rawImage.color = c;
			}
//			else if (image)
//			{
//				Color c = image.color;
//				c.a = a;
//				image.color = c;
//			}
			else if (button)
			{
				ColorBlock cb = button.colors;
				Color c = cb.disabledColor;
				c.a = a;
				cb.normalColor = c;
				cb.disabledColor = c;
				button.colors = cb;

				Text label = transform.GetComponentInChildren<Text>();
				if (label)
				{
					Color tc = label.color;
					tc.a = a;
					label.color = tc;
				}
			}
			else
			{
				// doesn't fade. Just disables if a == 0
				Image image = gameObject.GetComponent<Image>();
				if (image)
				{
					image.enabled = (a != 0);
					Color c = image.color;
					c.a = a;
					image.color = c;
				}
			}
		}

		public void Flash (int flashCount, float onTime, float offTime, bool finishVisible)
		{
			StartCoroutine(FlashCoroutine(flashCount, onTime, offTime, finishVisible));
		}

		IEnumerator FlashCoroutine (int flashCount, float onTime, float offTime, bool finishVisible)
		{
			for (int i=0; i<flashCount; i++)
			{
				SetAlpha (_initialAlpha);
				yield return new WaitForSeconds(onTime);
				SetAlpha (0);
				yield return new WaitForSeconds(offTime);
			}

			if (finishVisible)
				SetAlpha (_initialAlpha);
		}

		// scale the element up and back again
		public void Pulse (float maxScale, float time = 0.2f)
		{
			_timer = time;
			_effectPeriod = time;
			_pulseMaxScale = maxScale;
			_startScale = gameObject.transform.localScale.x;
			_effect = EffectType.Pulse;
		}

		// scale the element up and back again
		public void SineScale (float maxScale, float duration, float frequency)
		{
			float time = 0.0f;
			_timer = duration;
			_effectPeriod = frequency;
			_pulseMaxScale = maxScale;
			_startScale = gameObject.transform.localScale.x;
			_effect = EffectType.SineScale;
		}

		// scale the element up and back again
		public void SineFade (float maxAlpha, float minAlpha, float duration, float frequency)
		{
			float time = 0.0f;
			_timer = duration;
			_effectPeriod = frequency;
			_alphaRange = (maxAlpha - minAlpha)/2;
			_midAlpha = (maxAlpha + minAlpha)/2;
			_effect = EffectType.SineFade;
		}

		// scale the element up and back again
		public void ShrinkDown (float maxScale)
		{
			float time = 0.3f;
			_timer = time/3;
			_effectPeriod = time;
			_pulseMaxScale = maxScale;
			_startScale = gameObject.transform.localScale.x;
		}
		
		private void Update()
		{
			if (_timer > 0)
			{
				_timer -= Time.deltaTime;
				if (_timer < 0)
					_timer = 0;
				
				switch(_effect)
				{
				case EffectType.Pulse:
					float scale = _startScale * (1.0f + Mathf.Sin((Mathf.PI*_timer)/_effectPeriod)*_pulseMaxScale);
					gameObject.transform.localScale = new Vector3(scale,scale,scale);
					break;
				case EffectType.SineScale:
					float scale2 = _startScale * (1.0f + Mathf.Sin((Mathf.PI*_timer)/_effectPeriod)*_pulseMaxScale);
					gameObject.transform.localScale = new Vector3(scale2,scale2,scale2);
					break;
				case EffectType.SineFade:
					float alpha2 = _midAlpha + _alphaRange * Mathf.Sin((Mathf.PI*_timer)/_effectPeriod);
					SetAlpha(alpha2);
					break;
				case EffectType.Fade:
					float alpha = _endAlpha + (_timer/_effectPeriod)*(_startAlpha-_endAlpha);
					SetAlpha(alpha);
					break;
				case EffectType.PulseFade:
					// fade that overshoots
					float target = _pulseAlpha + (_pulseAlpha - _endAlpha);
					float alphaP = target + (_timer/_effectPeriod)*(_startAlpha-target);
					if (alphaP > _pulseAlpha)
						alphaP = 2*_pulseAlpha - alphaP;
					SetAlpha(alphaP);
					break;
				}
			}
		}
    }
