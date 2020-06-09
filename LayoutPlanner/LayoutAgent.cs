using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Rhino;
using RhinoInside;
using MLAgents;
using UnityEngine.Serialization;
using myRhinoWrapper;
using RhinoInside.Unity;

public class LayoutAgent : Agent
{
    public int agentNumber = 0;
    public bool showInfo = false;
  //  private LayoutAcademy layoutAcademy;
    private LayoutArea layoutArea;

    private int gridSize;
    private int x_Ex;
    private int y_Ex;
    private Rhino.Geometry.PolylineCurve shape;
    public Rhino.Geometry.Polyline offsetShape;

    private const int k_NoAction = 0;  // do nothing!
    private const int k_Up = 1;
    private const int k_Down = 2;
    private const int k_Left = 3;
    private const int k_Right = 4;

    public override void InitializeAgent()
    {
        // layoutAcademy = GameObject.FindObjectOfType<Academy>().GetComponent<LayoutAcademy>();
        layoutArea = transform.parent.GetComponent<LayoutArea>();

        //transform.position = layoutAcademy.agentPosition[agentNumber];
        transform.position = layoutArea.agentPosition[agentNumber];

        //gridSize = layoutAcademy.gridSize;
        gridSize = layoutArea.gridSize;
        x_Ex = layoutArea.x_Ex;
        y_Ex = layoutArea.y_Ex;
        shape = layoutArea.shape;
        offsetShape = layoutArea.offsetShape;
     
    }

   
    public override void CollectObservations()
    {

        //transform.position = layoutArea.agentPosition[agentNumber] / (layoutArea.gridSize * layoutArea.x_Ex);

        //Current position (relative)
        var curPos = new Vector2(transform.position.x / (float)(gridSize * x_Ex), transform.position.y / (float)(gridSize * y_Ex));
        AddVectorObs(curPos);
  
        //近くの壁までのVector
        var agentPt = transform.position.ToRhino();

        var nearWall = shape.ToPolyline().ClosestPoint(agentPt);
        var dirNearWall = (nearWall - agentPt);


        Vector2 dirNearWallVec2 = new Vector2((float)dirNearWall.X / (float)(gridSize * x_Ex), (float)dirNearWall.Y / (float)(gridSize * y_Ex));
        AddVectorObs(dirNearWallVec2);

        if (showInfo)
        {
            layoutArea.obsStr.Clear();
            layoutArea.obsStr.Add(("Current position " + curPos.ToString("F3")));
            layoutArea.obsStr.Add(("dirNearWall " + dirNearWallVec2.ToString("F3")));
        }

        //4方向の壁
        float[] fourAxisWall = GetFourAxis(agentPt, shape);
        AddVectorObs(fourAxisWall);

        if (showInfo)
        {
            foreach (var val in fourAxisWall)
            {
                layoutArea.obsStr.Add(("fourAxisWall" + val.ToString("F3")));
            }
        }

    
        /*
        var lenghtNearWall = dirNearWallVec2.magnitude;
        AddVectorObs(lenghtNearWall);
        */


        //他のAgentとの距離と方向
        for (int i = 0; i < layoutArea.agentPosition.Count; i++)
        {
            if (i == agentNumber) continue;
            var dir = layoutArea.agentPosition[i] - transform.position;

            Vector2 dirAgent = new Vector2((float)dir.x / (float)(gridSize * x_Ex), (float)dir.y / (float)(gridSize * y_Ex));
            AddVectorObs(dirAgent);


            /*
            var dist = dirAgent.magnitude;
            AddVectorObs(dist);
            */

            if (showInfo)
            {
                layoutArea.obsStr.Add((i + "AgentDir "  + dirAgent.ToString("F3")));
            }
        }

        //面積の差を割合として渡す
        float currentArea = layoutArea.currentAreaSize[agentNumber] / (1000f * 1000f);
        float targetArea = layoutArea.targetAreaSize[agentNumber] /(1000f * 1000f);
        float differenceArea = targetArea - currentArea;
        float shapeArea = (float)Rhino.Geometry.AreaMassProperties.Compute(shape).Area / (1000f * 1000f);

        if (differenceArea != 0)
        {
            //  differenceArea = differenceArea > 0 ? Mathf.Pow((differenceArea / (1000f * 1000f)), 2) : Mathf.Pow((differenceArea / (1000f * 1000f)), 2) * -1f;
            var diffArea = (differenceArea / targetArea);
            AddVectorObs(diffArea);

            if (showInfo)
            {
                layoutArea.obsStr.Add(("differenceAreaObs " + diffArea).ToString());
            }
        }
        else
        {
            AddVectorObs(0);
        }

        SetMask();
    }

    
    float preAreaDiff = 0;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        
        var action = Mathf.FloorToInt(vectorAction[0]);

            switch (action)
        {
            case k_NoAction:
                // do nothing
                break;
            case k_Right:
                  transform.position += new Vector3(gridSize, 0, 0f);
                break;
            case k_Left:
                  transform.position += new Vector3(-gridSize, 0, 0f);
                break;
            case k_Up:
                  transform.position += new Vector3(0f, gridSize, 0);
                break;
            case k_Down:
                  transform.position += new Vector3(0f, -gridSize, 0);
                break;
        }

        layoutArea.agentPosition[agentNumber] = transform.position;
        layoutArea.ReComputeArea();

        //この値を報酬に
        var currentArea = layoutArea.currentAreaSize[agentNumber] / (1000f * 1000f);
        var targetArea = layoutArea.targetAreaSize[agentNumber] / (1000f * 1000f);

        float areaDiff = Mathf.Abs(targetArea - currentArea);

