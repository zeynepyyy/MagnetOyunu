using UnityEngine;
using System.Collections;

public class KameraSallama : MonoBehaviour
{
    private Vector3 orijinalPozisyon;

    void Start()
    {
        orijinalPozisyon = transform.localPosition;
    }

    // Bu fonksiyonu dışarıdan çağıracağız
    public void Salla(float sure, float siddet)
    {
        StartCoroutine(SallamaEfekti(sure, siddet));
    }

    IEnumerator SallamaEfekti(float sure, float siddet)
    {
        float gecenSure = 0f;

        while (gecenSure < sure)
        {
            // Rastgele küçük sarsıntılar oluştur
            float x = Random.Range(-1f, 1f) * siddet;
            float y = Random.Range(-1f, 1f) * siddet;

            transform.localPosition = new Vector3(orijinalPozisyon.x + x, orijinalPozisyon.y + y, orijinalPozisyon.z);

            gecenSure += Time.deltaTime;
            yield return null; // Bir sonraki kareye kadar bekle
        }

        // Sallanma bitince kamerayı eski yerine oturt
        transform.localPosition = orijinalPozisyon;
    }
}