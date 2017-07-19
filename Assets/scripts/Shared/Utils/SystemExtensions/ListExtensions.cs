using System;
using System.Collections.Generic;

namespace Utils
{

	static class ListExtensions
	{
	public static void Shuffle<T>(this IList<T> list, int seed = 0)
		{
			Random rnd = seed > 0 ? new Random (seed) : new Random ();

			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k =rnd.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}