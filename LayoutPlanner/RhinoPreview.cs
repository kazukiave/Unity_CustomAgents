using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino;
using Rhino.Geometry;
using System.Linq;
using RhinoInside.Unity;

public static class RhinoPreview
{
    public static GameObject PolyLineShow(Rhino.Geometry.Polyline polyLine, Color color, float width, string name = "PolyLine", bool convertM = true)
    {
        var polyLineObj = new GameObject(name);
        var lineRender = polyLineObj.AddComponent<LineRenderer>();
        var vtxs = polyLine.ToList();
        lineRender.positionCount = vtxs.Count;

        for (int i = 0; i < vtxs.Count; i++)
        {
            var position = convertM ? new Vector3((float)vtxs[i].X, (float)vtxs[i].Y, (float)vtxs[i].Z) * 0.001f
                                        : new Vector3((float)vtxs[i].X, (float)vtxs[i].Y, (float)vtxs[i].Z) ;
            lineRender.SetPosition(i, position);
        }

        lineRender.startColor = color;
        lineRender.endColor = color;
        lineRender.startWidth = width;
        lineRender.endWidth = width;
        lineRender.receiveShadows = false;
        lineRender.material = new Material(Shader.Find("UI/Default"));

        return polyLineObj;
    }
    public static GameObject PtsShow(List<Rhino.Geometry.Point3d> grid, int gridSize, Color color, bool convertM = true, string name = "Pts")
    {
        var gridObj = new GameObject(name);

        foreach (var pt in grid)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = gridObj.transform;
           
            sphere.transform.position = convertM ?  pt.ToHost() * 0.001f : pt.ToHost();
            sphere.transform.localScale = convertM ? new Vector3(gridSize, gridSize, gridSize) * 0.001f : new Vector3(gridSize, gridSize, gridSize);
          
            sphere.GetComponent<MeshRenderer>().material.color = color;
            sphere.GetComponent<MeshRenderer>().material.shader = Shader.Find("UI/Default");
        }

        return gridObj;
    }

   
    public static GameObject TileShow(List<Rhino.Geometry.Point3d> grid, int gridSize, Color color, bool convertM = true, string name = "TileGrid")
    {
        var gridObj = new GameObject(name);

        Brep[] breps = new Brep[grid.Count];
       // List<Brep> breps = new List<Brep>();
       for(int i = 0; i < grid.Count; i++)
        {
            var pt = grid[i];
            if (convertM)
            {
                var mPt = pt * (0.001);
                var plane = new Rhino.Geometry.Plane(mPt, Vector3d.ZAxis);
                var interval = new Interval((-gridSize / 2) * (0.001),(gridSize / 2) * (0.001));

                var srf = new Rhino.Geometry.PlaneSurface(plane, interval, interval);
                var brep = srf.ToBrep();
                // breps.Add(brep);
                breps[i] = brep; 
            }
            else
            {
                var plane = new Rhino.Geometry.Plane(pt, Vector3d.ZAxis);
                var interval = new Interval(-gridSize / 2, gridSize / 2);

                var srf = new Rhino.Geometry.PlaneSurface(plane, interval, interval);
                var brep = srf.ToBrep();
                //breps.Add(brep);
                breps[i] = brep;
            }
        }
        var joinedBrep = Rhino.Geometry.Brep.CreateBooleanUnion(breps, 0.1);

        var meshParam = MeshingParameters.FastRenderMesh;
        var meshs = Rhino.Geometry.Mesh.CreateFromBrep(joinedBrep[0], meshParam);

        var joinedMesh = new Rhino.Geometry.Mesh();
        foreach (var m in meshs)
        {
            joinedMesh.Append(m);
        }
        joinedMesh.Weld(180);


        //attatch Mesh
        var UnityMesh = joinedMesh.ToHost();

        var meshRender = gridObj.AddComponent<MeshRenderer>();
        meshRender.material.color = color;
        meshRender.material.shader = Shader.Find("UI/Default");

        var meshFilter = gridObj.AddComponent<MeshFilter>();
        meshFilter.mesh = UnityMesh;

        return gridObj;
    }

}
