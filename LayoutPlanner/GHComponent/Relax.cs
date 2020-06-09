using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino.Geometry;
using System.Linq;
using myRhinoWrapper;
using RhinoInside.Unity;

public class Relax : MonoBehaviour
{
    List<Line> segments = new List<Line>();
    List<Vector3d> vectors = new List<Vector3d>();
    List<double>  counts = new List<double>();

    public void Compute(ref List<Point3d> centers, double minDist, Polyline region, double moveStep)
    {
        segments.Clear();
        for (int i = 0; i < region.SegmentCount; i++)
        {
            segments.Add(region.SegmentAt(i));
        }


        for (int k = 0; k < 1000; k++)
        {
        
            vectors.Clear();
            counts.Clear();

            for (int i = 0; i < centers.Count; i++)
            {
                vectors.Add(Vector3d.Zero);
                counts.Add(0.0);


                for (int j = 0; j < centers.Count; j++)
                {
                    if (i == j) continue;
                    Vector3d vec = centers[i] - centers[j];
                    double dist = vec.Length;

                    if (dist > minDist) continue;
                    vec.Unitize();
                    vectors[i] += vec;
                    counts[i] += 1.0;
                }
            }

       

            //to in wall
            for (int i = 0; i < centers.Count; i++)
            {
                if (counts[i] != 0)
                {
                    centers[i] += (vectors[i] / counts[i]) * moveStep;
                }

                if (RhinoWrapper.IsInside(centers[i], region)) continue;
                counts[i]++;
                var nearWall = region.ClosestPoint(centers[i]);
               // var vector = nearWall - centers[i];
                centers[i] = nearWall;
                /*
                vector.Unitize();
                centers[i] += vector * minDist;
               */
            }
        }
    }
}