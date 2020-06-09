using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino.Geometry;
using System.Linq;

public class Bbox 
{
  public Vector3d Max { get; set; }
  public Vector3d Min { get; set; }
  public Vector3d Cent { get; set; }

  public Bbox()
  {

  }

  public Bbox(List<Vector3d> positions)
  {
    var xList = new List<double>();
    var yList = new List<double>();
    var zList = new List<double>();


    for (int i = 0; i < positions.Count; i++)
    {
      xList.Add(positions[i].X);
      yList.Add(positions[i].Y);
      zList.Add(positions[i].Z);
    }
    Max = new Vector3d(xList.Max(), yList.Max(), zList.Max());
    Min = new Vector3d(xList.Min(), yList.Min(), zList.Min());
    Cent = new Vector3d((Max + Min) / 2);
  }

}
