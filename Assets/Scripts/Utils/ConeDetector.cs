using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ConeDetector : MonoBehaviour
{
    [Header("Cone Settings")]
    public float range = 10f;
    [Range(0f, 180f)]
    public float angle = 60f;
    public LayerMask layerMask = ~0;
    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal;

    /// <summary>
    /// Returns all colliders within the cone, sorted by ascending distance.
    /// </summary>
    public Collider[] GetTargetsInCone()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = angle * 0.5f;

        // 1) Broad phase: sphere overlap
        var allHits = Physics.OverlapSphere(origin, range, layerMask, triggerInteraction);
        // 2) Narrow phase: angle check (cone)
        var inCone = allHits
            .Where(col => Vector3.Angle(forward, col.transform.position - origin) <= halfAngle);
        // 3) Sort by distance
        return inCone
            .OrderBy(col => Vector3.Distance(origin, col.transform.position))
            .ToArray();
    }

    /// <summary>
    /// Returns the single "best" target:
    /// - If any colliders are inside the cone, picks the one most directly forward (smallest angle),
    ///   breaking ties by closest distance.
    /// - Otherwise, picks the collider with the smallest angle to forward.
    /// </summary>
    public Collider GetBestTargetInRange()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = angle * 0.5f;

        // Broad-phase: gather all colliders within range
        Collider[] allHits = Physics.OverlapSphere(
            origin, range, layerMask, triggerInteraction
        );

        // Narrow-phase: filter in-cone colliders
        var inCone = allHits
            .Where(c =>
                Vector3.Angle(forward, c.transform.position - origin) <= halfAngle
            );

        // If any are in the cone, prioritize most forward (angle), then closest (distance)
        if (inCone.Any())
        {
            return inCone
                .OrderBy(c =>
                    Vector3.Angle(forward, c.transform.position - origin)
                )
                .ThenBy(c =>
                    Vector3.Distance(origin, c.transform.position)
                )
                .First();
        }

        // Fallback: pick the single collider with smallest angle to forward
        return allHits
            .OrderBy(c =>
                Vector3.Angle(forward, c.transform.position - origin)
            )
            .FirstOrDefault();
    }

    // Optional: draw gizmos to visualize cone and best target
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = angle * 0.5f;

        // Draw sphere boundary
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, range);

        // Draw cone edges
        Quaternion leftRot = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(halfAngle, Vector3.up);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + leftRot * forward * range);
        Gizmos.DrawLine(origin, origin + rightRot * forward * range);

        // Highlight best target
        Collider best = GetBestTargetInRange();
        if (best != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(origin, best.transform.position);
        }
    }
}
