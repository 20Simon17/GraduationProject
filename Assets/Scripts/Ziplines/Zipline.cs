using System.Collections.Generic;
using UnityEngine;

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
        mesh.name = "Zipline";

        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();
        
        Vector3 sP = startPoint.AttachLocation; //startPoint
        Vector3 eP = endPoint.AttachLocation;   //endPoint
        
        Vector3 direction = (eP - sP).normalized;
        Vector3 rightVector = -Vector3.Cross(direction, Vector3.up);
        Vector3 upVector = Vector3.Cross(direction, rightVector);

        float meshSize = 0.05f;
        
        vertices.AddRange(new Vector3[]
        {
            sP - rightVector * meshSize - upVector * meshSize, //0
            sP - rightVector * meshSize + upVector * meshSize, //1
            sP + rightVector * meshSize + upVector * meshSize, //2
            sP + rightVector * meshSize - upVector * meshSize, //3
            
            eP - rightVector * meshSize - upVector * meshSize, //4
            eP - rightVector * meshSize + upVector * meshSize, //5
            eP + rightVector * meshSize + upVector * meshSize, //6
            eP + rightVector * meshSize - upVector * meshSize  //7
        });
        
        colors.AddRange(new Color[vertices.Count]);
        
        for (int i = 0; i < vertices.Count; i++) 
            colors[i] = Color.red;
        
        triangles.AddRange(new int[]
        {
            //0, 1, 3, 3, 1, 2, //front
            4, 5, 1, 1, 0, 4, //left
            1, 5, 2, 2, 5, 6, //top
            3, 2, 7, 7, 2, 6, //right
            0, 3, 4, 3, 7, 4 //bottom
            //6, 5, 7, 7, 5, 4  //back
        });
        
        // assign the mesh data
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // TODO: Set the material of the mesh renderer to a zipline material
        
        return mesh;
    }
}