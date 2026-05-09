using UnityEngine;
using TMPro;

public class HavalananYazi : MonoBehaviour
{
    public float yukselmeHizi = 3f;
    public float yokOlmaSuresi = 1.5f;

    private TextMeshPro textMesh;
    private Color baslangicRengi;
    private float zaman = 0;

    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null) baslangicRengi = textMesh.color;

        Destroy(gameObject, yokOlmaSuresi); // Havalandıktan 1.5 saniye sonra sistemden silinsin (Optimizasyon)
    }

    void Update()
    {
        // Yukarı doğru yavaşça süzül
        transform.Translate(Vector3.up * yukselmeHizi * Time.deltaTime, Space.World);

        // Hoca Şovu: Yazı 3D dünyada döneceği için her zaman kameraya doğru baksın ki oyuncu okuyabilsin!
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        // Yavaşça şeffaflaşarak silinme efekti
        zaman += Time.deltaTime;
        if (textMesh != null)
        {
            float oran = zaman / yokOlmaSuresi;
            textMesh.color = new Color(baslangicRengi.r, baslangicRengi.g, baslangicRengi.b, 1f - oran);
        }
    }
}
