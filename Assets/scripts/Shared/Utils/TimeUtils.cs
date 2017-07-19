using UnityEngine;
using System.Collections;
using System;

namespace Utils
{
	public static class TimeUtils
	{
		public const int SECOND = 1000;
		public const int MINUTE = SECOND * 60;
		public const int HOUR = MINUTE * 60;
		public const int DAY = HOUR * 24;

//		public static string CalculateTimeSinceString(long eventTime)
//		{
//			long currentTime = Utils.Date.GetEpochTimeMills();
//			long timeDiff = currentTime - eventTime;
//
//			TimeSpan timeSpan = TimeSpan.FromMilliseconds(timeDiff);
//
//			string timeText = null;
//			if (timeSpan.Days > 0)
//			{
//				if (timeSpan.Days == 1)
//				{
//					timeText = string.Format(TextManager.Get("TimeUtils.TimeSinceDayAgo"), timeSpan.Days);
//				}
//				else
//				{
//					timeText = string.Format(TextManager.Get("TimeUtils.TimeSinceDaysAgo"), timeSpan.Days);
//				}
//			}
//			else if (timeSpan.Hours > 0)
//			{
//				if (timeSpan.Hours == 1)
//				{
//					timeText = string.Format(TextManager.Get("TimeUtils.TimeSinceHourAgo"), timeSpan.Hours);
//				}
//				else
//				{
//					timeText = string.Format(TextManager.Get("TimeUtils.TimeSinceHoursAgo"), timeSpan.Hours);
//				}
//			}
//			else if (timeSpan.Minutes > 0)
//			{
//				if (timeSpan.Minutes == 1)
//				{
//					timeText = string.Format(TextManager.Get("TimeUtils.TimeSinceMinuteAgo"), timeSpan.Minutes);
//				}
//				else
//				{
//					timeText = string.Format(TextManager.Get("TimeUtils.TimeSinceMinutesAgo"), timeSpan.Minutes);
//				}
//			}
//			else if (timeSpan.Seconds >= 0)
//			{
//				if (timeSpan.Seconds == 1)
//				{
//					timeText = string.Format(TextManager.Get("TimeUtils.TimeSinceSecondAgo"), timeSpan.Seconds);
//				}
//				else
//				{
//					timeText = string.Format(TextManager.Get("TimeUtils.TimeSinceSecondsAgo"), timeSpan.Seconds);
//				}
//			}
//
//			return timeText;
//		}

		public static string GetDateStringFromEpochTime(long eventTime)
		{
			System.DateTime publishedDate = new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(eventTime);

			string month = publishedDate.ToString("MMMM");
			string year = publishedDate.Year.ToString();

			int day = publishedDate.Day;
			string dayOfMonth = day.ToString();

			return dayOfMonth + " " + month + " " + year;
		}

//		public static string GetDescriptiveDateStringFromEpochTime(long eventTime)
//		{
//			// TODO: Expand this to the full range of descriptive date strings
//			// based on game design - for now only implements 'today'
//			System.DateTime publishedDate = new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(eventTime);
//
//			// Fallback to full date
//			string descriptiveDateString = GetDateStringFromEpochTime(eventTime);
//
//			if (publishedDate.Date == System.DateTime.Today)
//			{
//				descriptiveDateString = TextManager.Get("TimeUtils.Today");
//			}
//			
//			return descriptiveDateString;
//		}

		public static string GetDateStringAsNumberFormatFromEpochTime(long eventTime)
		{
			System.DateTime publishedDate = new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(eventTime);

			string month = publishedDate.ToString("MM");
			string year = publishedDate.Year.ToString();
			year = year.Substring(2, 2);

			int day = publishedDate.Day;
			string dayOfMonth = day.ToString();

			return dayOfMonth + "." + month + "." + year;
		}

		public static DateTime GetDateTimeFromEpochTime(long eventTime)
		{
			return new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(eventTime);
		}

		public static DayOfWeek GetDayOfTheWeek(int offset = 0)
		{
			int dayOfWeek = (int)System.DateTime.UtcNow.DayOfWeek + offset;

			if (dayOfWeek > (int)DayOfWeek.Saturday)
			{
				dayOfWeek = dayOfWeek - (int)DayOfWeek.Saturday - 1;
			}
			else if (dayOfWeek < (int)DayOfWeek.Sunday)
			{
				dayOfWeek = (int)DayOfWeek.Saturday - dayOfWeek;
			}

			return (DayOfWeek)dayOfWeek;
		}

		public static long ConvertDateTimeToEpochTime(DateTime date)
		{
			DateTime epoch = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long epochTime = Convert.ToInt64(((date - epoch).TotalMilliseconds));

			return epochTime;
		}
	}
}