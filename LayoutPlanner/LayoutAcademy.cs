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

public class LayoutAcademy : Academy
{
    List<GameObject> layoutAreas = new List<GameObject>();
    public override void InitializeAcademy()
    {
        //それぞれのAreaにInitを行わせる
        layoutAreas = GameObject.FindGameObjectsWithTag("layoutArea").ToList();
      
        foreach(var area in layoutAreas)
        {
            area.GetComponent<LayoutArea>().Init();
        }
    }

    public override void AcademyReset()
    {
       
    }


    public override void AcademyStep()
    {
       /*
          foreach (var area in layoutAreas)
        {
            area.GetComponent<LayoutArea>().ReComputeArea();
        }
       */
    }
}
