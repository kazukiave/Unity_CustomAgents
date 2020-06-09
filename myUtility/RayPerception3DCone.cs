using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RayPerception3DCone : MonoBehaviour
{
    Vector3 m_EndPosition;
    RaycastHit m_Hit;
    private float[] m_SubList;
    private List<float> m_PerceptionBuffer = new List<float>();


    public List<float> PerceiveCone(float rayDistance, float[] rayAngles_Hori, float[] rayAngle_Verti,
                                                string[] detectableObjects, float startOffset = 0.0f, float endOffset = 0.0f)
    {
        if (m_SubList == null || m_SubList.Length != detectableObjects.Length + 2)
            m_SubList = new float[detectableObjects.Length + 2];

        m_PerceptionBuffer.Clear();
        m_PerceptionBuffer.Capacity = m_SubList.Length * rayAngles_Hori.Length * rayAngle_Verti.Length;

        // For each ray sublist stores categorical information on detected object
        // along with object distance.
        foreach (var angle_Ver in rayAngle_Verti)
        {
            foreach (var angle_Hori in rayAngles_Hori)
            {
                m_EndPosition = transform.TransformDirection(PolarToCartesian(rayDistance, angle_Hori, angle_Ver));
                m_EndPosition.y += endOffset;

                if (Application.isEditor)
                {
                  Debug.DrawRay(transform.position + new Vector3(0f, startOffset, 0f), m_EndPosition, Color.cyan, 0.01f, true);
                }

                Array.Clear(m_SubList, 0, m_SubList.Length);

                if (Physics.SphereCast(transform.position +
                    new Vector3(0f, startOffset, 0f), 0.5f,
                    m_EndPosition, out m_Hit, rayDistance))
                {
                    for (var i = 0; i < detectableObjects.Length; i++)
                    {
                        if (m_Hit.collider.gameObject.CompareTag(detectableObjects[i]))
                        {
                         //   Debug.Log(detectableObjects[i]);
                            m_SubList[i] = 1;
                            m_SubList[detectableObjects.Length + 1] = m_Hit.distance / rayDistance;
                            break;
                        }
                    }
                }
                else
                {
                    m_SubList[detectableObjects.Length] = 1f;
                }

                AddRangeNoAlloc(m_PerceptionBuffer, m_SubList);
            }
        }

        return m_PerceptionBuffer;
    }

    private void AddRangeNoAlloc<T>(List<T> dst, T[] src)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var item in src)
        {
            dst.Add(item);
        }
    }
    /// <summary>
    /// Converts polar coordinate to cartesian coordinate.
    /// </summary>
    private  Vector3 PolarToCartesian(float radius, float angle_Hori ,float angle_Ver)
    {
        var x = radius * Mathf.Cos(DegreeToRadian(angle_Hori));
        var z = radius * Mathf.Sin(DegreeToRadian(angle_Hori));
        var y = radius * Mathf.Tan(DegreeToRadian(angle_Ver));
      
        return new Vector3(x, y, z);
    }



    private  float DegreeToRadian(float degree)
    {
        return degree * Mathf.PI / 180f;
    }
}