        if(8f > areaDiff)
        {
            SetReward(0.5f);
        }
        else if (preAreaDiff > areaDiff)
        {
            SetReward(0.01f);
        }
        else
        {
            SetReward(-0.01f);
        }

        if (showInfo)
        {
            layoutArea.obsStr.Add((preAreaDiff + " preAreaDiff").ToString());
            layoutArea.obsStr.Add((areaDiff + " curAreaDiff").ToString());
        }
        preAreaDiff = areaDiff;

    }

    float[] rtnArr = new float[1];
    List<float> selectable = new List<float>();
    public override float[] Heuristic()
    {
        var nextRhigt = Vec3ToPt3d(transform.position + new Vector3(gridSize, 0, 0f));

        var nextLeft = Vec3ToPt3d(transform.position + new Vector3(-gridSize, 0, 0f));

        var nextUp = Vec3ToPt3d(transform.position + new Vector3(0f, gridSize, 0));

        var nextDown = Vec3ToPt3d(transform.position + new Vector3(0f, -gridSize, 0));


        selectable.Clear();
        selectable.Add(k_NoAction);

        if (RhinoWrapper.IsInside(nextRhigt, offsetShape))
        {
            selectable.Add(k_Right);
        }

        if (RhinoWrapper.IsInside(nextLeft, offsetShape))
        {
            selectable.Add(k_Left);
        }

        if (RhinoWrapper.IsInside(nextUp, offsetShape))
        {
            selectable.Add(k_Up);
        }

        if (RhinoWrapper.IsInside(nextDown, offsetShape))
        {
            selectable.Add(k_Down);
        }

        if (selectable.Count > 1)
        {
            var idx = Random.Range(1, selectable.Count);
            rtnArr[0] = selectable[idx - 1];
        }
        else
        {
            rtnArr[0] = selectable[0];
        }

        return rtnArr;
    }

    private void SetMask()
    {
        //進んだらエリア外に出る選択肢にマスクをする。

        var nextRhigt = Vec3ToPt3d(transform.position + new Vector3(gridSize, 0, 0f));
       
        var nextLeft = Vec3ToPt3d(transform.position + new Vector3(-gridSize, 0, 0f));

        var nextUp = Vec3ToPt3d(transform.position + new Vector3(0f, gridSize, 0));
      
        var nextDown = Vec3ToPt3d(transform.position + new Vector3(0f, -gridSize, 0));
        

        if (!RhinoWrapper.IsInside(nextRhigt, offsetShape))
        {
            SetActionMask(k_Right);
        }

        if (!RhinoWrapper.IsInside(nextLeft, offsetShape))
        {
            SetActionMask(k_Left);
        }

        if (!RhinoWrapper.IsInside(nextUp, offsetShape))
        {
            SetActionMask(k_Up);
        }

        if (!RhinoWrapper.IsInside(nextDown, offsetShape))
        {
            SetActionMask(k_Down);
        }
        
    }

    public override void AgentReset()
    {
        InitializeAgent();
        layoutArea.ReComputeShape();
        layoutArea.ReComputeArea();
    }

    private Rhino.Geometry.Point3d Vec3ToPt3d(Vector3 vector)
    {
        return new Rhino.Geometry.Point3d(vector.x, vector.y, vector.z);
    }


    List<Rhino.Geometry.Point3d> hitsPoints = new List<Rhino.Geometry.Point3d>();
    Rhino.Geometry.GeometryBase[] wall = new Rhino.Geometry.GeometryBase[1];
    Rhino.Geometry.Ray3d ray;
    Rhino.Geometry.Point3d[] hits;
    float[] rtnValues = new float[4];
    private float[] GetFourAxis(Rhino.Geometry.Point3d agentPt, Rhino.Geometry.PolylineCurve shapeCrv)
    {
        hitsPoints.Clear();
        wall[0] = Rhino.Geometry.Surface.CreateExtrusion(shapeCrv, Rhino.Geometry.Vector3d.ZAxis) as Rhino.Geometry.GeometryBase;

        ray = new Rhino.Geometry.Ray3d(agentPt, Rhino.Geometry.Vector3d.XAxis);
        hits = Rhino.Geometry.Intersect.Intersection.RayShoot(ray, wall, 1);
        hitsPoints.AddRange(hits);

        ray = new Rhino.Geometry.Ray3d(agentPt, Rhino.Geometry.Vector3d.YAxis);
        hits = Rhino.Geometry.Intersect.Intersection.RayShoot(ray, wall, 1);
        hitsPoints.AddRange(hits);

        ray = new Rhino.Geometry.Ray3d(agentPt, Rhino.Geometry.Vector3d.XAxis * -1);
        hits = Rhino.Geometry.Intersect.Intersection.RayShoot(ray, wall, 1);
        hitsPoints.AddRange(hits);

        ray = new Rhino.Geometry.Ray3d(agentPt, Rhino.Geometry.Vector3d.YAxis * -1);
        hits = Rhino.Geometry.Intersect.Intersection.RayShoot(ray, wall, 1);
        hitsPoints.AddRange(hits);


        for(int i = 0; i < hitsPoints.Count; i++)
        {
            var dir = hitsPoints[i] - agentPt;
            Vector2 hitVec = new Vector2((float)dir.X / (float)(gridSize * x_Ex), (float)dir.Y / (float)(gridSize * y_Ex));
            if (hitVec.x != 0)
            {
                rtnValues[i] = Mathf.Abs(hitVec.x);
            }
            if (hitVec.y != 0)
            {
                rtnValues[i] = Mathf.Abs(hitVec.y);
            }
        }
        return rtnValues;
    }
}
