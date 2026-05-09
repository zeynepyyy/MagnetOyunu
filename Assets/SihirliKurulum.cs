#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

public class SihirliKurulum : EditorWindow
{
    [MenuItem("Magnet/4 Oyuncu Sistemini Kur (Sihirli Buton)")]
    public static void KurulumYap()
    {
        // 1. Masaları Bul ve Ayarla
        GameObject p3Masa = GameObject.Find("MaviMasa (1)"); 
        GameObject p4Masa = GameObject.Find("KirmiziMasa (1)");
        
        if (p3Masa == null) p3Masa = GameObject.Find("YesilMasa_P3");
        if (p4Masa == null) p4Masa = GameObject.Find("SariMasa_P4");

        if (p3Masa != null)
        {
            p3Masa.name = "YesilMasa_P3";
            p3Masa.transform.position = new Vector3(0, -0.1f, 10f);
            p3Masa.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        if (p4Masa != null)
        {
            p4Masa.name = "SariMasa_P4";
            p4Masa.transform.position = new Vector3(0, -0.1f, -10f);
            p4Masa.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        // 2. Taşları Oluştur
        GameObject sablonTas = GameObject.Find("MaketTas");
        if (sablonTas == null) sablonTas = GameObject.Find("MaketTas (0)");

        if (sablonTas != null)
        {
            // Eski P3 ve P4 taşlarını temizle (tekrar basılırsa üst üste binmesin)
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                if (go.name.StartsWith("MaketTas_P3") || go.name.StartsWith("MaketTas_P4")) {
                    DestroyImmediate(go);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                float xOffset = -4.5f + (i * 1f);
                
                GameObject t3 = Instantiate(sablonTas);
                t3.name = "MaketTas_P3 (" + i + ")";
                t3.transform.position = new Vector3(xOffset, sablonTas.transform.position.y, 10f);

                GameObject t4 = Instantiate(sablonTas);
                t4.name = "MaketTas_P4 (" + i + ")";
                t4.transform.position = new Vector3(xOffset, sablonTas.transform.position.y, -10f);
            }
        }

        // 3. UI Panellerini Çoğalt ve Yöneticisine Bağla
        OyunYoneticisi yonetici = Object.FindAnyObjectByType<OyunYoneticisi>();
        if (yonetici != null && yonetici.p1IsimText != null)
        {
            Transform p1Panel = yonetici.p1IsimText.transform.parent;
            Transform p2Panel = yonetici.p2IsimText != null ? yonetici.p2IsimText.transform.parent : null;
            Transform p3Panel = yonetici.p3IsimText != null ? yonetici.p3IsimText.transform.parent : GameObject.Find("P3_Arayuz")?.transform;
            Transform p4Panel = yonetici.p4IsimText != null ? yonetici.p4IsimText.transform.parent : GameObject.Find("P4_Arayuz")?.transform;

            if (p3Panel == null)
            {
                p3Panel = Instantiate(p1Panel, p1Panel.parent);
                p3Panel.name = "P3_Arayuz";
            }
            if (p4Panel == null)
            {
                p4Panel = Instantiate(p2Panel != null ? p2Panel : p1Panel, p1Panel.parent);
                p4Panel.name = "P4_Arayuz";
            }

            // P1 (Sol Üst)
            RectTransform r1 = p1Panel.GetComponent<RectTransform>();
            r1.anchorMin = new Vector2(0f, 1f); r1.anchorMax = new Vector2(0f, 1f); r1.pivot = new Vector2(0f, 1f);
            r1.anchoredPosition = new Vector2(50, -50); r1.localScale = new Vector3(1.1f, 1.1f, 1f);

            // P2 (Sağ Üst)
            if (p2Panel != null) {
                RectTransform r2 = p2Panel.GetComponent<RectTransform>();
                r2.anchorMin = new Vector2(1f, 1f); r2.anchorMax = new Vector2(1f, 1f); r2.pivot = new Vector2(1f, 1f);
                r2.anchoredPosition = new Vector2(-50, -50); r2.localScale = new Vector3(1.1f, 1.1f, 1f);
            }

            // P3 (Sol Alt)
            RectTransform r3 = p3Panel.GetComponent<RectTransform>();
            r3.anchorMin = new Vector2(0f, 0f); r3.anchorMax = new Vector2(0f, 0f); r3.pivot = new Vector2(0f, 0f);
            r3.anchoredPosition = new Vector2(50, 50); r3.localScale = new Vector3(1.1f, 1.1f, 1f);
            UnityEngine.UI.Image img3 = p3Panel.GetComponent<UnityEngine.UI.Image>();
            if (img3 != null) img3.color = new Color(0.2f, 1f, 0.2f, 1f); // Yeşil Tint
            
            // P4 (Sağ Alt)
            RectTransform r4 = p4Panel.GetComponent<RectTransform>();
            r4.anchorMin = new Vector2(1f, 0f); r4.anchorMax = new Vector2(1f, 0f); r4.pivot = new Vector2(1f, 0f);
            r4.anchoredPosition = new Vector2(-50, 50); r4.localScale = new Vector3(1.1f, 1.1f, 1f);
            UnityEngine.UI.Image img4 = p4Panel.GetComponent<UnityEngine.UI.Image>();
            if (img4 != null) img4.color = new Color(1f, 1f, 0.2f, 1f); // Sarı Tint

            // Üstteki gereksiz uzun siyah şeridi (UstPanel arka planı) gizleyelim
            GameObject ustPanel = GameObject.Find("UstPanel");
            if (ustPanel != null)
            {
                UnityEngine.UI.Image ustBg = ustPanel.GetComponent<UnityEngine.UI.Image>();
                if (ustBg != null) { ustBg.enabled = false; ustBg.color = Color.clear; }
                
                // Geri (Çıkış) Butonu Oluşturma
                Transform exitBtnTransform = ustPanel.transform.Find("CikisButonu");
                if (exitBtnTransform == null)
                {
                    GameObject exitBtnObj = new GameObject("CikisButonu");
                    exitBtnObj.transform.SetParent(ustPanel.transform, false);
                    RectTransform eRect = exitBtnObj.AddComponent<RectTransform>();
                    eRect.anchorMin = new Vector2(0, 1); eRect.anchorMax = new Vector2(0, 1);
                    eRect.pivot = new Vector2(0, 1); eRect.anchoredPosition = new Vector2(20, -20);
                    eRect.sizeDelta = new Vector2(100, 40);
                    
                    UnityEngine.UI.Image eImg = exitBtnObj.AddComponent<UnityEngine.UI.Image>();
                    eImg.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
                    
                    UnityEngine.UI.Button eBtn = exitBtnObj.AddComponent<UnityEngine.UI.Button>();
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(eBtn.onClick, yonetici.OdadanCik);
                    
                    GameObject eTextObj = new GameObject("Text");
                    eTextObj.transform.SetParent(exitBtnObj.transform, false);
                    RectTransform etRect = eTextObj.AddComponent<RectTransform>();
                    etRect.anchorMin = Vector2.zero; etRect.anchorMax = Vector2.one;
                    etRect.sizeDelta = Vector2.zero;
                    
                    TextMeshProUGUI eText = eTextObj.AddComponent<TextMeshProUGUI>();
                    eText.text = "ÇIKIŞ";
                    eText.color = Color.white;
                    eText.alignment = TextAlignmentOptions.Center;
                    eText.fontSize = 20;
                    eText.fontStyle = FontStyles.Bold;
                }
            }

            // Giriş Paneli (İsim kutularını teke düşürme)
            if (yonetici.p2IsimInput != null)
            {
                yonetici.p2IsimInput.gameObject.SetActive(false); // 2. kutuyu gizle
            }
            if (yonetici.p1IsimInput != null)
            {
                RectTransform inRect = yonetici.p1IsimInput.GetComponent<RectTransform>();
                inRect.anchorMin = new Vector2(0.5f, 0.5f);
                inRect.anchorMax = new Vector2(0.5f, 0.5f);
                inRect.pivot = new Vector2(0.5f, 0.5f);
                inRect.anchoredPosition = new Vector2(0, 50);
                inRect.sizeDelta = new Vector2(300, 60);
                
                TextMeshProUGUI placeholder = yonetici.p1IsimInput.placeholder as TextMeshProUGUI;
                if (placeholder != null) placeholder.text = "İSMİNİZİ GİRİN...";
            }

            TextMeshProUGUI[] p3Texts = p3Panel.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI[] p4Texts = p4Panel.GetComponentsInChildren<TextMeshProUGUI>();

            foreach (var t in p3Texts) {
                if (t.name.ToLower().Contains("isim") || t.text.Contains("OYUNCU")) { yonetici.p3IsimText = t; t.text = "OYUNCU 3"; }
                else yonetici.p3TasText = t;
            }
            foreach (var t in p4Texts) {
                if (t.name.ToLower().Contains("isim") || t.text.Contains("OYUNCU")) { yonetici.p4IsimText = t; t.text = "OYUNCU 4"; }
                else yonetici.p4TasText = t;
            }
            
            EditorUtility.SetDirty(yonetici);
        }

        Debug.Log("SİHİRLİ KURULUM TAMAMLANDI! Sahne 4 oyuncuya başarıyla uyarlandı.");
    }
}
#endif
