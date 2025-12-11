using System;
using UnityEngine;

public class ZiplinePolePoint : ZiplinePoint
{
    public override Vector3[] GetAttachmentVerts()
    {
        return Array.Empty<Vector3>();
    }
}