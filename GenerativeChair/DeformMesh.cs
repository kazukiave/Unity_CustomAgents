using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformMesh : MonoBehaviour
{
    public float moveVal;
    private MeshEdit meshEdit;
    private Mesh mesh;
    private Vector3[] initVtx;
    private MeshCollider meshCollider;

    // Start is called before the first frame update
    void Start()
    {
        meshEdit = GetComponent<MeshEdit>();
        mesh = transform.GetComponent<MeshFilter>().mesh;
        mesh = meshEdit.WeldVertices(mesh);
        initVtx = mesh.vertices;
        meshCollider = GetComponent<MeshCollider>();
    }

  
    public void Deform()
    {
        Destroy(meshCollider);
        meshEdit.RandomVertices(ref mesh, -moveVal, moveVal, initVtx);
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.tag = "Target";
        meshCollider.convex = true;
    }
}
