using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Rhino;
using RhinoInside;
using MLAgents;
using RhinoInside.Unity;
using myRhinoWrapper;
using UnityEngine.UI;
using System.Text;

public class LayoutArea : MonoBehaviour
{
    //Component
    ShapedGrid _shapedGrid;
    AreaRatio _areaRatio;
    AreaGrowth _areaGrowth;
    Relax _relax;
    RectSelect _rectSelect;
    GridGrowth _gridGrowth;

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
    public int maxReduceNum = 3;

    public Vector3 originVec ;
    [System.NonSerialized]
    public int minGridNum = 2;
    [System.NonSerialized]
    public int roomNum = 3;
    [System.NonSerialized]
    public double minAreaRatio = 0.1;

    //ML property
    [Header("ML Values")]
    public List<Vector3> agentPosition;
    public List<int> targetAreaSize;
    public List<int> currentAreaSize;

    //GH output
    [Header("GH output")]
    [System.NonSerialized]
    public List<Rhino.Geometry.Point3d> shapedGrid;
    [System.NonSerialized]
    public Rhino.Geometry.PolylineCurve shape;
    [System.NonSerialized]
    public Rhino.Geometry.Polyline offsetShape;
    [System.NonSerialized]
    public Rhino.Geometry.Rectangle3d originalShape;
    [System.NonSerialized]
    public Rhino.Geometry.Rectangle3d rectMin;
 
    [System.NonSerialized]
    public List<double> areaRatio;
    [System.NonSerialized]
    public List<double> areaSize;
    [System.NonSerialized]
    public List<Rhino.Geometry.Point3d> areaCenters;
    [System.NonSerialized]
    public List<List<Rhino.Geometry.Point3d>> rectPts;
    [System.NonSerialized]
    public List<Rhino.Geometry.Point3d> othersPts;
    [System.NonSerialized]
    public int minDist;
    [System.NonSerialized]
    public List<List<Rhino.Geometry.Point3d>> RoomPts;

    //Visualize 
    [Header("Visualize")]
    public bool showInfo;
    public bool show;
    public bool debug;
    private List<GameObject> tiles = new List<GameObject>();
    private List<GameObject> agents = new List<GameObject>();
    private List<GameObject> polyLines = new List<GameObject>();
    private List<GameObject> debugPreview = new List<GameObject>();

    [System.NonSerialized]
    public List<string> obsStr = new List<string>();

    public GameObject text1;
    public GameObject text2;
    public GameObject text3;
    public GameObject obsStrPos;

    public int actionCount = 0;

    public bool convertM;
  
