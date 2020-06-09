using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhino.Geometry;
using System.Linq;
using myRhinoWrapper;


public class ShapedGrid : MonoBehaviour
{
    public void Compute(int gridSize, int x_Ex, int y_Ex, int offsetValue, int offsetValue2, int reduceNum, int minRoomNum , Point3d center
        , ref List<Rhino.Geometry.Point3d> shapedGrid, ref PolylineCurve shapeCrv, ref Polyline offsetShapeCrv, ref Rectangle3d originalShape)
    {
        var grid = RhinoWrapper.MakeGrid(x_Ex, y_Ex, gridSize);
        var rectMain = RhinoWrapper.MakeRect(center, x_Ex, y_Ex, gridSize);
        var rectMainCrv = rectMain.ToPolyline().ToPolylineCurve();
        originalShape = rectMain;

        //���炷�����̎l�p�`�����邽�߂̌���corner
        var corners = new Rhino.Geometry.Point3d[4];
        for (int i = 0; i < 4; i++)
        {
            corners[i] = rectMain.Corner(i);
        }

        //�������߂̎l�p�`�����͈�
        var rectSub = RhinoWrapper.MakeRect(center, x_Ex, y_Ex, gridSize, offsetValue);
        var rectSub2 = RhinoWrapper.MakeRect(center, x_Ex, y_Ex, gridSize, offsetValue2);
    

        var populate = RhinoWrapper.RandomPt(rectSub, reduceNum);
        var randPts = populate.Where(pt => RhinoWrapper.IsInside(pt, rectSub2.ToPolyline()) == false).ToList();

        //�_���߂��Ƃ���ɂ���ƃI�t�Z�b�g�����܂��@�\���Ȃ��B
        for (int i = 0; i < randPts.Count; i++)
        {
            for (int j = 0; j < randPts.Count; j++)
            {
                if (i == j) continue;
                if (randPts[i].DistanceTo(randPts[j]) < gridSize * (minRoomNum + 1))
                {
                    randPts.RemoveAt(j);
                    j--;
                }
            }
        }

        //Reduce Rects������Ă�
        var reduceRects = new List<Rhino.Geometry.PolylineCurve>();
        var planeXY = new Rhino.Geometry.Plane(Point3d.Origin, Vector3d.ZAxis);
        for (int i = 0; i < randPts.Count; i++)
        {
            var pc = new Rhino.Geometry.PointCloud(corners);
            int closestIdx = pc.ClosestPoint(randPts[i]);
            var reduceRect = new Rectangle3d(planeXY, randPts[i], corners[closestIdx]);
            var polyCrv = reduceRect.ToPolyline().ToPolylineCurve();
            reduceRects.Add(polyCrv);
        }

        
        var shape = Curve.CreateBooleanDifference(rectMainCrv, reduceRects, 0.1);
        offsetShapeCrv = null;
        //���l�p�`�łȂ��`���ł��Ƃ�
        if (shape.Length > 0)
        {
            shape[0].TryGetPolyline(out Polyline polyShape);
           
            //ref
            shapedGrid = grid.Where(pt => RhinoWrapper.IsInside(pt, polyShape)).ToList();
            
            //ref
            shapeCrv = polyShape.ToPolylineCurve();

            //ref
            var plane = new Rhino.Geometry.Plane(AreaMassProperties.Compute(shapeCrv).Centroid, Vector3d.ZAxis);
            shapeCrv.Offset(plane, (-gridSize * minRoomNum) , 1, CurveOffsetCornerStyle.Sharp)[0].TryGetPolyline(out offsetShapeCrv);
            if (offsetShapeCrv == null)
            {
                offsetShapeCrv = new Rectangle3d(plane, 1, 1).ToPolyline();
            }
        }
        else//���l�p�`���c�����Ƃ��B�i�Ђ����߂�Rectangle���ł��Ȃ�)
        {
            shapedGrid = grid.Where(pt => RhinoWrapper.IsInside(pt, rectMainCrv.ToPolyline())).ToList();

            //ref
            shapeCrv = rectMainCrv.ToPolyline().ToPolylineCurve();

            //ref
            var plane = new Rhino.Geometry.Plane(AreaMassProperties.Compute(shapeCrv).Centroid, Vector3d.ZAxis);
           shapeCrv.Offset(plane, (-gridSize * minRoomNum), 1, CurveOffsetCornerStyle.Sharp)[0].TryGetPolyline(out offsetShapeCrv);
            if (offsetShapeCrv == null)
            {
                offsetShapeCrv = new Rectangle3d(plane, 1, 1).ToPolyline();
            }
        }
    }
}


