using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorHelperExtensions
{
	public static Color With(this Color c, float? nR = null, float? nG = null, float? nB = null, float? nA = null)
	{
		return new Color(nR != null ? nR.Value : c.r,
						 nG != null ? nG.Value : c.g,
						 nB != null ? nB.Value : c.b,
						 nA != null ? nA.Value : c.a);
	}
}
