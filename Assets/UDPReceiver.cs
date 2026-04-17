using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Globalization; // KRİTİK: Nokta/Virgül ayrımı için gerekli

public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5005;

    // --- VERİ AYRIŞTIRMA İÇİN GEREKLİ DEĞİŞKENLER ---
    public float handX; // Update içinde kullanacağımız X koordinatı
    public float handY; // Update içinde kullanacağımız Y koordinatı
    public string handLabel = ""; // Sağ mı sol mu?
    
    private string lastReceivedPacket = "";
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
                byte[] data = client.Receive(ref anyIP); 
                lastReceivedPacket = Encoding.UTF8.GetString(data);

                // --- 2. ADIM: VERİ AYRIŞTIRMA VE DÖNÜŞTÜRME ---
                string[] parts = lastReceivedPacket.Split(','); // Virgülle ayır

                if (parts.Length == 3) // Label, X ve Y geldiğinden emin ol
                {
                    try
                    {
                        handLabel = parts[0]; // "Left" veya "Right"
                        
                        // float.Parse ile metni sayıya çeviriyoruz
                        // InvariantCulture sayesinde bilgisayarın dil ayarı ne olursa olsun (.) ondalık sayılır.
                        handX = float.Parse(parts[1], CultureInfo.InvariantCulture);
                        handY = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Sayıya dönüştürme hatası: " + e.Message);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (isRunning) Debug.Log("UDP Hatası: " + e.ToString());
        }
    }

    // --- VERİ GÜVENLİĞİ: Update ana döngüdür ve görsel hareket burada yapılır ---
    void Update()
    {
        // Python'dan gelen 0-1 arası veriyi sahne boyutuna oranla
        // MediaPipe'da sol üst (0,0), sağ alt (1,1)'dir. Unity koordinatlarına uyarla:
        float finalX = (handX - 0.5f) * 10f; 
        float finalY = (0.5f - handY) * 7f; // Y eksenini ters çeviriyoruz

        // Kutuyu yeni pozisyona ışınla
        transform.position = new Vector3(finalX, finalY, 0);
    }

    void OnDisable() { StopUDP(); }
    void OnApplicationQuit() { StopUDP(); }

    void StopUDP()
    {
        isRunning = false;
        if (client != null) client.Close();
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Abort();
        Debug.Log("UDP Bağlantısı Kapatıldı.");
    }
}