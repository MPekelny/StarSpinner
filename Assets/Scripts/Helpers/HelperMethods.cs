using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
	private const float EPSILON_FLOAT = 0.00001f;

	/// <summary>
	/// Makes a Color with its red, green and blue components a random value between 0 and 1.
	/// </summary>
	/// <returns></returns>
	public static Color MakeRandomColor()
	{
		return new Color(UnityEngine.Random.Range(0f, 1f),
						 UnityEngine.Random.Range(0f, 1f),
						 UnityEngine.Random.Range(0f, 1f));
	}

	/// <summary>
	/// Gets if two floats are very, very close in value. Essentially an equality check that factors in floating point errors.
	/// </summary>
	public static bool EpsilonCheck(float a, float b)
	{
		return Mathf.Abs(a - b) <= EPSILON_FLOAT;
	}
}
