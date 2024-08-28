using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SightMeshDrawer : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh mesh;

    private void Start()
    {
        GameObject drawer = new GameObject();
        drawer.name = "MeshDrawer";

        meshFilter = drawer.AddComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "MeshDrawer Auto Created Mesh";
        meshFilter.mesh = mesh;

        drawer.AddComponent<MeshRenderer>();
    }

    public void UpdateVertices(List<Vector2> pointsClockwise)
    {
        int nFaces = pointsClockwise.Count - 1;
        mesh.vertices = pointsClockwise.Select(v => (Vector3)v + Vector3.back).ToArray();

        List<int> triangles = new List<int>();
        for (int faceIndex = 1; faceIndex < nFaces; faceIndex++)
        {
            triangles.Add(faceIndex + 1);
            triangles.Add(faceIndex);
            triangles.Add(0);
        }

        mesh.normals = Enumerable.Repeat(Vector3.back, pointsClockwise.Count).ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
    }
}
