using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SightMeshDrawer))]
public class Sight : MonoBehaviour
{
    [SerializeField] private LayerMask seeableLayers;

    // for nice inspector
    [SerializeField][Range(0, 2 * Mathf.PI + 0.1f)] private float _directionInRadiansClockwiseFromUp = 0;
    [SerializeField][Range(0, 2 * Mathf.PI + 0.1f)] private float _spreadInRadians = Mathf.PI * 2;
    private float directionInRadiansClockwiseFromUp => (float)NormalizeAngle(_directionInRadiansClockwiseFromUp);
    private float spreadInRadians => Mathf.Clamp(_spreadInRadians, 0, Mathf.PI * 2);

    [SerializeField] private float radius = 8;
    [SerializeField] private float offsetDistanceFromVertices = 0.01f;
    [SerializeField] private int uniformRays = 16;
    private HashSet<GameObject> lastSeen = new HashSet<GameObject>();
    private List<Vector2> sightShape = new List<Vector2>();
    private SightMeshDrawer meshDrawer;

    private void Awake()
    {
        meshDrawer = GetComponent<SightMeshDrawer>();
    }

    public IEnumerable<GameObject> GetSeenObjects()
    {
        return lastSeen;
    }

    public void SetDirection(float radiansClockwiseFromUp)
    {
        _directionInRadiansClockwiseFromUp = radiansClockwiseFromUp;
    }

    public void SetDirection(Vector2 dir)
    {
        SetDirection(-Vector2.SignedAngle(Vector2.up, dir) * Mathf.Deg2Rad);
    }

    public void SetDirectionByTarget(Vector2 target)
    {
        SetDirection(target - (Vector2)transform.position);
    }

    public void SetSpread(float radians)
    {
        _spreadInRadians = radians;
    }

    private void Update()
    {
        Recalculate();
        meshDrawer.UpdateVertices(sightShape);
    }

    /// <summary>
    /// Get the shape of the sight vertices clockwise.
    /// The first item is the ray orign. 
    /// </summary>
    public void Recalculate()
    {
        lastSeen.Clear();
        sightShape.Clear();
        List<(Vector2, float)> verticesAndAngles = new List<(Vector2, float)>(); // point, angle to point

        void DoRaycastAtDirection(Vector2 dir)
        {
            verticesAndAngles.Add((DoRaycast(dir), GetAngleRelativeToLookDir(dir)));
        }

        void DoRaycastAtAngle(float angle)
        {
            Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            verticesAndAngles.Add((DoRaycast(dir), GetAngleRelativeToLookDir(dir)));
        }

        Vector2 lookDirection = new Vector2(Mathf.Sin(directionInRadiansClockwiseFromUp), Mathf.Cos(directionInRadiansClockwiseFromUp));
        float GetAngleRelativeToLookDir(Vector2 dir)
        {
            return Mathf.Deg2Rad * Vector2.SignedAngle(lookDirection, dir);
        }

        // per vertex rays
        foreach (Vector2 vertex in GetVerticesToCheck())
        {
            if (!IsInSightDirection(vertex)) continue;

            Vector2 positionChange = vertex - (Vector2)transform.position;
            Vector2 dir = positionChange.normalized;
            Vector2 perpindicular = new Vector2(dir.y, -dir.x);
            Vector2 offset = perpindicular * offsetDistanceFromVertices;

            DoRaycastAtDirection(positionChange + offset);
            DoRaycastAtDirection(positionChange);
            DoRaycastAtDirection(positionChange - offset);
        }

        // uniform rays
        for (int i = 0; i < uniformRays; i++)
        {
            float angle = Mathf.PI * 2 * i / uniformRays;
            if (!IsInSightDirection(angle)) continue;
            DoRaycastAtAngle(angle);
        }

        // sight spread rays
        bool canSeeAllAngles = spreadInRadians >= Mathf.PI * 2;
        if (!canSeeAllAngles)
        {
            DoRaycastAtAngle(directionInRadiansClockwiseFromUp - spreadInRadians / 2);
            DoRaycastAtAngle(directionInRadiansClockwiseFromUp + spreadInRadians / 2);
        }

        verticesAndAngles.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        List<Vector2> vertices = verticesAndAngles.Select(item => item.Item1).ToList();
        if (canSeeAllAngles) vertices.Add(vertices[0]);
        vertices.Insert(0, transform.position);
        sightShape = vertices;
    }

    private bool IsInSightDirection(Vector2 point)
    {
        return IsInSightDirection(-Mathf.Deg2Rad * Vector2.SignedAngle(Vector2.up, point - (Vector2)transform.position));
    }

    private bool IsInSightDirection(float angleTo)
    {
        if (spreadInRadians >= Mathf.PI * 2) return true;
        return IsAngleBetween(angleTo, directionInRadiansClockwiseFromUp - spreadInRadians / 2, directionInRadiansClockwiseFromUp + spreadInRadians / 2);
    }

    bool IsAngleBetween(double angle, double min, double max)
    {
        // Normalize the angle to the range [0, 2π)
        angle = NormalizeAngle(angle);
        min = NormalizeAngle(min);
        max = NormalizeAngle(max);

        if (min == max) return angle == min;
        if (min < max) return angle >= min && angle <= max;

        // Handle the wraparound case where min > max (crossing 2π)
        return angle >= min || angle <= max;
    }

    double NormalizeAngle(double angle)
    {
        angle = angle % (2 * Mathf.PI);
        if (angle < 0) angle += 2 * Mathf.PI;
        return angle;
    }

    private Vector2 DoRaycast(Vector2 dir)
    {
        dir = dir.normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, radius, seeableLayers);
        if (hit.collider is null)
        {
            return (Vector2)transform.position + dir * radius;
        }
        lastSeen.Add(hit.collider.gameObject);
        return hit.point;
    }

    private IEnumerable<Vector2> GetVerticesToCheck()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, seeableLayers);

        foreach (Collider2D collider in colliders)
        {
            PhysicsShapeGroup2D physicsShapeGroup = new PhysicsShapeGroup2D();
            collider.GetShapes(physicsShapeGroup);

            for (int shapeIndex = 0; shapeIndex < physicsShapeGroup.shapeCount; shapeIndex++)
            {
                List<Vector2> vertices = new List<Vector2>();
                physicsShapeGroup.GetShapeVertices(shapeIndex, vertices);

                foreach (Vector2 vertex in vertices)
                {
                    yield return vertex;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (sightShape is null) return;
        foreach (Vector2 vertex in sightShape)
        {
            Gizmos.DrawSphere(vertex, 0.01f);
        }
    }
}
