using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino.Geometry;
using System.Linq;
using myRhinoWrapper;
using Rhino.Geometry;


public class AreaGrowth : MonoBehaviour
{

  public void Compute(List<Point3d> others, int gridSize, List<int> targetArea, int tolerance, ref List<List<Point3d>> areaPts, ref List<Point3d>  areaCenters)
  {
    var rtnTree = new List<List<Point3d>>();
    //Initialize
    for (int i = 0; i < targetArea.Count; i++)
    {
      rtnTree.Add(new List<Point3d>());
    }

    double hypotenuse = Math.Sqrt((gridSize * gridSize) * 2);

    for (int i = 0; i < targetArea.Count; i++)
    {
      SortList(ref others);
      if (others.Count == 0) break;
      var stPts = new List<Point3d>() { others[0] };
      GrowthMethod(stPts, ref others, ref rtnTree, hypotenuse, gridSize, targetArea[i], tolerance, i);

    }

    areaCenters = new List<Point3d>();
    for (int i = 0; i < rtnTree.Count; i++)
    {
      var path = i;
      var center = new Vector3d();
      foreach (Point3d pt in rtnTree[path])
      {
        center += new Vector3d(pt);
      }
      center /= rtnTree[path].Count;
      areaCenters.Add(new Point3d(center));
    }

    areaPts = rtnTree;
  }


  void GrowthMethod(List<Point3d> stPts, ref List<Point3d> others, ref List<List<Point3d>> rtnTree
      , double distance, int gridSize, int targetArea, int tolerance, int iterat)
  {

    var stPtsBuff = new List<Point3d>();

    //start Pts それぞれに処理していき、
    //結果としてothersを減らすのとstPtsを変更していく
    for (int i = 0; i < stPts.Count; i++)
    {
      //範囲内のptsをothersから取り出す
      var RangePt = GetPtsInRange(stPts[i], others, distance + 0.1);
      PtsRemovePts(RangePt, ref others);
      stPtsBuff.AddRange(RangePt);

      rtnTree[iterat].AddRange(RangePt);
    }

    //面積でなくただたんに隣合うポイントをつなげたいとき。
    if (targetArea == 0 && others.Count != 0 && stPtsBuff.Count != 0)
    {
      GrowthMethod(stPtsBuff, ref others, ref rtnTree
  , distance, gridSize, targetArea, tolerance, iterat);
      return;
    }
    else if (targetArea == 0 && others.Count == 0 && stPtsBuff.Count == 0)
    {
      return;
    }


    double area = AreaCalculate(rtnTree[iterat].Count, gridSize);
    if (Math.Abs(area - targetArea) < tolerance || area > targetArea ||
        others.Count == 0 || stPtsBuff.Count == 0)
    {
      return;
    }
    else
    {
      GrowthMethod(stPtsBuff, ref others, ref rtnTree
  , distance, gridSize, targetArea, tolerance, iterat);
    }
  }

  void SortList(ref List<Point3d> list)
  {
    var arr = list.ToArray();
    var xArr = arr.Select(pt => pt.X).ToArray();
    var yArr = arr.Select(pt => pt.Y).ToArray();

    System.Array.Sort(arr, xArr);
    System.Array.Sort(arr, yArr);

    list.Clear();
    list.AddRange(arr);
  }

  List<Point3d> GetPtsInRange(Point3d stPt, List<Point3d> others, double distance)
  {
    var rtnList = new List<Point3d>();
    var rPt = others.Where(other => stPt.DistanceTo(other) < distance);
    rtnList.AddRange(rPt.ToArray());
    return rtnList;
  }

  double AreaCalculate(int gridNum, int gridSize)
  {
    int areaSize = gridSize * gridSize;
    return gridNum * areaSize;
  }


  void PtsRemovePts(List<Point3d> removePt, ref List<Point3d> targetPts)
  {

    var rtnTree = new List<Point3d>();

    foreach (Point3d target in targetPts)
    {
      if (removePt.Contains(target) == false)
      {
        rtnTree.Add(target);
      }
    }
    targetPts = rtnTree;
    return;
  }

  List<Point3d> PtsContainsPts(List<Point3d> targets, List<Point3d> others)
  {
    var rtnArr = targets.Where(target => others.Contains(target) == true).ToArray();
    var rtnTree = new List<Point3d>();
    rtnTree.AddRange(rtnArr);
    return rtnTree;
  }

}
