using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Text;

public class ChairAcademy : Academy
{
    public List<GameObject> spheres;
    public List<Vector3> spheresPos;
    private UDPSender udpSender;

    private int stepCount = 0;

    public override void InitializeAcademy()
    {
        spheres = new List<GameObject>();
        udpSender = GetComponent<UDPSender>();
        Debug.Log("AcademyInitialize");
    }


    public override void AcademyStep()
    {
        
    }

    public override void AcademyReset()
    {
        Debug.Log("AcademyReset");
    }

    public void SavePosition()
    {
        TextWriter textWriter = GetComponent<TextWriter>();
        var text = "";

        foreach (GameObject sphere in spheres)
        {
            if (sphere == null) continue;
            text += sphere.transform.position.ToString();
        }

        textWriter.TextWrite(text);

    }

  
    private void UDPSend()
    {
        var sb = new StringBuilder();
    
        foreach (GameObject sphere in spheres)
        {
            if (spheres.Count == 0) break;
            if (sphere == null) break;

           sb.Append(sphere.transform.position.ToString());
        }

        udpSender.UdpSend(sb.ToString());
    }
   
}
