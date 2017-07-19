using System;
using UnityEngine;
using UnityEngine.UI;

public struct IntVec2
{
	public int x,y;

	public IntVec2(int ix, int iy)
	{
		x = ix;
		y = iy;
	}

	public static IntVec2 operator +(IntVec2 c1, IntVec2 c2) 
	{
		return new IntVec2(c1.x + c2.x, c1.y + c2.y);
	}
}
