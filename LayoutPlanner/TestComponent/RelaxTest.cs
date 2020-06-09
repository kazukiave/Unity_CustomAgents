using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using myRhinoWrapper;
using Rhino;
using Rhino.Geometry;
using RhinoInside.Unity;

public class RelaxTest : MonoBehaviour
{
    List<Point3d> pts;
    List<GameObject> spheres;
    Rectangle3d rect;
    Rectangle3d region;
    Relax _relax;
    public int numSphere;
    // Start is called before the first frame update
    void Start()
    {
        var plane = new Rhino.Geometry.Plane(Point3d.Origin, Vector3d.ZAxis);
        var interval = new Interval(-0.5, 0.5);
        rect = new Rectangle3d(plane, interval, interval);
        var intervalR = new Interval(-5, 5);
        region = new Rectangle3d(plane, intervalR, intervalR);
        pts = RhinoWrapper.RandomPt(rect, numSphere);

        _relax = gameObject.AddComponent<Relax>();
        spheres = new List<GameObject>();

        var col = new Color(0.5f, 0, 0, 0.01f);
        for (int i = 0; i < pts.Count; i++)
        {
            var sphere =  GameObject.CreatePrimitive(PrimitiveType.Quad);
            sphere.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(1f ,1f, 1f, 1f, 0,0) ;
            spheres.Add(sphere);
        }

       
        RhinoPreview.PolyLineShow(region.ToPolyline(), col, 0.3f);
    }

    int count = 0;
    // Update is called once per frame
    void Update()
    {
        count++;
        if (count > 7)
        {
            pts.RemoveAt(0);
            pts.Add(RhinoWrapper.RandomPt(rect, 1)[0]);
            count = 0;
        }
        _relax.Compute(ref pts, 2, region.ToPolyline(), 0.05);
        var pos = pts.ToHost();
        
        for(int i = 0; i < pos.Count; i++)
        {
            spheres[i].transform.position = pos[i];
        }
    }
}
