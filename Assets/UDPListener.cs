using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPListener : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5005;

    public float playerX;
    public float playerY;
    private string lastReceivedPacket = "";

    void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        try
        {
            client = new UdpClient(port);
            while (true)
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                lastReceivedPacket = Encoding.UTF8.GetString(data);

                string[] parts = lastReceivedPacket.Split(',');
                if (parts.Length == 2)
                {
                    playerX = float.Parse(parts[0]);
                    playerY = float.Parse(parts[1]);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    void Update()
    {
        transform.position = new Vector3(playerX * 5, playerY * 4, 0);
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
}