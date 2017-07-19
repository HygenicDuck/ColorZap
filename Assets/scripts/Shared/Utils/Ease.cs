using UnityEngine;
using System.Collections;

public class Ease {

	public enum Type
	{
		CUBIC_IN,
		CUBIC_OUT,
		LINEAR
	}

	public delegate float EaseFunction(float time, float startValue, float increasedValue, float duration);

	public static EaseFunction GetEase(Type type)
	{
		switch(type)
		{
		case Type.CUBIC_IN:
			return EaseInCubic;
		case Type.CUBIC_OUT:
			return EaseOutCubic;
		case Type.LINEAR:
			return EaseLinear;

		}
		return EaseLinear;
	}

	public static float EaseInCubic(float time, float startValue, float increasedValue, float duration)
	{
		if (duration == 0)
		{
			return increasedValue + startValue;
		}
		time /= duration;
		return increasedValue*(time*time*time) + startValue;
	}

	public static float EaseOutCubic(float time, float startValue, float increasedValue, float duration)
	{
		if (duration == 0)
		{
			return increasedValue + startValue;
		}
		time /= duration;
		time--;
		return increasedValue*(time*time*time + 1) + startValue;
	}


	public static float EaseLinear(float time, float startValue, float increasedValue, float duration)
	{
		if (duration == 0)
		{
			return increasedValue + startValue;
		}
		time /= duration;
		return increasedValue*(time) + startValue;
	}
		
}
