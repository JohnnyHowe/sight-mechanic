using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sight : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private MeshDrawer meshDrawer;
    [SerializeField] private float radius = 8;
    [SerializeField] private float offsetDistanceFromVertices = 0.01f;
    [SerializeField] private int uniformRays = 16;

    void Update()
    {
        meshDrawer.UpdateVertices(GetSightShapeVertices());
    }

    /// <summary>
    /// Get the shape of the sight vertices clockwise.
    /// The first item is the ray orign. 
    /// </summary>
    private List<Vector2> GetSightShapeVertices()
    {
        List<(Vector2, float)> verticesAndAngles = new List<(Vector2, float)>(); // point, angle to point
        foreach (Vector2 vertex in GetVerticesToCheck())
        {
            Vector2 positionChange = vertex - (Vector2)transform.position;
            Vector2 dir = positionChange.normalized;
            Vector2 perpindicular = new Vector2(dir.y, -dir.x);
            Vector2 offset = perpindicular * offsetDistanceFromVertices;

            verticesAndAngles.Add((DoRaycast(positionChange + offset), Mathf.Deg2Rad * Vector2.SignedAngle(Vector2.up, positionChange + offset)));
            verticesAndAngles.Add((DoRaycast(positionChange), Mathf.Deg2Rad * Vector2.SignedAngle(Vector2.up, positionChange)));
            verticesAndAngles.Add((DoRaycast(positionChange - offset), Mathf.Deg2Rad * Vector2.SignedAngle(Vector2.up, positionChange - offset)));
        }

        for (int i = 0; i < uniformRays; i++)
        {
            float angle = Mathf.PI * 2 * i / uniformRays;
            Vector2 dir = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
            verticesAndAngles.Add((DoRaycast(dir), Mathf.Deg2Rad * Vector2.SignedAngle(Vector2.up, dir)));
        }

        verticesAndAngles.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        List<Vector2> vertices = verticesAndAngles.Select(item => item.Item1).ToList();
        vertices.Add(vertices[0]);
        vertices.Insert(0, transform.position);
        return vertices;
    }

    private Vector2 DoRaycast(Vector2 dir)
    {
        dir = dir.normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, radius);
        if (hit.collider is null)
        {
            Debug.DrawLine(transform.position, (Vector2)transform.position + dir * radius, Color.red);
            return (Vector2)transform.position + dir * radius;
        }
        Debug.DrawLine(transform.position, hit.point, Color.white);
        return hit.point;
    }

    private IEnumerable<Vector2> GetVerticesToCheck()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, layerMask);

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
}
