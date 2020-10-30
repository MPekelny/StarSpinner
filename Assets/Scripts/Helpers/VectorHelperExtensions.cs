using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorHelperExtentions
{
    public static Vector3 With(this Vector3 vector, float? nX = null, float? nY = null, float? nZ = null)
	{
		return new Vector3(nX != null ? nX.Value : vector.x,
							nY != null ? nY.Value : vector.y,
							nZ != null ? nZ.Value : vector.z);
	}
}
