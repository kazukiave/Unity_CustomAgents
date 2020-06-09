using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rectangle : MonoBehaviour
{
    GameObject rect;
    LineRenderer lineRenderer;

    Vector3[] pts;
    Vector3 max;
    Vector3 min;

    Vector3 pt0;
    Vector3 pt1;
    Vector3 pt2;
    Vector3 pt3;


    public Rectangle(float x, float y, Vector3 center)
    {
         pts =new Vector3[5];
         rect = new GameObject("Rectangle");

         max = new Vector3( x / 2f, y / 2f, 0);
         min = new Vector3(-x / 2f, -y / 2f, 0);
         pts[0] = new Vector3(min.x, min.y, 0);
         pts[1] = new Vector3(min.x, max.y, 0);
         pts[2] = new Vector3(max.x, max.y, 0);
         pts[3] = new Vector3(max.x, min.y, 0);
         pts[4] = new Vector3(min.x, min.y, 0);
    }

    public List<int> ContainsPts(List<Vector3> pts)
    {
        var rtnList = new List<int>();
        for (int i = 0; i < pts.Count; i++)
        {
            if (min.x < pts[i].x && pts[i].x < max.x && min.y < pts[i].y && pts[i].y < max.y)
            {
                rtnList.Add(i);
            }
        }

        return rtnList;
    }

    public void Show()
    {
        lineRenderer = rect.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 5;
        lineRenderer.SetPositions(pts);
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }
}
