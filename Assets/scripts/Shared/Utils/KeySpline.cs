using UnityEngine;
using System.Collections;
using Utils;

namespace Utils
{

	/*
	Relates to the curve created on http://cubic-bezier.com
	*/

	public class KeySpline
	{

		private const int REQUIRED_POINTS_ARRAY_SIZE = 4;

		private float m_x1;
		private float m_y1;
		private float m_x2;
		private float m_y2;

		public KeySpline(float[] pointsArray)
		{
			Debugger.Assert(pointsArray != null && pointsArray.Length == REQUIRED_POINTS_ARRAY_SIZE, "Points array is invalid");
			if (pointsArray != null && pointsArray.Length == REQUIRED_POINTS_ARRAY_SIZE)
			{
				m_x1 = pointsArray[0];
				m_y1 = pointsArray[1];
				m_x2 = pointsArray[2];
				m_y2 = pointsArray[3];
			}
		}
		
		public float GetValue(float aX)
		{
			if (m_x1 == m_y1 && m_x2 == m_y2)
			{
				return aX; // linear
			}

			return CalcBezier(GetTForX(aX), m_y1, m_y2);
		}

		private float CalcBezier(float aT, float aA1, float aA2)
		{
			return ((((A(aA1, aA2) * aT) + B(aA1, aA2)) * aT) + C(aA1)) * aT;
		}

		private float A(float aA1, float aA2)
		{
			return 1f - (3f * aA2) + (3f * aA1);
		}

		private float B(float aA1, float aA2)
		{
			return (3f * aA2) - (6f * aA1);
		}

		private float C(float aA1)
		{
			return (3f * aA1);
		}

		private float GetSlope(float aT, float aA1, float aA2)
		{
			return (3f * A(aA1, aA2) * aT * aT) + (2f * B(aA1, aA2) * aT) + C(aA1);
		}

		private float GetTForX(float aX)
		{
			float aGuessT = aX;
			for (int i = 0; i < REQUIRED_POINTS_ARRAY_SIZE; ++i)
			{
				float currentSlope = GetSlope(aGuessT, m_x1, m_x2);
				if (currentSlope == 0.0)
				{
					return aGuessT;
				}
				float currentX = CalcBezier(aGuessT, m_x1, m_x2) - aX;
				aGuessT -= currentX / currentSlope;
			}
			return aGuessT;
		}

	}
}