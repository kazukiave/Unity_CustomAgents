using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino.Geometry;
using System.Linq;
using myRhinoWrapper;
public class GridGrowth : MonoBehaviour
{

    public List<List<Vector3d>> areaGrid;
    public List<List<Point3d>> rommPtsBuff = new List<List<Point3d>>();
    public void Compute(List<List<Vector3d>> rectPts, List<Vector3d> spacePts, int gridSize, List<int> targetAreaSize,
        int minGridNum,  ref List<int> areaSize, ref List<List<Point3d>> roomPts, int iteration = 1000)
    {
        int num_rect = rectPts.Count;
        int count = 0;
        //loop
        //  while (canGrowthTree.AllData().Contains(true))
        while (count < iteration)
        {
            count++;

            var sortedDictionary = new SortedDictionary<double, int>();

            for (int i = 0; i < num_rect; i++)
            {
                var path = i;
                var rect = rectPts[path];

                var diff = (targetAreaSize[i] - AreaCalculate(rect.Count, gridSize));

                while (sortedDictionary.Keys.Contains(diff))
                {
                    diff++;
                }

                sortedDictionary.Add(diff, path);
            }

            var sortedList = sortedDictionary.Values.ToList();
            sortedList.Reverse();

            int isEnd = 0;
            for (int i = 0; i < num_rect; i++)
            {
                //このパスを面積の差が多きやつからにすればそうなる。
                var path = sortedList[i];
                var rect = rectPts[path];
                var outlinePts = new OutlinePts
                {
                    left = GetNextLeftLine(rect, spacePts, gridSize),
                    right = GetNextRightLine(rect, spacePts, gridSize),
                    up = GetNextUpLine(rect, spacePts, gridSize),
                    down = GetNextDownLine(rect, spacePts, gridSize)
                };

                if (outlinePts.GetAll().Count == 0 || outlinePts.GetMaxCountList().Count < minGridNum) continue;

                isEnd++;

                List<Vector3d> nextPos = outlinePts.GetMaxCountList();
                rectPts[path].AddRange(nextPos);
                ListRemoveList(ref spacePts, nextPos);
            }
            if (isEnd == 0) break;
        }

        areaGrid = rectPts;

        rommPtsBuff.Clear();
        for (int i = 0; i < rectPts.Count; i++)
        {
            rommPtsBuff.Add(new List<Point3d>());
            for (int j = 0; j < rectPts[i].Count; j++)
            {
                rommPtsBuff[i].Add(new Point3d(rectPts[i][j]));
            }
        }
        roomPts = rommPtsBuff;

        //  areaCenter = TreeAverage(rectPts);
        areaSize = TreeAreaCalculate(rectPts, gridSize);
    }

    private List<Point3d> TreeAverage(List<List<Vector3d>> ptTree)
    {
        var rtnList = new List<Point3d>();

        for (int i = 0; i < ptTree.Count; i++)
        {
            var mass = Vector3d.Zero;
            var count = ptTree[i].Count;
            if (count == 0) continue;

            for (int j = 0; j < ptTree[i].Count; j++)
            {
                mass += ptTree[i][j];
            }

            rtnList.Add(new Point3d(mass / count));
        }

        return rtnList;
    }

    private List<int> TreeAreaCalculate(List<List<Vector3d>> ptTree, int gridSize)
    {
        var rtnList = new List<int>();

        foreach (var list in ptTree)
        {
            int area = Mathf.FloorToInt((float)AreaCalculate(list.Count, gridSize));
            rtnList.Add(area);
        }

        return rtnList;
    }

    private double AreaCalculate(int gridNum, int gridSize)
    {
        int areaSize = gridSize * gridSize;
        return gridNum * areaSize;
    }

    private void ListRemoveList(ref List<Vector3d> source, List<Vector3d> delData)
    {
        for (int i = 0; i < delData.Count; i++)
        {
            for (int j = 0; j < source.Count; j++)
            {
                if (delData[i] == source[j])
                {
                    source.RemoveAt(j);
                }
            }
        }
    }

