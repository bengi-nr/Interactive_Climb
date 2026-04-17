using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5005;
    public string lastReceivedPacket = "";

    // Thread'in çalışıp çalışmadığını kontrol etmek için
    bool isRunning = false;

    void Start()
    {
        isRunning = true;
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        try
        {
            client = new UdpClient(port);
            while (isRunning)
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP); // Burada bekler (Blocking)
                lastReceivedPacket = Encoding.UTF8.GetString(data);

                // Unity konsolunda görmek isterseniz (Opsiyonel)
                // Debug.Log("Gelen Veri: " + lastReceivedPacket);
            }
        }
        catch (System.Exception e)
        {
            // Thread kapatıldığında buraya bir exception düşebilir, bu normaldir.
            if (isRunning) Debug.Log("UDP Hatası: " + e.ToString());
        }
    }

    // KRİTİK KISIM: Oyun durduğunda veya bu script devre dışı kaldığında çalışır
    void OnDisable()
    {
        StopUDP();
    }

    void OnApplicationQuit()
    {
        StopUDP();
    }

    void StopUDP()
    {
        isRunning = false;

        if (client != null)
        {
            client.Close(); // Portu serbest bırakır, Receive metodu hata fırlatıp durur
        }

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort(); // Thread'i zorla durdurur
        }

        Debug.Log("UDP Bağlantısı Kapatıldı ve Port Serbest Bırakıldı.");
    }
}