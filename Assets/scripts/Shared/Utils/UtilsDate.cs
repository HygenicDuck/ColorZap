using UnityEngine;
using System.Collections;
using System;

namespace Utils
{
	public class Date
	{
		public static long GetEpochTimeMills()
		{
			DateTime epochStart = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			return (long)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;
		}

		public static long GetTimeMills(int year, int month, int day)
		{
			DateTime epochStart = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime epochTime = new DateTime (year, month, day, 0, 0, 1, DateTimeKind.Utc);

			if (epochTime > epochStart)
			{
				return (long)(epochTime - epochStart).TotalMilliseconds;
			}
			else
			{
				return 0;
			}
		}

		public static long GetEpochTime()
		{
			return (long)(((float)GetEpochTimeMills()) / 1000.0);
		}
	}
}