    private bool ListContainsList(List<Vector3d> child, List<Vector3d> parent)
    {
        var rtnVal = true;

        for (int i = 0; i < child.Count; i++)
        {
            if (!parent.Contains(child[i]))
            {
                return false;
            }
        }

        return rtnVal;
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

    double Hypotenuse(int gridSize)
    {
        return Math.Sqrt((gridSize * gridSize) * 2);
    }


    List<Point3d> VecToPt(List<Vector3d> vec)
    {
        var rtnList = new List<Point3d>();

        var rtnArr = vec.Select(v => new Point3d(v)).ToArray();
        rtnList.AddRange(rtnArr);
        return rtnList;
    }

    List<Vector3d> PtToVec(List<Point3d> pts)
    {
        var rtnList = new List<Vector3d>();

        var rtnArr = pts.Select(pt => new Vector3d(pt)).ToArray();
        rtnList.AddRange(rtnArr);
        return rtnList;
    }


    private List<Vector3d> GetNextLeftLine(List<Vector3d> outLine, List<Vector3d> spacePts, int gridSize)
    {
        var rtnList = new List<Vector3d>();
        if (outLine.Count == 0 )
        {
            return rtnList;
        }

        Bbox _bbox = new Bbox(outLine);
        for (int i = 0; i < outLine.Count; i++)
        {
            if (outLine[i].X == _bbox.Min.X)
            {

                var nextPos = new Vector3d(outLine[i].X - gridSize
                                    , outLine[i].Y
                                    , 0);
                if (spacePts.Contains(nextPos))
                    rtnList.Add(nextPos);
            }
        }

        //二つに分かれたときは数の多いほうを返す。
        var rtnTree = new List<List<Point3d>>();
        double hypotenuse = Math.Sqrt((gridSize * gridSize) * 2);
        var rtnListPts = VecToPt(rtnList);

        int iterat = 0;
        while (rtnListPts.Count > 0 && iterat < 1000)
        {
            SortList(ref rtnListPts);
            var stPts = new List<Point3d>() { rtnListPts[0] };
            ConnectPts(stPts, ref rtnListPts, ref rtnTree, hypotenuse, 10, 1, iterat);
            iterat++;
        }

        if (rtnTree.Count == 1)
        {
            return rtnList;
        }

        if (rtnTree.Count > 1)
        {
            int maxPath = 0;
            int maxCount = 0;
            for (int i = 0; i < rtnTree.Count; i++)
            {
                var path = i;
                if (rtnTree[path].Count > maxCount)
                {
                    maxCount = rtnTree[path].Count;
                    maxPath = path;
                }
            }
            return PtToVec(rtnTree[maxPath]);
        }

        return rtnList;
    }

    private List<Vector3d> GetNextRightLine(List<Vector3d> outLine, List<Vector3d> spacePts, int gridSize)
    {
        var rtnList = new List<Vector3d>();
        if (outLine.Count == 0)
        {
            return rtnList;
        }

        Bbox _bbox = new Bbox(outLine);
        for (int i = 0; i < outLine.Count; i++)
        {
            if (outLine[i].X == _bbox.Max.X)
            {
                var nextPos = new Vector3d(outLine[i].X + gridSize
                                 , outLine[i].Y
                                 , 0);
                if (spacePts.Contains(nextPos))
                    rtnList.Add(nextPos);
            }
        }

        //二つに分かれたときは数の多いほうを返す。
        var rtnTree = new List<List<Point3d>>();
        double hypotenuse = Math.Sqrt((gridSize * gridSize) * 2);
        var rtnListPts = VecToPt(rtnList);

        int iterat = 0;
        while (rtnListPts.Count > 0 && iterat < 1000)
        {
            SortList(ref rtnListPts);
            var stPts = new List<Point3d>() { rtnListPts[0] };
            ConnectPts(stPts, ref rtnListPts, ref rtnTree, hypotenuse, 10, 1, iterat);
            iterat++;
        }

        if (rtnTree.Count == 1)
        {
            return rtnList;
        }

        if (rtnTree.Count > 1)
        {
            int maxPath = 0;
            int maxCount = 0;
            for (int i = 0; i < rtnTree.Count; i++)
            {
                var path = i;
                if (rtnTree[path].Count > maxCount)
                {
                    maxCount = rtnTree[path].Count;
                    maxPath = path;
                }
            }
            return PtToVec(rtnTree[maxPath]);
        }


        return rtnList;
    }

    private List<Vector3d> GetNextUpLine(List<Vector3d> outLine, List<Vector3d> spacePts, int gridSize)
    {
        var rtnList = new List<Vector3d>();
        if (outLine.Count == 0)
        {
            return rtnList;
        }

        Bbox _bbox = new Bbox(outLine);
        for (int i = 0; i < outLine.Count; i++)
        {
            if (outLine[i].Y == _bbox.Max.Y)
            {
                var nextPos = new Vector3d(outLine[i].X
                                  , outLine[i].Y + gridSize
                                  , 0);

                if (spacePts.Contains(nextPos))
                    rtnList.Add(nextPos);
            }
        }
        //二つに分かれたときは数の多いほうを返す。
        var rtnTree = new List<List<Point3d>>();
        double hypotenuse = Math.Sqrt((gridSize * gridSize) * 2);
        var rtnListPts = VecToPt(rtnList);

        int iterat = 0;
        while (rtnListPts.Count > 0 && iterat < 1000)
        {
            SortList(ref rtnListPts);
            var stPts = new List<Point3d>() { rtnListPts[0] };
            ConnectPts(stPts, ref rtnListPts, ref rtnTree, hypotenuse, 10, 1, iterat);
            iterat++;
        }

        if (rtnTree.Count == 1)
        {
            return rtnList;
        }

        if (rtnTree.Count > 1)
        {
            int maxPath = 0;
            int maxCount = 0;
            for (int i = 0; i < rtnTree.Count; i++)
            {
                var path = i;
                if (rtnTree[path].Count > maxCount)
                {
                    maxCount = rtnTree[path].Count;
                    maxPath = path;
                }
            }
            return PtToVec(rtnTree[maxPath]);
        }

        return rtnList;
    }
    private List<Vector3d> GetNextDownLine(List<Vector3d> outLine, List<Vector3d> spacePts, int gridSize)
    {
        var rtnList = new List<Vector3d>();
        if (outLine.Count == 0)
        {
            return rtnList;
        }

        Bbox _bbox = new Bbox(outLine);
        for (int i = 0; i < outLine.Count; i++)
        {
            if (outLine[i].Y == _bbox.Min.Y)
            {
                var nextPos = new Vector3d(outLine[i].X
                                 , outLine[i].Y - gridSize
                                 , 0);
                if (spacePts.Contains(nextPos))
                    rtnList.Add(nextPos);
            }
        }

        //二つに分かれたときは数の多いほうを返す。
        var rtnTree = new List<List<Point3d>>();


        double hypotenuse = Math.Sqrt((gridSize * gridSize) * 2);
        var rtnListPts = VecToPt(rtnList);

        int iterat = 0;
        while (rtnListPts.Count > 0 && iterat < 1000)
        {
            rtnTree.Add(new List<Point3d>());
            SortList(ref rtnListPts);
            var stPts = new List<Point3d>() { rtnListPts[0] };
            ConnectPts(stPts, ref rtnListPts, ref rtnTree, hypotenuse, 10, 1, iterat);
            iterat++;
        }

        if (rtnTree.Count == 1)
        {
            return rtnList;
        }

        if (rtnTree.Count > 1)
        {
            int maxPath = 0;
            int maxCount = 0;
            for (int i = 0; i < rtnTree.Count; i++)
            {
                var path = i;
                if (rtnTree[path].Count > maxCount)
                {
                    maxCount = rtnTree[path].Count;
                    maxPath = path;
                }
            }
            return PtToVec(rtnTree[maxPath]);
        }

        return rtnList;
    }



    void ConnectPts(List<Point3d> stPts, ref List<Point3d> others, ref List<List<Point3d>> rtnTree
      , double distance, int gridSize, int tolerance, int iterat)
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


            rtnTree.Add(new List<Point3d>());

            rtnTree[iterat].AddRange(RangePt);
        }

        //面積でなくただたんに隣合うポイントをつなげたいとき。
        if (others.Count != 0 && stPtsBuff.Count != 0)
        {
            ConnectPts(stPtsBuff, ref others, ref rtnTree
              , distance, gridSize, tolerance, iterat);
            return;
        }
        else if (others.Count == 0 && stPtsBuff.Count == 0)
        {
            return;
        }

        if (others.Count == 0 || stPtsBuff.Count == 0)
        {
            return;
        }
        else
        {
            ConnectPts(stPtsBuff, ref others, ref rtnTree
              , distance, gridSize, tolerance, iterat);
        }
    }
}
