using UnityEngine;

public class MoveThroughWayPoints : MonoBehaviour
{
    [SerializeField] private Transform waypointContanier;
    [SerializeField] private float speed = 1;
    [SerializeField] private float lerpSpeed = 1;
    private int lastWaypointIndex = 0;
    private int nextWaypointIndex => (lastWaypointIndex + 1) % waypointContanier.childCount;
    private float intraWaypointT = 0;
    private Vector3 lastWaypoint => waypointContanier.GetChild(lastWaypointIndex % waypointContanier.childCount).position;
    private Vector3 nextWaypoint => waypointContanier.GetChild(nextWaypointIndex).position;

    void Start()
    {

    }

    void Update()
    {
        UpdateDistance();
        Vector3 targetPosition = Vector3.Lerp(lastWaypoint, nextWaypoint, intraWaypointT);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * lerpSpeed);
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
