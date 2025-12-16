using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
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
        List<int> zipTriangles = new List<int>();
        List<int> colTriangles = new List<int>();
        
        Vector3 sP = startPoint.AttachLocation; //startPoint
        Vector3 eP = endPoint.AttachLocation;   //endPoint
        
        Vector3 direction = (eP - sP).normalized;
        Vector3 rightVector = -Vector3.Cross(direction, Vector3.up);
        Vector3 upVector = Vector3.Cross(direction, rightVector);

        float meshSize = 0.05f;
        float colSize = 0.4f;
        
        vertices.AddRange(new Vector3[]
        {
            sP - rightVector * meshSize - upVector * meshSize,  //0
            sP - rightVector * meshSize + upVector * meshSize,  //1
            sP + rightVector * meshSize + upVector * meshSize,  //2
            sP + rightVector * meshSize - upVector * meshSize,  //3
            
            eP - rightVector * meshSize - upVector * meshSize,  //4
            eP - rightVector * meshSize + upVector * meshSize,  //5
            eP + rightVector * meshSize + upVector * meshSize,  //6
            eP + rightVector * meshSize - upVector * meshSize,  //7
            
            sP - rightVector * colSize - upVector * colSize,    //8
            sP - rightVector * colSize + upVector * colSize,    //9
            sP + rightVector * colSize + upVector * colSize,    //10
            sP + rightVector * colSize - upVector * colSize,    //11
            
            eP - rightVector * colSize - upVector * colSize,    //12
            eP - rightVector * colSize + upVector * colSize,    //13
            eP + rightVector * colSize + upVector * colSize,    //14
            eP + rightVector * colSize - upVector * colSize     //15
        });
        
        colors.AddRange(new Color[vertices.Count]);
        
        for (int i = 0; i < vertices.Count; i++) 
            colors[i] = Color.red;
        
        zipTriangles.AddRange(new int[]
        {
            //0, 1, 3, 3, 1, 2, //front
            4, 5, 1, 1, 0, 4,   //left
            1, 5, 2, 2, 5, 6,   //top
            3, 2, 7, 7, 2, 6,   //right
            0, 3, 4, 3, 7, 4    //bottom
            //6, 5, 7, 7, 5, 4  //back
        });
        
        colTriangles.AddRange(new int[]
        {
            //8, 9, 11, 11, 9, 10,      //front
            12, 13, 9, 9, 8, 12,        //left
            9, 13, 10, 10, 13, 14,      //top
            11, 10, 15, 15, 10, 14,     //right
            8, 11, 12, 11, 15, 12       //bottom
            //14, 13, 15, 15, 13, 12    //back
        });
        
        // assign the mesh data
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        
        mesh.subMeshCount = 2;
        mesh.SetTriangles(zipTriangles, 0);
        mesh.SetTriangles(colTriangles, 1);
        
        // TODO: Somehow generate the hitbox here

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // TODO: Set the material of the mesh renderer to a zipline material
        
        return mesh;
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (endPoint is null || startPoint is null) return;
        
        if (endPoint.transform.hasChanged || startPoint.transform.hasChanged)
        {
            UpdateMesh();
        }
        #endif
    }
}