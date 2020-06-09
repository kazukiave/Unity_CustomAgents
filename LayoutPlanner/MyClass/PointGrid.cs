using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PointGrid : MonoBehaviour
{
    private GameObject pointGrid;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    public float GridSize { get; set; }
    public float SizeX {get; set;}
    public float SizeY { get; set; }
    public int ExtentX { get; set; }
    public int ExtentY { get; set; }

    public enum Axis
    { 
        Up = 0,
        Right,
        Down,
        Left,
    }

    public List<Vector2> points { get; set; }
    public List<GameObject> spheres { get; set; }
    public  PointGrid()
    {
        Init();
    }

    public PointGrid(float gridSize, int extentX, int extentY)
    {
        GridSize = gridSize;
        ExtentX = extentX;
        ExtentY = extentY;

        Init();

        for (int i = 0; i < extentX; i++)
        {
            for (int j = 0; j < extentY; j++)
            {
                var pt = new Vector2((gridSize * i), (gridSize * j));

                points.Add(pt);
            }
        }
    }

    public void ShapedGrid(float offsetOut, float offsetIn, int reduceNum)
    {
        var min = new Vector2(offsetOut, offsetOut);

        var randX = Random.Range(min.x, points.Max().x);
        var randY = Random.Range(min.y, points.Max().y);
        var pt = new Vector2(randX, randY);


    }

    private void Init()
    {
        spheres = new List<GameObject>();
        points = new List<Vector2>();
        pointGrid = new GameObject("PointGrid");
    }

    private List<Vector2> Outline()
    {
        var rtnList = points.Where(pt => GetNeighbor(pt).Count != 4).ToList();
        return rtnList;
    }

    private Vector2 GetUp(Vector2 current)
    {
        return new Vector2(current.x, current.y + GridSize);
    }

    private Vector2 GetDown(Vector2 current)
    {
        return new Vector2(current.x, current.y - GridSize);
    }

    private Vector2 GetRight(Vector2 current)
    {
        return new Vector2(current.x + GridSize, current.y);
    }

    private Vector2 GetLeft(Vector2 current)
    {
        return new Vector2(current.x - GridSize, current.y);
    }

    private List<Vector2> GetNeighbor(Vector2 current)
    {
        var rtnList = new List<Vector2>();

        var up = GetUp(current);
        var right = GetRight(current);
        var down = GetDown(current);
        var left = GetLeft(current);

        if (points.Contains(up))
        {
            rtnList.Add(up);
        }

        if (points.Contains(right))
        {
            rtnList.Add(right);
        }

        if (points.Contains(down))
        {
            rtnList.Add(down);
        }

        if (points.Contains(left))
        {
            rtnList.Add(left);
        }
        return rtnList;
    }

    private int GetIndex(Vector2 searchPt)
    {
        int idx = -1;

        for(int i = 0; i < points.Count; i++)
        {
            if (points[i] == searchPt)
            {
                idx = i;
                break;
            }
        }

        return idx;
    }
    

    private void CreatePoints()
    { 
        
    }

    public void Show()
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] == null) continue;
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = points[i];
            sphere.name = i.ToString();
            spheres.Add(sphere);
        }
    }

    /*
    public void ShowAsTopology()
    {

        if(meshRenderer == null || meshFilter == null)
        {
            meshRenderer = pointGrid.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshFilter = pointGrid.AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
        }

        var idx = new int[points.Count];
        var cols = new Color[points.Count];
        for(int i= 0; i < idx.Length; i++)
        {
            idx[i] = i;
            cols[i] = Color.red;
        }

        var mesh = new Mesh();
        meshFilter.mesh.vertices = points.ToArray();
        meshFilter.mesh.colors = cols;


        meshFilter.mesh.vertices = points.ToArray();
        meshFilter.mesh.SetIndices(idx, MeshTopology.Points , 0);
    }
    */
}
