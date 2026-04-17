using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Globalization;

public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5005;

    [Header("Duvar Boyutları (Metre)")]
    public float wallWidth = 4.0f;  // Senin 4 metrelik duvarın
    public float wallHeight = 4.0f; // Senin 4 metrelik duvarın

    [Header("Hareket Ayarları")]
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.15f; // Hareketin yumuşaklığı (0: Çok yavaş, 1: Işınlanma)
    public bool invertX = false;      // Eğer elin sağa gidince kutu sola giderse bunu işaretle

    // Veri değişkenleri
    private float handX;
    private float handY;
    public string handLabel = "";
    
    // Yumuşatma için hedef ve mevcut pozisyon
    private Vector3 targetPosition;
    private bool isRunning = false;

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
                string lastReceivedPacket = Encoding.UTF8.GetString(data);

                string[] parts = lastReceivedPacket.Split(',');

                if (parts.Length == 3)
                {
                    try
                    {
                        handLabel = parts[0];
                        
                        // Ham veriyi al (0 ile 1 arası)
                        float rawX = float.Parse(parts[1], CultureInfo.InvariantCulture);
                        float rawY = float.Parse(parts[2], CultureInfo.InvariantCulture);

                        // --- 3. ADIM: SENKRONİZASYON VE ÖLÇEKLENDİRME (MAPPING) ---
                        // (rawX - 0.5f) -> Veriyi merkeze çeker (-0.5 ile +0.5 arası yapar)
                        // wallWidth ile çarpınca -2m ile +2m arasına yayar (Toplam 4m)
                        float processedX = (rawX - 0.5f) * wallWidth;
                        
                        // MediaPipe'da 0 yukarısı olduğu için (0.5 - rawY) yönü yukarı çevirir
                        float processedY = (0.5f - rawY) * wallHeight;

                        if (invertX) processedX *= -1;

                        // Yeni hedef noktayı belirle
                        targetPosition = new Vector3(processedX, processedY, 0);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Dönüştürme hatası: " + e.Message);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (isRunning) Debug.Log("UDP Hatası: " + e.ToString());
        }
    }

    void Update()
    {
        // --- EKSTRA: HAREKET YUMUŞATMA (LERP) ---
        // Kutuyu mevcut yerinden hedef yere 'smoothSpeed' hızıyla kaydırarak götürür.
        // Bu, elindeki küçük titremelerin kutuya yansımasını engeller.
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
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