    public void Init()
    {
        //子のエージェントに番号を振る
        var transforms= gameObject.GetComponentsInChildren<Transform>(false);
        List<Transform> agents = transforms.Where(transform => transform.CompareTag("agent")).ToList();
        
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].gameObject.GetComponent<LayoutAgent>().agentNumber = i;
            agents[i].gameObject.name = "Agent" + "Num" + i.ToString();
        }

        //初期化
        _shapedGrid = gameObject.AddComponent<ShapedGrid>();
        _areaRatio = gameObject.AddComponent<AreaRatio>();
        _areaGrowth = gameObject.AddComponent<AreaGrowth>();
        _relax = gameObject.AddComponent<Relax>();
        _rectSelect = gameObject.AddComponent<RectSelect>();
        _gridGrowth = gameObject.AddComponent<GridGrowth>();

        ReComputeShape();
        ReComputeArea();

        if (show)
        {
            ReloadPreview(false);
        }
    }

   
    int frameCount = 0;
    // Update is called once per frame
    void Update()
    {
     
        if (!currentAreaSize.Contains(0))
        {
            if (debug)
            {
                foreach (var preview in debugPreview)
                {
                    Destroy(preview);
                }
            }

            if (show)
            {
                frameCount++;
                bool isUnload = frameCount > 0 ? true : false;
                ReloadPreview(isUnload);
                if (isUnload) frameCount = 0;
            }
        }
       
    }
      
  
    public void ReComputeShape()
    {
        //Shape
        Rhino.Geometry.Point3d origin = originVec.ToRhino();
        _shapedGrid.Compute(gridSize, x_Ex, y_Ex, offsetValue, offsetValue2, maxReduceNum, minGridNum, origin, ref shapedGrid, ref shape, ref offsetShape, ref originalShape);
        if (Rhino.Geometry.AreaMassProperties.Compute(offsetShape.ToPolylineCurve()).Area <
            (Rhino.Geometry.AreaMassProperties.Compute(shape).Area / 2f))
        {
            ReComputeShape();
            Debug.Log("Failed RecomputeShape and start RecomputeShape again");
            return;
        }

        _areaRatio.Compute(shape, minAreaRatio, roomNum, ref areaSize, ref areaRatio);
        targetAreaSize = areaSize.ToIntList();
        if (debug)
        {
            debugPreview.Add(RhinoPreview.PtsShow(shapedGrid, gridSize, Color.white));
        }
        if (show)
        {
            if (polyLines.Count != 0)
            {
                foreach (var line in polyLines)
                {
                    Destroy(line);
                }
            }

            polyLines.Add(RhinoPreview.PolyLineShow(shape.ToPolyline(), Color.cyan, 0.3f, "shape"));
            polyLines.Add(RhinoPreview.PolyLineShow(offsetShape, Color.blue, 0.3f, "offsetShape"));
            polyLines.Add(RhinoPreview.PolyLineShow(originalShape.ToPolyline(), Color.gray, 0.3f, "originalShape"));
        }


        // make first agent position
        var center = Rhino.Geometry.AreaMassProperties.Compute(shape).Centroid;
        rectMin = RhinoWrapper.MakeRect(center, gridSize, gridSize);
        var areaCenters = RhinoWrapper.RandomPt(rectMin, roomNum);

     
        if(debug)
        {
            debugPreview.Add(RhinoPreview.PolyLineShow(rectMin.ToPolyline(), Color.green, 0.3f, "minRect"));
            debugPreview.Add(RhinoPreview.PtsShow(areaCenters, gridSize, Color.black));

            var centersList = new List<Rhino.Geometry.Point3d>();
            centersList.Add(center);
            debugPreview.Add(RhinoPreview.PtsShow(centersList, gridSize, Color.magenta)); 
        }

        //relax agent position
        minDist = (int)Math.Ceiling(Math.Sqrt(2) * gridSize) * (minGridNum + 1);
        _relax.Compute(ref areaCenters, minDist, offsetShape, (double)gridSize / 2.0);
        if (debug)
        {
            debugPreview.Add(RhinoPreview.PtsShow(areaCenters, gridSize, Color.red));
        }
        agentPosition = areaCenters.ToHost();
    }

    private List<Rhino.Geometry.Point3d> agentPts = new List<Rhino.Geometry.Point3d>();
    public void ReComputeArea()
    {
        //relax
         agentPts = agentPosition.ToRhino().ToList();
        _relax.Compute(ref agentPts, minDist , offsetShape, (double)gridSize /5.0);
        agentPosition = agentPts.ToHost();

        if (debug)
        {
            debugPreview.Add(RhinoPreview.PtsShow(agentPts, gridSize, Color.black));
        }

        //rect select
        _rectSelect.Compute(agentPts, shapedGrid, gridSize, ref rectPts, ref othersPts);
    

        //gridGrowth
        _gridGrowth.Compute(rectPts.ToVector3d().ToList(), othersPts.ToVector3d().ToList(), gridSize, targetAreaSize, minGridNum, ref currentAreaSize, ref RoomPts);
        if (debug)
        {
            for (int i = 0; i < RoomPts.Count; i++)
            {
                var col = Color.HSVToRGB((float)((float)i / (float)(rectPts.Count)), 1, 1f, true);
                debugPreview.Add(RhinoPreview.TileShow(RoomPts[i], gridSize, col));
            }
        }
    }

    public void ReloadPreview(bool isResourceUnload)
    {
        if(agents.Count != 0) { 
            foreach(var agent in agents)
                Destroy(agent); 
        }

        if (tiles.Count != 0)
        {
            foreach (var tile in tiles)
                Destroy(tile);
        }

        //agent 
        var agentPts = agentPosition.ToRhino().ToList();
        agents.Add(RhinoPreview.PtsShow(agentPts, gridSize, Color.white, convertM));

        //Room
        for (int i = 0; i < RoomPts.Count; i++)
        {
            var col = Color.HSVToRGB((float)((float)i / (float)(rectPts.Count)), 1, 1f, true);
            tiles.Add(RhinoPreview.TileShow(RoomPts[i], gridSize, col, convertM));
        }

        for (int i = 0; i < rectPts.Count; i++)
        {
            var pts = rectPts[i].Select(pt => pt + new Rhino.Geometry.Vector3d(0, 0, -1000f)).ToList();
            tiles.Add(RhinoPreview.TileShow(pts, gridSize, Color.white, convertM));
        }

        if (isResourceUnload) Resources.UnloadUnusedAssets();

    }


    List<string> targetTexts = new List<string>();
    List<string> currentTexts = new List<string>();
    List<string> differenceTexts = new List<string>();
    private void OnGUI()
    {
        if (!showInfo) return;

        if (text1 != null)
        {
            targetTexts.Clear();
            foreach (var val in targetAreaSize)
            {
                targetTexts.Add((val / (1000 * 1000)).ToString());
            }

            myGUIUtilty.TextItemize(text1.transform.position, targetTexts, Color.black, "Target", 30); ;
        }

        if (text2 != null)
        {
            currentTexts.Clear();
            foreach (var val in currentAreaSize)
            {
                currentTexts.Add((val / (1000 * 1000)).ToString());
            }
            myGUIUtilty.TextItemize(text2.transform.position, currentTexts, Color.black, "Current", 30);
        }

        if (text3 != null)
        {
            differenceTexts.Clear();
            for (int i = 0; i < targetAreaSize.Count; i++)
            {
                differenceTexts.Add(((targetAreaSize[i] - currentAreaSize[i]) / (1000 * 1000)).ToString());
            }
            myGUIUtilty.TextItemize(text3.transform.position, differenceTexts, Color.black, "Difference", 30);
        }
        
        if(obsStrPos != null)
        {
            myGUIUtilty.TextItemize(obsStrPos.transform.position, obsStr, Color.black, "obsStrPos", 30);
        }
    }
}
