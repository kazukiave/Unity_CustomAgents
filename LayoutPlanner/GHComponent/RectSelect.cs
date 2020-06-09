using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino.Geometry;
using System.Linq;
using myRhinoWrapper;

public class RectSelect : MonoBehaviour
{

    List<Rectangle3d> rects = new List<Rectangle3d>();
    List<Point3d> otherPtsBuff = new List<Point3d>();
    public void Compute(List<Point3d> AreaCenters, List<Point3d> gridPts, int gridSize, ref List<List<Point3d>> rectPts, ref List<Point3d> othersPts)
  {
        /*
        if (rectPts.Count == 0)
        {
            rectPts = new List<List<Point3d>>();
        }
        for (int i = 0; i < AreaCenters.Count; i++)
        {
            rectPts.Add(new List<Point3d>());
        }
        */

        var dists = RhinoWrapper.DistNearPt(AreaCenters);
        var intervals = MakeInterval(dists, gridSize);
        rects.Clear();
        for (int i = 0; i < AreaCenters.Count; i++)
        {
            var plane = new Rhino.Geometry.Plane(AreaCenters[i], Vector3d.ZAxis);
            rects.Add(new Rectangle3d(plane, intervals[i], intervals[i]));
        }

        rectPts = SelRectPts(gridPts, rects);

        otherPtsBuff.Clear();
        var allRectPts = GetAllData(rectPts);
        foreach (var pt in gridPts)
        {
            if(allRectPts.Contains(pt) == false)
            {
                otherPtsBuff.Add(pt);
            }
        }
        othersPts = otherPtsBuff;
    }

    private List<Interval> MakeInterval(List<double> dists, int gridSize)
    {
        var rtnList = new List<Interval>();

        for (int i = 0; i < dists.Count; i++)
        {
            var interValue = (dists[i] / 2) - gridSize;
            var inter = new Interval(interValue * -1, interValue);
            rtnList.Add(inter);
        }

        return rtnList;
    }

    private List<List<Point3d>> SelRectPts(List<Point3d> gridPts, List<Rectangle3d> exctArea)
    {
        var rectPts = new List<List<Point3d>>();
        for (int i = 0; i < exctArea.Count; i++)
        {
            rectPts.Add(new List<Point3d>());
        }


        for (int i = 0; i < gridPts.Count; i++)
        {
            int count = 0;

            for (int j = 0; j < exctArea.Count; j++)
            {
                if (IsInside(gridPts[i], exctArea[j].ToPolyline()))
                {
                    count++;
                    rectPts[j].Add(gridPts[i]);
                }
            }
        }

        return rectPts;
    }

    private List<Point3d> GetAllData(List<List<Point3d>> tree)
    {
        var rtnList = new List<Point3d>();

        for (int i = 0; i < tree.Count; i++)
        {
            for (int j = 0; j < tree[i].Count; j++)
            {
                rtnList.Add(tree[i][j]);
            }
        }

        return rtnList;
    }

    private bool IsInside(Point3d pt, Polyline crv)
    {
        Point3d pt1, pt2;
        bool oddNodes = false;

        for (int i = 0; i < crv.SegmentCount; i++) //for each contour line
        {

            pt1 = crv.SegmentAt(i).From; //get start and end pt
            pt2 = crv.SegmentAt(i).To;

            if ((pt1[1] < pt[1] && pt2[1] >= pt[1] || pt2[1] < pt[1] && pt1[1] >= pt[1]) && (pt1[0] <= pt[0] || pt2[0] <= pt[0])) //if pt is between pts in y, and either of pts is before pt in x
                oddNodes ^= (pt2[0] + (pt[1] - pt2[1]) * (pt1[0] - pt2[0]) / (pt1[1] - pt2[1]) < pt[0]); //^= is xor
                                                                                                         //end.X + (pt-end).Y   * (start-end).X  /(start-end).Y   <   pt.X
        }


        if (!oddNodes)
        {
            double minDist = 1e10;
            for (int i = 0; i < crv.SegmentCount; i++)
            {
                Point3d cp = crv.SegmentAt(i).ClosestPoint(pt, true);
                //Point3d cp = mvContour[i].closestPoint(pt);
                //minDist = min(minDist, cp.distance(pt));
                minDist = Math.Min(minDist, cp.DistanceTo(pt));
            }
            if (minDist < 1e-10)
                return true;
        }

        if (oddNodes) return true;

        return false;
    }
}
