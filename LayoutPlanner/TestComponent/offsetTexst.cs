using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino.Geometry;
using System.Linq;
using myRhinoWrapper;
using RhinoInside.Unity;


public class offsetTexst : MonoBehaviour
{
    public GameObject obj;

    [Header("Property")]
    [System.NonSerialized]
    public int gridSize = 1000;
    [System.NonSerialized]
    public int x_Ex = 20;
    [System.NonSerialized]
    public int y_Ex = 20;
    [System.NonSerialized]
    public int offsetValue = 2000;
    [System.NonSerialized]
    public int offsetValue2 = 6000;
    [System.NonSerialized]
    public int maxReduceNum = 6;
    [System.NonSerialized]
    public Rhino.Geometry.Point3d origin = new Rhino.Geometry.Point3d(0, 0, 0);
    [System.NonSerialized]
    public int minGridNum = 2;
    [System.NonSerialized]
    public int roomNum = 5;

    // Start is called before the first frame update
    void Start()
    {
        var render = obj.GetComponent<LineRenderer>();
        var vetx = new List<Point3d>();
       for(int i = 0; i < render.positionCount; i++)
        {
            vetx.Add(render.GetPosition(i).ToRhino());
        }

        var shapeCrv = new Rhino.Geometry.Polyline(vetx).ToPolylineCurve();
        shapeCrv.MakeClosed(1);
        var plane = new Rhino.Geometry.Plane(AreaMassProperties.Compute(shapeCrv).Centroid, Vector3d.ZAxis);
        shapeCrv.Offset(plane, (-gridSize * minGridNum) / 2.0 * 0.001f, 1, CurveOffsetCornerStyle.Sharp)[0].TryGetPolyline(out Polyline offsetShapeCrv);

        Debug.Log(shapeCrv.Offset(plane, (-gridSize * minGridNum) / 2.0 * 0.001f ,1, CurveOffsetCornerStyle.Sharp).Length);
        RhinoPreview.PolyLineShow(offsetShapeCrv, Color.red, 0.3f, "offset", false);
        RhinoPreview.PolyLineShow(new Rhino.Geometry.Polyline(vetx), Color.green, 0.3f, "offset", false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
