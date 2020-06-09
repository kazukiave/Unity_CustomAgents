using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;


public class UDPSender : MonoBehaviour
{
    private string _address = "";

    public int _port = 6666;

    private UdpClient _udp;

    private float _sendTime = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        _address = GetServerIPAddress();
        
        _udp = new UdpClient();
        _udp.Connect(IPAddress.Parse(_address), _port);
    }

    public void UdpSend(string content)
    {
        byte[] dgram = Encoding.UTF8.GetBytes(content);
        _udp.Send(dgram, dgram.Length);
    }

    public void OtherUdpSned(string content, int port)
    {
        using (var udp = new UdpClient())
        {
            string hostname = Dns.GetHostName();

            byte[] dgram = Encoding.UTF8.GetBytes(content);
            udp.Send(dgram, dgram.Length, hostname, port);
        }
    }

    private void OnApplicationQuit()
    {
        _udp.Close();
    }

    public string GetServerIPAddress()
    {

        string hostAddress = "";
        string hostname = Dns.GetHostName();

        // ホスト名からIPアドレスを取得します.
        IPAddress[] adrList = Dns.GetHostAddresses(hostname);

        for (int i = 0; i < adrList.Length; ++i)
        {
            string addr = adrList[i].ToString();
            string[] c = addr.Split('.');

            if (c.Length == 4)
            {
                hostAddress = addr;
                break;
            }
        }

        return hostAddress;
    }
}
