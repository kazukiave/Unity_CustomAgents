using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class MeshSender : MonoBehaviour
{
    private Mesh mesh;
    private UDPSender udpSneder;

    // Start is called before the first frame update
    void Start()
    {
        mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        udpSneder = GetComponent<UDPSender>();
    }

    // Update is called once per frame
    void Update()
    {
        var sb = new StringBuilder();

        foreach (var data in mesh.vertices)
        {
            if (data == null) break;

            sb.Append(data.ToString());
        }
        udpSneder.OtherUdpSned(sb.ToString(), 3333);
    }
}
