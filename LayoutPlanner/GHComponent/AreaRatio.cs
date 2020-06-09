using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino.Geometry;
using System.Linq;
using myRhinoWrapper;

public class AreaRatio : MonoBehaviour
{
    List<double> areaList = new List<double>();
    List<double> ratioList = new List<double>();

    public  void Compute(PolylineCurve targetCrv, double ratioMin, int roomNum, ref List<double> areaSize, ref List<double> areaRatio)
    {
        areaList.Clear();
        ratioList.Clear();

        double targetArea = Rhino.Geometry.AreaMassProperties.Compute(targetCrv, 1).Area;
        double rationMax = 1f / (float)(roomNum);

        var rand = new System.Random();
        //ratio List ‚É’l‚ð“ü‚ê‚Ä‚¢‚­
        for (int i = 0; i < roomNum - 1; i++)
        {
            var randVal = rand.NextDouble().Remap(0, 1.0, ratioMin, rationMax);
            ratioList.Add(randVal);
        }
        ratioList.Add(1 - ratioList.Sum());

        areaList = ratioList.Select(ratio => ratio * targetArea).ToList();
        areaList.Jitter();

        areaRatio = ratioList;
        areaSize = areaList;

    }
}
