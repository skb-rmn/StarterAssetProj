using UnityEngine;
using Color = UnityEngine.Color;

public static class DebugShapes
{
    /// <summary>
    /// Draws a wireframe sphere in the Game view using Debug.DrawLine.
    /// </summary>
    /// <param name="center">World‐space position of the sphere’s center.</param>
    /// <param name="radius">Radius of the sphere in world units.</param>
    /// <param name="color">Line color.</param>
    /// <param name="duration">
    /// How long each line segment remains visible (seconds). 
    /// Zero = one frame.
    /// </param>
    /// <param name="depthTest">
    /// Whether lines are obscured by geometry closer to the camera.
    /// </param>
    /// <param name="segments">
    /// Number of line segments per circle (higher = smoother).
    /// </param>
    public static void DebugDrawSphere(
        Vector3 center,
        float radius,
        Color color,
        float duration = 0f,
        bool depthTest = true,
        int segments = 24
    )
    {
        // Clamp segments to at least 4
        segments = Mathf.Max(4, segments);
        float angleStep = 360f / segments;

        // Draw circles in X‑Z, Y‑Z, and X‑Y planes
        for (int plane = 0; plane < 3; plane++)
        {
            Vector3 prevPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;

                // Pick plane: 0 = XZ, 1 = YZ, 2 = XY
                Vector3 point;
                switch (plane)
                {
                    case 0: point = new Vector3(x, 0f, y); break;
                    case 1: point = new Vector3(0f, x, y); break;
                    default: point = new Vector3(x, y, 0f); break;
                }

                point += center;

                if (i > 0)
                {
                    Debug.DrawLine(prevPoint, point, color, duration, depthTest);
                }
                else
                {
                    firstPoint = point;
                }

                prevPoint = point;
            }

            // Close the loop
            Debug.DrawLine(prevPoint, firstPoint, color, duration, depthTest);
        }
    }
}
