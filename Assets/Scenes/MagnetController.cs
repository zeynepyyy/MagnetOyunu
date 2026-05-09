using UnityEngine;
using Unity.Netcode; // Yeni ekledik

public class MagnetController : NetworkBehaviour // MonoBehaviour yerine NetworkBehaviour
{
    public float cekimGucu = 500f; // Gücü artırdık
    public float etkiMesafesi = 6f;
    public bool oyundaMi = false;
    public bool p1inTasiMi;
    public bool cezaKesildi = false;
    public NetworkVariable<int> atanID = new NetworkVariable<int>(0); // Taşı atan oyuncunun ID'si
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Fizik hesaplamaları her zaman FixedUpdate içinde ve SADECE Sunucuda yapılır
    void FixedUpdate()
    {
        if (!IsServer || !oyundaMi) return; // Sadece server hesaplasın

        MagnetController[] digerleri = Object.FindObjectsByType<MagnetController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var diger in digerleri)
        {
            if (diger == this || !diger.oyundaMi) continue;
            float mesafe = Vector3.Distance(transform.position, diger.transform.position);
            if (mesafe < etkiMesafesi)
            {
                Vector3 yon = (transform.position - diger.transform.position).normalized;
                rb.AddForce(-yon * (cekimGucu / (mesafe * mesafe)));
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || !oyundaMi || cezaKesildi) return;

        MagnetController digerM = collision.gameObject.GetComponent<MagnetController>();
        if (digerM != null && digerM.oyundaMi && !digerM.cezaKesildi)
        {
            cezaKesildi = true;
            digerM.cezaKesildi = true;

            if (OyunYoneticisi.Singleton != null)
            {
                // Kritik Düzeltme: Cezayı "son oynayan" yerine doğrudan taşı atan kişiye (atanID) kesiyoruz
                OyunYoneticisi.Singleton.TaslarCarpisti(
                    GetComponent<NetworkObject>().NetworkObjectId,
                    digerM.GetComponent<NetworkObject>().NetworkObjectId,
                    atanID.Value
                );
            }
        }
    }

    // Renk ayarlama gibi görsel işleri tüm istemcilerde (Client) çalıştırabiliriz
    public void ZeminRenginiAyarla(bool p1Mi)
    {
        p1inTasiMi = p1Mi;
        
        // FİZİĞİ AKTİF ET: Taş yere indiğinde hareket edebilmesi için kilidi açıyoruz
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
    }
}
