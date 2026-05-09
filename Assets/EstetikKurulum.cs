#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class EstetikKurulum : EditorWindow
{
    [MenuItem("Magnet/TASLARI MASAYA DIZ VE DUZELT!")]
    public static void TaslariDiz()
    {
        // 1. TAŞLARI BUL VE NİZAMİ DİZ
        int stoneCount = 0;
        int p1 = 0, p2 = 0, p3 = 0, p4 = 0;
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        // En güvenli materyal
        Material safeGold = new Material(Shader.Find("Hidden/Internal-Colored"));
        safeGold.color = new Color(0.9f, 0.8f, 0.4f);

        foreach (GameObject go in allObjects) {
            if (EditorUtility.IsPersistent(go)) continue;
            string n = go.name.ToLower();
            
            if (n.Contains("tas") || n.Contains("stone") || go.GetComponent<MagnetController>() != null || n.StartsWith("maket")) {
                if (n.Contains("manager") || n.Contains("canvas") || n.Contains("arena")) continue;

                go.SetActive(true);
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                
                // Oyuncu önlerine nizami diz (Üst üste binmesinler)
                if (p1 < 10) { go.transform.position = new Vector3(-2f + (p1 * 0.5f), 0.25f, 4.5f); p1++; }
                else if (p2 < 10) { go.transform.position = new Vector3(-2f + (p2 * 0.5f), 0.25f, -4.5f); p2++; }
                else if (p3 < 10) { go.transform.position = new Vector3(-4.5f, 0.25f, -2f + (p3 * 0.5f)); p3++; }
                else if (p4 < 10) { go.transform.position = new Vector3(4.5f, 0.25f, -2f + (p4 * 0.5f)); p4++; }
                
                var mr = go.GetComponent<MeshRenderer>();
                if (mr != null) mr.material = safeGold;
                stoneCount++;
            }
        }

        // 2. MASAYI YERİNE KOY VE SABİTLE
        GameObject masa = GameObject.Find("SonsuzMasa");
        if (masa != null) {
            masa.SetActive(true);
            masa.transform.position = Vector3.zero;
            masa.transform.localScale = Vector3.one;
        }

        Debug.Log("TOPLAM " + stoneCount + " ADET TAS MASAYA DIZILDI.");
    }
}
#endif
