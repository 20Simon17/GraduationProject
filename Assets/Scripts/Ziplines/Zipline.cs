using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class Zipline : ProceduralMesh, IInteractable
{
    public ZiplinePoint startPoint;
    public ZiplinePoint endPoint;

    public void Interact(GameObject interactor)
    {
        //TODO: Implement zipline interaction
    }

    protected override Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.DontSave;
        mesh.name = "Tracks";

        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();
        
        Vector3 direction = (endPoint.AttachLocation - startPoint.AttachLocation).normalized;

        Vector3 crossProduct = Vector3.Cross(direction, Vector3.up);
        Debug.Log("Ziplines right vector:" + crossProduct);
        
        Debug.DrawRay(direction * Vector3.Distance(startPoint.AttachLocation, endPoint.AttachLocation) / 2, crossProduct.normalized, Color.red, 10f);
        
        //TODO: Find the zipline's right and up vector
        // calculate the 4 vertex points from both the start and the end using the right and up vectors * size
        // generate the zipline mesh triangles between the 8 vertices
        
        colors.AddRange(new Color[vertices.Count]);
        
        for (int i = 0; i < vertices.Count; i++) 
            colors[i] = Color.black;
        
        /*triangles.AddRange(new int[]
        {
            0, 1, 3, 3, 1, 2,
            4, 5, 1, 1, 0, 4,
            1, 5, 2, 2, 5, 6,
            3, 2, 7, 7, 2, 6,
            3, 0, 4, 4, 7, 3,
            6, 5, 7, 7, 5, 4
        });*/
        
        // assign the mesh data
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}