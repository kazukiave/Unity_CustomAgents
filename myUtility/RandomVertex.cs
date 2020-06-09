using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVertex : MonoBehaviour
{
    private Mesh mesh;
    public float max, min;
    Vector3[] iniVec;
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        iniVec = mesh.vertices;
    }

    // Update is called once per frame
    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;

        for (var i = 0; i < vertices.Length; i++)
        {
            if (i % 2 == 0)
            {
                vertices[i] = iniVec[i] + normals[i] * Random.value;
            }
            else
            {
                vertices[i] = iniVec[i] + normals[i] * -Random.value;
            }
            // vertices[i] += normals[i]  * Mathf.Sin(vertices[i].y + Time.deltaTime) * 0.1f ;
        }
     
        mesh.vertices = vertices;
        mesh.Clear();
        //mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

    private Vector3[] randomVertex(Mesh mesh, int min, int max)
    {
        var vtx = mesh.vertices;
        var normals = mesh.normals;

        Vector3[] rtnVtx = new Vector3[vtx.Length];

        for (int i = 0; i < vtx.Length; i++)
        {
            var rand = Random.Range(min, max);
            //vtx[i] += normals[i] * rand;
            vtx[i] += normals[i] * Mathf.Sin(Time.time) * 0.1f;
        }

        return rtnVtx;
    }
}
