using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public UDPReceiver udpSource;

    void Update()
    {
        // 1. Güvenlik: Kaynak var mý ve veri boþ mu?
        if (udpSource != null && !string.IsNullOrEmpty(udpSource.lastReceivedPacket))
        {
            try
            {
                // 2. Python'dan gelen "0.5,0.8" metnini virgülden ayýrýyoruz
                string[] data = udpSource.lastReceivedPacket.Split(',');

                if (data.Length >= 2)
                {
                    // Metinleri sayýya çeviriyoruz
                    float xPercent = float.Parse(data[0]);
                    float yPercent = float.Parse(data[1]);

                    // 3. Unity'nin anlayacaðý dünya koordinatlarýna çeviriyoruz
                    // 10f deðeri kameradan olan uzaklýktýr, sabit kalsýn.
                    Vector3 screenPoint = new Vector3(xPercent, 1f - yPercent, 10f);
                    Vector3 worldPos = Camera.main.ViewportToWorldPoint(screenPoint);

                    // 4. Karakteri yeni yerine ýþýnlýyoruz (Z her zaman 0!)
                    transform.position = new Vector3(worldPos.x, worldPos.y, 0);
                }
            }
            catch
            {
                // Eðer Python hatalý veri gönderirse oyunun çökmesini engeller
            }
        }
    }
}