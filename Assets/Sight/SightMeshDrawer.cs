using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SightMeshDrawer : MonoBehaviour
{
    [SerializeField] private Material material;
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

        drawer.AddComponent<MeshRenderer>().material = material;
    }

    public void UpdateVertices(List<Vector2> pointsClockwise)
    {
        mesh.Clear();
        int nPoints = pointsClockwise.Count;
        int nFaces = nPoints - 1;
        mesh.SetVertices(pointsClockwise.Select(v => (Vector3)v + Vector3.back).ToArray());

        List<int> triangles = new List<int>();
        for (int faceIndex = 1; faceIndex < nFaces; faceIndex++)
        {
            triangles.Add(faceIndex + 1);
            triangles.Add(faceIndex);
            triangles.Add(0);
        }
        mesh.triangles = triangles.ToArray();

        mesh.SetNormals(Enumerable.Repeat(Vector3.back, nPoints).ToArray());
        mesh.RecalculateBounds();
    }
}
