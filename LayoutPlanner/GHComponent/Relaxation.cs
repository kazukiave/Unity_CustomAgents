using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino;
using Rhino.Geometry;

public class Relaxation : MonoBehaviour
{

    // Start is called before the first frame update
    public void Compute(List<Point3d> iStartingPositions, Polyline region, int minDistance, double moveVal, int maxIteration, ref List<Point3d> oCenters)
    {
        List<Point3d> centers = new List<Point3d>();

        foreach (var pt in iStartingPositions)
        {
            if (!IsInside(pt, region))
            {
                var nearPt = region.ClosestPoint(pt);
                var vec = (nearPt - pt);
                vec.Unitize();
                nearPt += vec * moveVal;
                centers.Add(nearPt);
            }
            else
            {
                centers.Add(pt);
            }
        }

        int count = 0;
        while (true)
        {
            count++;

            if (count > maxIteration)
            {
                break;
            }

            List<Vector3d> totalMoves = new List<Vector3d>();
            List<double> collisionCounts = new List<double>();

            for (int i = 0; i < centers.Count; i++)
            {
                totalMoves.Add(new Vector3d(0.0, 0.0, 0.0));
                collisionCounts.Add(0.0);
            }

            double collisionDistance = minDistance;

            for (int i = 0; i < centers.Count; i++)
            {
                var others = new List<Point3d>(centers);
                var nearWall = region.ClosestPoint(centers[i]);
                others.Add(nearWall);

                for (int j = 0; j < others.Count; j++)
                {
                    if (centers[i] == others[j]) continue;

                    double d = centers[i].DistanceTo(others[j]);
                    if (d > collisionDistance) continue;
                    Vector3d move = centers[i] - others[j];
                    move.Unitize();
                    move *= moveVal;
                    // move *= moveVal * (collisionDistance - d);
                    totalMoves[i] += move;
                    //totalMoves[j] -= move;
                    collisionCounts[i] += 1.0;
                    // collisionCounts[j] += 1.0;
                }
            }

            for (int i = 0; i < centers.Count; i++)
            {
                if (collisionCounts[i] != 0.0)
                {
                    var ptBuff = centers[i];
                    centers[i] += totalMoves[i] / collisionCounts[i]; ;
                    if (!IsInside(centers[i], region))
                    {
                        centers[i] = ptBuff;
                    }
                }
            }

            var sum = 0.0;
            foreach (var val in collisionCounts)
            {
                sum += val;
            }

            if (sum == 0)
            {
                break;
            }
        }
        oCenters = centers;
    }
    bool IsInside(Point3d pt, Polyline crv)
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
