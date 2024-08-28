using System;
using UnityEngine;

public class MoveThroughWayPoints : MonoBehaviour
{
    [SerializeField] private Sight sight;
    [SerializeField] private Transform waypointContanier;
    [SerializeField] private float speed = 1;
    [SerializeField] private float lerpSpeed = 1;
    [SerializeField][Range(0, 2 * Mathf.PI + 0.1f)] private float minSpread = 1;
    [SerializeField][Range(0, 2 * Mathf.PI + 0.1f)] private float maxSpread = 1.5f * Mathf.PI;
    [SerializeField] private float spreadChangePeriod = 2f;
    private int lastWaypointIndex = 0;
    private int nextWaypointIndex => (lastWaypointIndex + 1) % waypointContanier.childCount;
    private float intraWaypointT = 0;
    private Vector3 lastWaypoint => waypointContanier.GetChild(lastWaypointIndex % waypointContanier.childCount).position;
    private Vector3 nextWaypoint => waypointContanier.GetChild(nextWaypointIndex).position;
    private Vector2 lastPoint;

    void Update()
    {
        UpdateDistance();
        Vector3 targetPosition = Vector3.Lerp(lastWaypoint, nextWaypoint, intraWaypointT);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * lerpSpeed);

        sight.SetDirection(-Mathf.Deg2Rad * Vector2.SignedAngle(Vector2.up, (Vector2)transform.position - lastPoint));

        float spreadT = (Mathf.Sin(2 * Mathf.PI * Time.time / spreadChangePeriod) + 1) / 2;
        sight.SetSpread(Mathf.Lerp(minSpread, maxSpread, spreadT));

        lastPoint = transform.position;
    }

    void UpdateDistance()
    {
        lastWaypointIndex = lastWaypointIndex % waypointContanier.childCount;
        float currentSectionDistance = (lastWaypoint - nextWaypoint).magnitude;

        intraWaypointT += Time.deltaTime * speed / currentSectionDistance;
        while (intraWaypointT > 1)
        {
            intraWaypointT -= 1;
            lastWaypointIndex += 1;
        }
    }
}
