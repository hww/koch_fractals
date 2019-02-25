using System.Collections.Generic;
using UnityEngine;

namespace VARP.KochFractals
{
    public static class BezierCurve
    {
        public static Vector3[] Generate(Vector3[] points, int vertexCount)
        {
            var pointsList = new List<Vector3>();
            for (var i = 0; i < points.Length; i += 2)
            {
                if (i + 2 <= points.Length - 1)
                {
                    for (float ratio = 0f; ratio <= 1f; ratio += 1f / vertexCount)
                    {
                        var tangent1 = Vector3.Lerp(points[i], points[i + 1], ratio);
                        var tangent2 = Vector3.Lerp(points[i + 1], points[i + 2], ratio);
                        var bezierPoint = Vector3.Lerp(tangent1, tangent2, ratio);
                        pointsList.Add(bezierPoint);
                    }
                }
            }
            return pointsList.ToArray();
        }
    }
}