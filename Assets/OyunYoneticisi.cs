using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;

public class OyunYoneticisi : NetworkBehaviour
{
    [Header("Paneller")]
    public GameObject lobiPaneli;
    public GameObject girisPaneli;
    public GameObject oyunArayuzuPanel;

    [Header("İsimler")]
    public TMP_InputField p1IsimInput;
    public TMP_InputField p2IsimInput;

    [Header("Ağ Değişkenleri")]
    public NetworkVariable<int> oyuncuSayisi = new NetworkVariable<int>(2);
    public NetworkVariable<int> siraKimde = new NetworkVariable<int>(1);
    public NetworkVariable<bool> oyunBasladiNet = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> oyunDurduNet = new NetworkVariable<bool>(false);
    public NetworkVariable<ulong> durduranKisi = new NetworkVariable<ulong>(999);
    public NetworkVariable<int> sonOynayanID = new NetworkVariable<int>(1);

    // Oyuncu 1 (Kırmızı)
    public NetworkVariable<int> p1Tas = new NetworkVariable<int>(10);
    public NetworkVariable<float> p1Sure = new NetworkVariable<float>(60f);
    public NetworkVariable<bool> p1Hazir = new NetworkVariable<bool>(false);
    public NetworkVariable<Unity.Collections.FixedString32Bytes> p1IsimNet = new NetworkVariable<Unity.Collections.FixedString32Bytes>("OYUNCU 1");
    public NetworkVariable<int> p1FizikselCezaSayisi = new NetworkVariable<int>(0);

    // Oyuncu 2 (Mavi)
    public NetworkVariable<int> p2Tas = new NetworkVariable<int>(10);
    public NetworkVariable<float> p2Sure = new NetworkVariable<float>(60f);
    public NetworkVariable<bool> p2Hazir = new NetworkVariable<bool>(false);
    public NetworkVariable<Unity.Collections.FixedString32Bytes> p2IsimNet = new NetworkVariable<Unity.Collections.FixedString32Bytes>("OYUNCU 2");
    public NetworkVariable<int> p2FizikselCezaSayisi = new NetworkVariable<int>(0);

    // Oyuncu 3 (Yeşil)
    public NetworkVariable<int> p3Tas = new NetworkVariable<int>(10);
    public NetworkVariable<float> p3Sure = new NetworkVariable<float>(60f);
    public NetworkVariable<bool> p3Hazir = new NetworkVariable<bool>(false);
    public NetworkVariable<Unity.Collections.FixedString32Bytes> p3IsimNet = new NetworkVariable<Unity.Collections.FixedString32Bytes>("OYUNCU 3");
    public NetworkVariable<int> p3FizikselCezaSayisi = new NetworkVariable<int>(0);

    // Oyuncu 4 (Sarı)
    public NetworkVariable<int> p4Tas = new NetworkVariable<int>(10);
    public NetworkVariable<float> p4Sure = new NetworkVariable<float>(60f);
    public NetworkVariable<bool> p4Hazir = new NetworkVariable<bool>(false);
    public NetworkVariable<Unity.Collections.FixedString32Bytes> p4IsimNet = new NetworkVariable<Unity.Collections.FixedString32Bytes>("OYUNCU 4");
    public NetworkVariable<int> p4FizikselCezaSayisi = new NetworkVariable<int>(0);

    public static OyunYoneticisi Singleton;

    private List<GameObject> p1MaketTaslar = new List<GameObject>();
    private List<GameObject> p2MaketTaslar = new List<GameObject>();
    private List<GameObject> p3MaketTaslar = new List<GameObject>();
    private List<GameObject> p4MaketTaslar = new List<GameObject>();

    [Header("Arayüz Metinleri")]
    public TextMeshProUGUI p1IsimText, p1TasText;
    public TextMeshProUGUI p2IsimText, p2TasText;
    public TextMeshProUGUI p3IsimText, p3TasText;
    public TextMeshProUGUI p4IsimText, p4TasText;
    public TextMeshProUGUI duraklatYazisi;
    public TextMeshProUGUI saatYazisi, devasaSiraYazisi, durumYazisi;
    public Image butonImage;
    public Sprite duraklatSprite, oynatSprite;

    [Header("Görseller")]
    public GameObject gercekMiknatisPrefab;
    public Image arkaPlanImaj;
    public Sprite arkaPlanResmi;

    [Header("Efektler ve Sistem")]
    public GameObject konfetiPrefab;
    public GameObject carpismaEfektiPrefab;
    public AudioClip carpismaSesi;

    public NetworkVariable<bool> tasUcusdaNet = new NetworkVariable<bool>(false); // Sunucu taraflı uçuş kilidi
    private bool oyunBitti = false;
    private Material altinMat, camMat; // Materyalleri global yaptık ki her yerde erişebilelim

    void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        Time.timeScale = 1f;

        Player1ArayuzunuOlustur();
        UILariKoselereSabitle();
        VignetteEfektiOlustur();

#if UNITY_EDITOR
        if (carpismaEfektiPrefab == null) carpismaEfektiPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/KivilcimVFX.prefab");
        if (konfetiPrefab == null) konfetiPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/KonfetiEfekti.prefab");
        if (carpismaSesi == null) carpismaSesi = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Firefly_audio_magnet_click_variation1.wav");
#endif

        if (arkaPlanImaj != null && arkaPlanResmi != null)
        {
            arkaPlanImaj.sprite = arkaPlanResmi;
            arkaPlanImaj.gameObject.SetActive(true);
        }
        if (lobiPaneli) lobiPaneli.SetActive(true);
        if (girisPaneli) girisPaneli.SetActive(false);
        if (oyunArayuzuPanel) oyunArayuzuPanel.SetActive(false);

        GameObject ust = GameObject.Find("UstPanel");
        if (ust) ust.SetActive(false);
        if (devasaSiraYazisi) devasaSiraYazisi.gameObject.SetActive(false);

        TaslariDizVeGrupla();

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += IstemciKoptu;
        }
    }

    void Player1ArayuzunuOlustur()
    {
        // Eğer Player 1 UI silinmişse veya null ise, Player 2'nin UI'ını klonlayıp sol üste yerleştirelim
        if (p1IsimText == null && p2IsimText != null && p2IsimText.transform.parent != null)
        {
            GameObject p2Panel = p2IsimText.transform.parent.gameObject;
            GameObject p1Panel = Instantiate(p2Panel, p2Panel.transform.parent);
            p1Panel.name = "P1_Panel_Otomatik";

            RectTransform p1Rect = p1Panel.GetComponent<RectTransform>();
            if (p1Rect != null)
            {
                // Ekranın sol üst köşesine hizala
                p1Rect.anchorMin = new Vector2(0, 1);
                p1Rect.anchorMax = new Vector2(0, 1);
                p1Rect.pivot = new Vector2(0, 1);
                
                // ÇIKIŞ butonuyla üst üste gelmemesi için biraz aşağı (Y: -150) yerleştir
                p1Rect.anchoredPosition = new Vector2(50f, -150f); 
            }

            // Yeni paneli Player 1 için script'e bağla
            TextMeshProUGUI[] texts = p1Panel.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts.Length > 0) p1IsimText = texts[0];
            if (texts.Length > 1) p1TasText = texts[1];
        }
    }

    void UILariKoselereSabitle()
    {
        void KoselereCek(TextMeshProUGUI txt, Vector2 anchor, Vector2 pivot, Vector2 pos)
        {
            if (txt != null && txt.transform.parent != null)
            {
                RectTransform panelRect = txt.transform.parent.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    // ÖNEMLİ HATA DÜZELTMESİ:
                    // Panelleri direkt Ana Canvas'a bağlarsak Lobi ekranında kapatılamazlar!
                    // Bu yüzden onları kendi ana menülerinde (örn. GameUI) tutmalıyız.
                    // Bunun yerine, içinde bulundukları ana menüyü (Container) tam ekran yapıyoruz ki köşelere ulaşabilsinler.
                    RectTransform containerRect = panelRect.parent.GetComponent<RectTransform>();
                    if (containerRect != null && containerRect.GetComponent<Canvas>() == null)
                    {
                        containerRect.anchorMin = Vector2.zero;
                        containerRect.anchorMax = Vector2.one;
                        containerRect.offsetMin = Vector2.zero;
                        containerRect.offsetMax = Vector2.zero;
                    }
                    
                    panelRect.anchorMin = anchor;
                    panelRect.anchorMax = anchor;
                    panelRect.pivot = pivot;
                    panelRect.anchoredPosition = pos;
                }
            }
        }

        // Çıkış ve Duraklat butonlarıyla üst üste binmemesi için üst panelleri Y ekseninde -100 aşağı kaydırıyoruz.
        // Alt paneller tam köşede kalabilir.
        KoselereCek(p1IsimText, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20f, -100f)); // Sol Üst (Çıkış butonunun altı)
        KoselereCek(p2IsimText, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20f, -100f)); // Sağ Üst (Duraklat butonunun altı)
        KoselereCek(p3IsimText, new Vector2(0, 0), new Vector2(0, 0), new Vector2(20f, 20f));   // Sol Alt
        KoselereCek(p4IsimText, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20f, 20f));  // Sağ Alt
    }

    void VignetteEfektiOlustur()
    {
        Canvas anaCanvas = Object.FindObjectOfType<Canvas>();
        if (anaCanvas == null) return;

        // Post-Processing ayarlarıyla uğraşmadan garantili karartma için prosedürel UI kullanıyoruz
        GameObject vignetteObj = new GameObject("Vignette_Efekti");
        vignetteObj.transform.SetParent(anaCanvas.transform, false);
        vignetteObj.transform.SetAsFirstSibling(); // Diğer UI elemanlarının altında kalsın ki menüleri karartmasın
        
        RectTransform rt = vignetteObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        UnityEngine.UI.Image img = vignetteObj.AddComponent<UnityEngine.UI.Image>();
        img.raycastTarget = false; // Tıklamaları kesinlikle engellememeli!

        // Radyal (Yuvarlak) bir Kararma Dokusu (Texture) oluştur
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Merkeze olan uzaklığı hesapla
                float dist = Vector2.Distance(new Vector2(x, y), center);
                // Merkez (saydam) ile köşeler (siyah) arasında yumuşak geçiş
                float alpha = Mathf.InverseLerp(radius * 0.45f, radius, dist); 
                
                // Karartmanın şiddetini ve yumuşaklığını ayarla (Smoothstep ile sinematik geçiş)
                alpha = Mathf.SmoothStep(0f, 1f, alpha) * 0.75f; // Maksimum %75 karanlık
                
                tex.SetPixel(x, y, new Color(0, 0, 0, alpha));
            }
        }
        tex.Apply();

        img.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void ArenaOlustur()
    {
        // Varsa eski bozuk/mor arenayı tamamen kapatıyoruz
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "Arena_Cerceve" && go.scene.isLoaded) go.SetActive(false);
            if (go.name == "Yeni_Arena_Cerceve" && go.scene.isLoaded) Destroy(go); // Tekrar oluşumu engelle
        }

        GameObject yeniArena = new GameObject("Yeni_Arena_Cerceve");
        yeniArena.transform.position = new Vector3(0, 1.05f, 0); // Masanın üstüne tam otursun

        // Kraliyet Altını Materyali (Global değişkene atadık)
        altinMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        altinMat.color = new Color(1f, 0.8f, 0.2f);
        altinMat.SetFloat("_Metallic", 0.9f);
        altinMat.SetFloat("_Smoothness", 0.85f);

        // Füme Cam Materyali (Global değişkene atadık)
        camMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        camMat.color = new Color(0.1f, 0.1f, 0.1f, 0.65f); // Şık, yarı saydam siyah
        camMat.SetFloat("_Surface", 1); // Transparent
        camMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        camMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        camMat.SetInt("_ZWrite", 0);
        camMat.renderQueue = 3000;
        camMat.SetFloat("_Metallic", 0.5f);
        camMat.SetFloat("_Smoothness", 0.95f);

        // 1. Altın Tepsi (Kusursuz Yuvarlak Dev Alt Taban)
        GameObject altinTepsi = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        altinTepsi.transform.SetParent(yeniArena.transform);
        altinTepsi.transform.localPosition = new Vector3(0, 0, 0);
        altinTepsi.transform.localScale = new Vector3(15f, 0.05f, 15f); // 40 taşlık dev arena
        Destroy(altinTepsi.GetComponent<Collider>()); // Oyun fiziğini bozmasın
        altinTepsi.GetComponent<Renderer>().material = altinMat;

        // 2. Füme Cam Zemin (Altın tepsinin içine oturan tam yuvarlak dev cam)
        GameObject camZemin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        camZemin.transform.SetParent(yeniArena.transform);
        camZemin.transform.localPosition = new Vector3(0, 0.02f, 0); // Altın tepsinin çok hafif üstünde
        camZemin.transform.localScale = new Vector3(14.5f, 0.06f, 14.5f); // Altın kısımdan küçük, muazzam bir altın kenarlık efekti verir
        Destroy(camZemin.GetComponent<Collider>()); // Fizik hatası olmasın
        camZemin.GetComponent<Renderer>().material = camMat;

        // 3. GÖRSEL ŞÖLEN: Manyetik Ortam Tozları (Ambient Particles)
        GameObject tozObjesi = new GameObject("Manyetik_Toz_Efekti");
        tozObjesi.transform.SetParent(yeniArena.transform);
        tozObjesi.transform.localPosition = new Vector3(0, 0.5f, 0); // Camın hemen üstünde süzülsünler

        ParticleSystem ps = tozObjesi.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = tozObjesi.GetComponent<ParticleSystemRenderer>();
        
        // Şık altın rengi materyali bul veya oluştur
        Material tozMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (tozMat.shader != null)
        {
            tozMat.SetInt("_Surface", 1); // Transparent
            tozMat.SetColor("_BaseColor", new Color(1f, 0.8f, 0.2f, 0.8f));
            psr.material = tozMat;
        }

        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f); // Çok yavaş kaybolsunlar
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.15f); // Çok yavaş hareket etsinler
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f); // Minik zerreler
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.4f, 1f), new Color(1f, 0.6f, 0.1f, 0.5f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 150; // Ortamda kalabalık ama zarif bir toz bulutu

        var emission = ps.emission;
        emission.rateOverTime = 20f; // Saniyede 20 toz zerresi

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle; // Tüm arenaya yayılsın
        shape.radius = 13.5f; // Arenanın çapına uygun
        shape.radiusThickness = 1f; // Tüm dairenin içi dolsun
        shape.rotation = new Vector3(-90, 0, 0); // Yere paralel (Zeminden havaya doğru)

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.1f, 0.3f); // Yerden hafifçe yukarı yükselme
        velocity.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f); // Rüzgarda salınma
        velocity.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f); // Rüzgarda salınma

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) } // Yumuşakça belirip kaybolma (Fade in/out)
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.15f; // Manyetik alan dalgalanması hissi
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.2f;

        ps.Play();

        // 4. OYUNCU İÇİ RAHATLATICI "CEZA BÖLGELERİ" (Penalty Trays)
        // Oyuncunun ceza taşlarının nereye gideceğini net olarak görebilmesi için görsel zeminler
        void CezaZeminiCiz(Vector3 pos, Vector3 scale, string isim)
        {
            GameObject z = GameObject.CreatePrimitive(PrimitiveType.Cube);
            z.name = "Ceza_Bolgesi_" + isim;
            z.transform.SetParent(yeniArena.transform);
            z.transform.localPosition = pos;
            z.transform.localScale = scale;
            Destroy(z.GetComponent<Collider>());
            z.GetComponent<Renderer>().material = camMat; // Füme cam
        }
        CezaZeminiCiz(new Vector3(11f, 0.01f, 0f), new Vector3(1.5f, 0.02f, 12f), "P1"); // P1 Arkası
        CezaZeminiCiz(new Vector3(-11f, 0.01f, 0f), new Vector3(1.5f, 0.02f, 12f), "P2"); // P2 Arkası
        CezaZeminiCiz(new Vector3(0f, 0.01f, 11f), new Vector3(12f, 0.02f, 1.5f), "P3"); // P3 Arkası
        CezaZeminiCiz(new Vector3(0f, 0.01f, -11f), new Vector3(12f, 0.02f, 1.5f), "P4"); // P4 Arkası

        // --- TEMİZLİK: Ortada unutulan mor noktayı (hatalı/test objesini) yokediyoruz ---
        foreach (MeshRenderer mr in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            // Eğer obje yeni arenanın bir parçasıysa dokunma
            if (yeniArena != null && mr.transform.IsChildOf(yeniArena.transform)) continue;
            // Masa veya Taş ise dokunma
            if (mr.gameObject.name.Contains("Masa") || mr.gameObject.name.Contains("MaketTas") || mr.gameObject.name.Contains("Arena")) continue;

            // Eğer obje tam merkeze yakınsa (X=0, Z=0) ve ufak tefek bir şeyse onu kapat!
            Vector3 pos = mr.transform.position;
            pos.y = 0; // Sadece X ve Z eksenindeki merkeze uzaklığa bakalım
            if (pos.magnitude < 1.0f && mr.bounds.size.magnitude < 3f)
            {
                mr.gameObject.SetActive(false);
            }
        }
    }

    void TaslariDizVeGrupla()
    {
        ArenaOlustur();

        p1MaketTaslar.Clear();
        p2MaketTaslar.Clear();
        p3MaketTaslar.Clear();
        p4MaketTaslar.Clear();

        List<GameObject> tumTaslar = new List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name.StartsWith("MaketTas") && go.scene.isLoaded)
            {
                tumTaslar.Add(go);
            }
        }

        // Y yüksekliğini uçak gibi havada kalmamaları için 3'ten 1.15'e indirdik. Tam masaya oturacaklar!
        for (int i = 0; i < tumTaslar.Count; i++)
        {
            tumTaslar[i].transform.SetParent(null);
            tumTaslar[i].transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // Birbirlerine girmemeleri için 0.6'ya küçülttük
            
            // Eğer parlak materyali oluşturabildiysek, taşa ata ki 3 boyutlu ve şık görünsün
            if (altinMat != null)
            {
                Renderer rnd = tumTaslar[i].GetComponentInChildren<Renderer>();
                if (rnd != null) rnd.material = altinMat;
            }
            
            // Taşların sınırları genişletildi (±8.5f), böylece dev arena için yer açıldı
            if (i < 10) 
            {
                p1MaketTaslar.Add(tumTaslar[i]);
                float zPos = -3.6f + (i * 0.8f); 
                tumTaslar[i].transform.position = new Vector3(8.5f, 1.15f, zPos); 
            }
            else if (i < 20)
            {
                p2MaketTaslar.Add(tumTaslar[i]);
                float zPos = -3.6f + ((i - 10) * 0.8f);
                tumTaslar[i].transform.position = new Vector3(-8.5f, 1.15f, zPos); 
            }
            else if (i < 30)
            {
                p3MaketTaslar.Add(tumTaslar[i]);
                float xPos = -3.6f + ((i - 20) * 0.8f);
                tumTaslar[i].transform.position = new Vector3(xPos, 1.15f, 8.5f); 
            }
            else
            {
                p4MaketTaslar.Add(tumTaslar[i]);
                float xPos = -3.6f + ((i - 30) * 0.8f);
                tumTaslar[i].transform.position = new Vector3(xPos, 1.15f, -8.5f); 
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= IstemciKoptu;
        }
    }

    void IstemciKoptu(ulong clientId)
    {
        if (NetworkManager.Singleton != null && (clientId == NetworkManager.Singleton.LocalClientId || clientId == 0))
        {
            NetworkManager.Singleton.Shutdown();
            
            if (IsServer)
            {
                oyunBasladiNet.Value = false;
                oyunDurduNet.Value = false;
                p1Hazir.Value = false; p2Hazir.Value = false; p3Hazir.Value = false; p4Hazir.Value = false;
            }

            if (girisPaneli) girisPaneli.SetActive(false);
            if (oyunArayuzuPanel) oyunArayuzuPanel.SetActive(false);
            if (lobiPaneli) lobiPaneli.SetActive(true);

            GameObject ust = GameObject.Find("UstPanel");
            if (ust) ust.SetActive(false);
            if (devasaSiraYazisi) devasaSiraYazisi.gameObject.SetActive(false);
            if (arkaPlanImaj != null) arkaPlanImaj.gameObject.SetActive(true);
            
            oyunBitti = false;
        }
    }

    public void OdadanCik()
    {
        if (NetworkManager.Singleton != null)
        {
            ulong id = NetworkManager.Singleton.LocalClientId;
            IstemciKoptu(id);
        }
    }

    public override void OnNetworkSpawn()
    {
        NetworkVariable<int>[] intVars = { p1Tas, p2Tas, p3Tas, p4Tas, p1FizikselCezaSayisi, p2FizikselCezaSayisi, p3FizikselCezaSayisi, p4FizikselCezaSayisi, siraKimde };
        foreach (var v in intVars) v.OnValueChanged += (o, n) => GuncelleUI();

        NetworkVariable<bool>[] boolVars = { p1Hazir, p2Hazir, p3Hazir, p4Hazir };
        foreach (var v in boolVars) v.OnValueChanged += (o, n) => GuncelleUI();

        // İSİM GÜNCELLEME HATASI DÜZELTMESİ: İsimler değiştiğinde de UI güncellenmeli
        p1IsimNet.OnValueChanged += (o, n) => GuncelleUI();
        p2IsimNet.OnValueChanged += (o, n) => GuncelleUI();
        p3IsimNet.OnValueChanged += (o, n) => GuncelleUI();
        p4IsimNet.OnValueChanged += (o, n) => GuncelleUI();

        oyunDurduNet.OnValueChanged += (o, n) => 
        {
            Time.timeScale = n ? 0f : 1f;
            if (duraklatYazisi != null) duraklatYazisi.text = n ? "►" : "||";
            else if (butonImage != null) butonImage.sprite = n ? oynatSprite : duraklatSprite;
            
            if (n && devasaSiraYazisi) 
            {
                devasaSiraYazisi.gameObject.SetActive(true);
                string isim = "BİR OYUNCU";
                if (durduranKisi.Value == 0) isim = p1IsimNet.Value.ToString();
                else if (durduranKisi.Value == 1) isim = p2IsimNet.Value.ToString();
                else if (durduranKisi.Value == 2) isim = p3IsimNet.Value.ToString();
                else if (durduranKisi.Value == 3) isim = p4IsimNet.Value.ToString();
                
                devasaSiraYazisi.text = $"{isim} OYUNU DURAKLATTI";
                devasaSiraYazisi.color = Color.white;
            }
            else if (!n) GuncelleUI();
        };
        tasUcusdaNet.OnValueChanged += (o, n) => GuncelleUI();
        GuncelleUI();
    }

    public void HostOl()
    {
        var utp = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        if (utp != null) utp.ConnectionData.Port = 8888;
        NetworkManager.Singleton.StartHost();
        LobiKapat();
    }

    public void Katil()
    {
        var utp = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        if (utp != null) utp.ConnectionData.Port = 8888;
        NetworkManager.Singleton.StartClient();
        LobiKapat();
    }

    void LobiKapat() { if (lobiPaneli) lobiPaneli.SetActive(false); if (girisPaneli) girisPaneli.SetActive(true); }

    string GetLocalInputName()
    {
        // Hangi kutu doluysa oradaki ismi alıyoruz, böylece karışıklık önleniyor
        if (p1IsimInput != null && !string.IsNullOrEmpty(p1IsimInput.text)) return p1IsimInput.text;
        if (p2IsimInput != null && !string.IsNullOrEmpty(p2IsimInput.text)) return p2IsimInput.text;
        return null;
    }

    public void OyunuBaslat()
    {
        bool sunucuYok = NetworkManager.Singleton != null && (!NetworkManager.Singleton.IsListening || (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsConnectedClient));

        if (sunucuYok)
        {
            NetworkManager.Singleton.Shutdown();
            HostOl();
            return;
        }

        PanelAyarlariniYap();

        if (IsServer)
        {
            string isim = GetLocalInputName();
            if (!string.IsNullOrEmpty(isim)) p1IsimNet.Value = isim;
            p1Hazir.Value = true;
        }
        else
        {
            string isim = GetLocalInputName();
            IsimVeHazirBelirleServerRpc(isim ?? "MISAFIR");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void IsimVeHazirBelirleServerRpc(string isim, ServerRpcParams rpcParams = default)
    {
        ulong id = rpcParams.Receive.SenderClientId;
        // Client ID 1 olan kişi Player 2'dir
        if (id == 1) { p2IsimNet.Value = isim; p2Hazir.Value = true; }
        else if (id == 2) { p3IsimNet.Value = isim; p3Hazir.Value = true; }
        else if (id == 3) { p4IsimNet.Value = isim; p4Hazir.Value = true; }
    }

    public void PanelAyarlariniYap()
    {
        if (lobiPaneli) lobiPaneli.SetActive(false);
        if (girisPaneli) girisPaneli.SetActive(false);
        if (oyunArayuzuPanel) oyunArayuzuPanel.SetActive(true);

        GameObject ust = GameObject.Find("UstPanel");
        if (ust) ust.SetActive(true);
        if (devasaSiraYazisi) devasaSiraYazisi.gameObject.SetActive(true);

        if (duraklatYazisi != null) duraklatYazisi.text = "||";
        else if (butonImage != null && duraklatSprite != null) butonImage.sprite = duraklatSprite;
        
        if (arkaPlanImaj != null) arkaPlanImaj.gameObject.SetActive(false);

        // Kullanılmayan masaları ve UI'ları gizle
        if (oyunBasladiNet.Value)
        {
            if (oyuncuSayisi.Value < 3)
            {
                if (p3IsimText != null && p3IsimText.transform.parent != null) p3IsimText.transform.parent.gameObject.SetActive(false);
                GameObject m3 = GameObject.Find("YesilMasa_P3");
                if (m3 != null) m3.SetActive(false);
                GameObject cb3 = GameObject.Find("Ceza_Bolgesi_P3");
                if (cb3 != null) cb3.SetActive(false);
                foreach(var t in p3MaketTaslar) if(t) t.SetActive(false);
            }
            if (oyuncuSayisi.Value < 4)
            {
                if (p4IsimText != null && p4IsimText.transform.parent != null) p4IsimText.transform.parent.gameObject.SetActive(false);
                GameObject m4 = GameObject.Find("SariMasa_P4");
                if (m4 != null) m4.SetActive(false);
                GameObject cb4 = GameObject.Find("Ceza_Bolgesi_P4");
                if (cb4 != null) cb4.SetActive(false);
                foreach(var t in p4MaketTaslar) if(t) t.SetActive(false);
            }
        }

        GuncelleUI();
    }

    void Update()
    {
        if (!oyunBasladiNet.Value)
        {
            if (IsServer)
            {
                int c = NetworkManager.Singleton.ConnectedClients.Count;
                if (c >= 2 && p1Hazir.Value && p2Hazir.Value && (c < 3 || p3Hazir.Value) && (c < 4 || p4Hazir.Value))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        oyuncuSayisi.Value = c;
                        oyunBasladiNet.Value = true;
                        tasUcusdaNet.Value = false; // Başlangıçta kilidi temizle
                    }
                }
            }

            if (saatYazisi)
            {
                if (IsServer && NetworkManager.Singleton.ConnectedClients.Count >= 2)
                    saatYazisi.text = "OYUNCULAR HAZIR! BAŞLATMAK İÇİN EKRANA TIKLA.";
                else
                    saatYazisi.text = "OYUNCULAR BEKLENİYOR...";
            }
            return;
        }

        if (oyunDurduNet.Value || oyunBitti) return;

        if (IsServer)
        {
            if (siraKimde.Value == 1) p1Sure.Value = Mathf.Max(0, p1Sure.Value - Time.deltaTime);
            else if (siraKimde.Value == 2) p2Sure.Value = Mathf.Max(0, p2Sure.Value - Time.deltaTime);
            else if (siraKimde.Value == 3) p3Sure.Value = Mathf.Max(0, p3Sure.Value - Time.deltaTime);
            else if (siraKimde.Value == 4) p4Sure.Value = Mathf.Max(0, p4Sure.Value - Time.deltaTime);
        }

        if (p1Sure.Value <= 0 && !oyunBitti) OyunuBitir(EnAzTasiOlanID(), "SÜRE BİTTİ!");
        if (p2Sure.Value <= 0 && !oyunBitti) OyunuBitir(EnAzTasiOlanID(), "SÜRE BİTTİ!");
        if (oyuncuSayisi.Value >= 3 && p3Sure.Value <= 0 && !oyunBitti) OyunuBitir(EnAzTasiOlanID(), "SÜRE BİTTİ!");
        if (oyuncuSayisi.Value >= 4 && p4Sure.Value <= 0 && !oyunBitti) OyunuBitir(EnAzTasiOlanID(), "SÜRE BİTTİ!");

        if (saatYazisi) saatYazisi.text = ""; // Ortadaki yazıyı gizle

        IsimleriVeSureleriGuncelle();

        if (Input.GetMouseButtonDown(0) && !tasUcusdaNet.Value && !ButonaTiklandiMi())
        {
            int localPlayerID = IsServer ? 1 : (int)NetworkManager.Singleton.LocalClientId + 1;
            if (siraKimde.Value == localPlayerID)
            {
                TiklaServerRpc(GetMousePos());
            }
        }
    }

    void IsimleriVeSureleriGuncelle()
    {
        if (p1IsimText) p1IsimText.text = $"{p1IsimNet.Value.ToString().ToUpper()} <size=80%><color=#FFD700>[{p1Sure.Value:F0}s]</color></size>";
        if (p2IsimText) p2IsimText.text = $"{p2IsimNet.Value.ToString().ToUpper()} <size=80%><color=#FFD700>[{p2Sure.Value:F0}s]</color></size>";
        if (p3IsimText) p3IsimText.text = $"{p3IsimNet.Value.ToString().ToUpper()} <size=80%><color=#FFD700>[{p3Sure.Value:F0}s]</color></size>";
        if (p4IsimText) p4IsimText.text = $"{p4IsimNet.Value.ToString().ToUpper()} <size=80%><color=#FFD700>[{p4Sure.Value:F0}s]</color></size>";
    }

    int EnAzTasiOlanID()
    {
        int minTas = p1Tas.Value; int kazanan = 1;
        if (oyuncuSayisi.Value >= 2 && p2Tas.Value < minTas) { minTas = p2Tas.Value; kazanan = 2; }
        if (oyuncuSayisi.Value >= 3 && p3Tas.Value < minTas) { minTas = p3Tas.Value; kazanan = 3; }
        if (oyuncuSayisi.Value >= 4 && p4Tas.Value < minTas) { minTas = p4Tas.Value; kazanan = 4; }
        return kazanan;
    }

    Vector3 GetMousePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        new Plane(Vector3.up, new Vector3(0, 0.5f, 0)).Raycast(ray, out float d);
        return ray.GetPoint(d);
    }

    void SonrakiSira()
    {
        int limit = oyuncuSayisi.Value;
        for (int i = 0; i < limit; i++)
        {
            siraKimde.Value++;
            if (siraKimde.Value > limit) siraKimde.Value = 1;

            if (siraKimde.Value == 1 && p1Tas.Value > 0) return;
            if (siraKimde.Value == 2 && p2Tas.Value > 0) return;
            if (siraKimde.Value == 3 && p3Tas.Value > 0) return;
            if (siraKimde.Value == 4 && p4Tas.Value > 0) return;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void TiklaServerRpc(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        // Sunucu tarafında uçuş kilidi kontrolü (Race Condition engelleme)
        if (tasUcusdaNet.Value) return;

        int expectedID = (int)rpcParams.Receive.SenderClientId + 1;
        if (expectedID != siraKimde.Value) return;

        tasUcusdaNet.Value = true; // Atış başladı, kilitle
        sonOynayanID.Value = siraKimde.Value;

        MagnetController[] tumTaslar = Object.FindObjectsByType<MagnetController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var t in tumTaslar)
        {
            if (t.cezaKesildi)
            {
                // Multiplayer Ağ Güncellemesi: Taşlar X=8.5 ve Z=8.5'e itildiği için ceza sınırları güvenli alan olarak ±10.5'e çıkartıldı.
                if (siraKimde.Value == 1 && t.transform.position.x > 10.5f) { RemoveCeza(t, 1); break; }
                if (siraKimde.Value == 2 && t.transform.position.x < -10.5f) { RemoveCeza(t, 2); break; }
                if (siraKimde.Value == 3 && t.transform.position.z > 10.5f) { RemoveCeza(t, 3); break; }
                if (siraKimde.Value == 4 && t.transform.position.z < -10.5f) { RemoveCeza(t, 4); break; }
            }
        }

        GameObject t_obj = Instantiate(gercekMiknatisPrefab, new Vector3(pos.x, 5f, pos.z), Quaternion.identity);
        MagnetController mc = t_obj.GetComponent<MagnetController>();
        if (mc != null) 
        {
            mc.cezaKesildi = false;
            mc.atanID.Value = expectedID; // Taşa sahibini kazıdık!
        }
        
        t_obj.GetComponent<NetworkObject>().Spawn();
        TiklaClientRpc(t_obj.GetComponent<NetworkObject>().NetworkObjectId, pos, siraKimde.Value);

        if (siraKimde.Value == 1) p1Tas.Value--;
        else if (siraKimde.Value == 2) p2Tas.Value--;
        else if (siraKimde.Value == 3) p3Tas.Value--;
        else if (siraKimde.Value == 4) p4Tas.Value--;

        SonrakiSira();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TasUcusuBittiServerRpc()
    {
        tasUcusdaNet.Value = false; // Kilidi kaldır
    }

    void RemoveCeza(MagnetController t, int p)
    {
        t.GetComponent<NetworkObject>().Despawn();
        if (p == 1) p1FizikselCezaSayisi.Value = Mathf.Max(0, p1FizikselCezaSayisi.Value - 1);
        else if (p == 2) p2FizikselCezaSayisi.Value = Mathf.Max(0, p2FizikselCezaSayisi.Value - 1);
        else if (p == 3) p3FizikselCezaSayisi.Value = Mathf.Max(0, p3FizikselCezaSayisi.Value - 1);
        else if (p == 4) p4FizikselCezaSayisi.Value = Mathf.Max(0, p4FizikselCezaSayisi.Value - 1);
    }

    [ClientRpc]
    void TiklaClientRpc(ulong id, Vector3 h, int pID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var obj))
            StartCoroutine(Ucur(obj.gameObject, h, pID));
    }

    IEnumerator Ucur(GameObject t, Vector3 h, int pID)
    {
        float et = 0; Vector3 b = t.transform.position;
        while (et < 1f)
        {
            et += Time.unscaledDeltaTime * 4f;
            t.transform.position = Vector3.Lerp(b, h, et) + Vector3.up * Mathf.Sin(et * Mathf.PI) * 2f;
            yield return null;
        }
        t.transform.position = h;
        if (t.GetComponent<MagnetController>())
        {
            t.GetComponent<MagnetController>().oyundaMi = true;
            t.GetComponent<MagnetController>().ZeminRenginiAyarla(pID == 1 || pID == 3);
            
            // RENK HATASI DÜZELTMESİ: Yeni fırlatılan taşa altın materyalini ata
            Renderer rnd = t.GetComponentInChildren<Renderer>();
            if (rnd != null && altinMat != null) rnd.material = altinMat;
        }
        
        // Atan kişi bizsek, sunucuya uçuşun bittiğini söyleyelim ki kilit kalksın
        if (IsOwner || IsServer) TasUcusuBittiServerRpc();
    }

    public void TaslarCarpisti(ulong id1, ulong id2, int atanID)
    {
        if (!IsServer) return;

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id1, out var obj1);
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id2, out var obj2);

        if (obj1 != null && obj2 != null)
        {
            // Yarış durumu koruması: Eğer taştan gelen geçerli bir atanID varsa onu kullan, yoksa son oynayana bak.
            int cezaAlanID = (atanID > 0) ? atanID : sonOynayanID.Value;
            List<GameObject> yapisanlar = new List<GameObject> { obj1.gameObject, obj2.gameObject };

            MagnetController[] tumTaslar = Object.FindObjectsByType<MagnetController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            bool yeniBulundu = true;
            while (yeniBulundu)
            {
                yeniBulundu = false;
                foreach (var t in tumTaslar)
                {
                    if (yapisanlar.Contains(t.gameObject) || t.cezaKesildi) continue;
                    foreach (var y in yapisanlar)
                    {
                        if (Vector3.Distance(t.transform.position, y.transform.position) < 2.8f)
                        {
                            yapisanlar.Add(t.gameObject);
                            yeniBulundu = true;
                            break;
                        }
                    }
                }
            }

            CarpismaEfektiOynatClientRpc(obj1.transform.position);
            KivilcimOlusturClientRpc(obj1.transform.position);

            foreach (var tas in yapisanlar)
            {
                int cIndex = 0;
                if (cezaAlanID == 1) cIndex = p1FizikselCezaSayisi.Value;
                else if (cezaAlanID == 2) cIndex = p2FizikselCezaSayisi.Value;
                else if (cezaAlanID == 3) cIndex = p3FizikselCezaSayisi.Value;
                else if (cezaAlanID == 4) cIndex = p4FizikselCezaSayisi.Value;

                MagnetController mc = tas.GetComponent<MagnetController>();
                if (mc != null) { mc.oyundaMi = false; mc.cezaKesildi = true; }

                Vector3 hedef = CalculateCezaHedef(cezaAlanID, cIndex);

                TaslariEveGonderClientRpc(tas.GetComponent<NetworkObject>().NetworkObjectId, hedef);

                if (cezaAlanID == 1) { p1FizikselCezaSayisi.Value++; p1Tas.Value++; }
                else if (cezaAlanID == 2) { p2FizikselCezaSayisi.Value++; p2Tas.Value++; }
                else if (cezaAlanID == 3) { p3FizikselCezaSayisi.Value++; p3Tas.Value++; }
                else if (cezaAlanID == 4) { p4FizikselCezaSayisi.Value++; p4Tas.Value++; }
            }
        }
    }

    Vector3 CalculateCezaHedef(int playerID, int cezaIndex)
    {
        float basX, basZ;
        float off1 = (cezaIndex % 8) * 1.5f - 5f;
        
        // KRİTİK HATA DÜZELTMESİ: 
        // Eskiden off2 (Y ekseni) 0.5f'ti. Masamızın yüksekliği ise 1.0f.
        // Taşlar masanın içine gömülüyordu! Şimdi normal taşlarla aynı hizaya (1.15f) çektik.
        float off2 = 1.15f + (cezaIndex / 8) * 0.8f; 

        if (playerID == 1) {
            basX = (p1MaketTaslar.Count > 0 ? p1MaketTaslar[0].transform.position.x : 8.5f) + 2.5f;
            return new Vector3(basX, off2, off1);
        }
        else if (playerID == 2) {
            basX = (p2MaketTaslar.Count > 0 ? p2MaketTaslar[0].transform.position.x : -8.5f) - 2.5f;
            return new Vector3(basX, off2, off1);
        }
        else if (playerID == 3) {
            basZ = (p3MaketTaslar.Count > 0 ? p3MaketTaslar[0].transform.position.z : 8.5f) + 2.5f;
            return new Vector3(off1, off2, basZ);
        }
        else {
            basZ = (p4MaketTaslar.Count > 0 ? p4MaketTaslar[0].transform.position.z : -8.5f) - 2.5f;
            return new Vector3(off1, off2, basZ);
        }
    }

    [ClientRpc]
    void TaslariEveGonderClientRpc(ulong id, Vector3 hedef)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var obj))
            StartCoroutine(IstemciUcurmaKrutin(obj.gameObject, hedef));
    }

    IEnumerator IstemciUcurmaKrutin(GameObject tas, Vector3 hedef)
    {
        if (tas == null) yield break;
        Rigidbody rb = tas.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        Collider col = tas.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Vector3 baslangic = tas.transform.position;
        float et = 0;
        while (et < 1f)
        {
            if (tas == null) yield break;
            et += Time.deltaTime * 3f;
            tas.transform.position = Vector3.Lerp(baslangic, hedef, et) + Vector3.up * Mathf.Sin(et * Mathf.PI) * 5f;
            yield return null;
        }
        if (tas != null) 
        { 
            tas.transform.position = hedef; 
            if (col != null) col.enabled = true; 
            
            // RENK HATASI DÜZELTMESİ: Ceza alan taşın griye dönmesini engelle ve altın yap
            Renderer rnd = tas.GetComponentInChildren<Renderer>();
            if (rnd != null && altinMat != null) rnd.material = altinMat;
        }
    }

    [ClientRpc]
    void CarpismaEfektiOynatClientRpc(Vector3 pozisyon)
    {
        if (carpismaSesi != null) AudioSource.PlayClipAtPoint(carpismaSesi, pozisyon);
        if (carpismaEfektiPrefab != null) 
        {
            GameObject efekt = Instantiate(carpismaEfektiPrefab, pozisyon, Quaternion.identity);
            ParticleSystem ps = efekt.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(efekt, 2f);
        }
    }

    Color GetPlayerColor(int p)
    {
        if (p == 1) return Color.red;
        if (p == 2) return Color.blue;
        if (p == 3) return Color.green;
        if (p == 4) return Color.yellow;
        return Color.white;
    }

    public void OyunuBitir(int kazananID, string sebep)
    {
        if (oyunBitti) return;
        oyunBitti = true;
        if (devasaSiraYazisi)
        {
            string kIsim = kazananID == 1 ? p1IsimNet.Value.ToString() : kazananID == 2 ? p2IsimNet.Value.ToString() : kazananID == 3 ? p3IsimNet.Value.ToString() : p4IsimNet.Value.ToString();
            devasaSiraYazisi.text = $"{sebep}\n{kIsim.ToUpper()} KAZANDI!";
            devasaSiraYazisi.color = GetPlayerColor(kazananID);
        }
        if (konfetiPrefab != null) 
        {
            GameObject efekt = Instantiate(konfetiPrefab, new Vector3(0, 15f, 0), Quaternion.identity);
            ParticleSystem ps = efekt.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }
    }

    void GuncelleUI()
    {
        IsimleriVeSureleriGuncelle();
        
        if (p1TasText) p1TasText.text = "TAŞ: " + p1Tas.Value;
        if (p2TasText) p2TasText.text = "TAŞ: " + p2Tas.Value;
        if (p3TasText) p3TasText.text = "TAŞ: " + p3Tas.Value;
        if (p4TasText) p4TasText.text = "TAŞ: " + p4Tas.Value;

        if (p1Tas.Value <= 0) OyunuBitir(1, "TAŞLAR BİTTİ!");
        else if (oyuncuSayisi.Value >= 2 && p2Tas.Value <= 0) OyunuBitir(2, "TAŞLAR BİTTİ!");
        else if (oyuncuSayisi.Value >= 3 && p3Tas.Value <= 0) OyunuBitir(3, "TAŞLAR BİTTİ!");
        else if (oyuncuSayisi.Value >= 4 && p4Tas.Value <= 0) OyunuBitir(4, "TAŞLAR BİTTİ!");
        
        else if (!oyunBasladiNet.Value)
        {
            if (devasaSiraYazisi)
            {
                devasaSiraYazisi.gameObject.SetActive(true);
                int c = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClients.Count : 0;
                if (c < 2) { devasaSiraYazisi.text = "RAKİPLER BEKLENİYOR..."; devasaSiraYazisi.color = Color.white; }
                else { devasaSiraYazisi.text = "OYUNCULAR HAZIR\nBAŞLAMAK İÇİN EKRANA TIKLA (Sadece Host)"; devasaSiraYazisi.color = Color.yellow; }
            }
        }
        else if (!oyunBitti && devasaSiraYazisi && !oyunDurduNet.Value)
        {
            // Renk isimleri yerine oyuncu isimlerini kullanarak profesyonel bir görünüm sağlıyoruz
            string sIsim = siraKimde.Value == 1 ? p1IsimNet.Value.ToString() : siraKimde.Value == 2 ? p2IsimNet.Value.ToString() : siraKimde.Value == 3 ? p3IsimNet.Value.ToString() : p4IsimNet.Value.ToString();
            devasaSiraYazisi.text = $"SIRA {sIsim.ToUpper()}'DE";
            devasaSiraYazisi.color = GetPlayerColor(siraKimde.Value);
        }

        int p1GerekenMaket = p1Tas.Value - p1FizikselCezaSayisi.Value;
        int p2GerekenMaket = p2Tas.Value - p2FizikselCezaSayisi.Value;
        int p3GerekenMaket = p3Tas.Value - p3FizikselCezaSayisi.Value;
        int p4GerekenMaket = p4Tas.Value - p4FizikselCezaSayisi.Value;

        for (int i = 0; i < p1MaketTaslar.Count; i++) if (p1MaketTaslar[i]) p1MaketTaslar[i].SetActive(i < p1GerekenMaket);
        for (int i = 0; i < p2MaketTaslar.Count; i++) if (p2MaketTaslar[i]) p2MaketTaslar[i].SetActive(i < p2GerekenMaket);
        
        if (oyuncuSayisi.Value >= 3 || !oyunBasladiNet.Value)
        {
            for (int i = 0; i < p3MaketTaslar.Count; i++) if (p3MaketTaslar[i]) p3MaketTaslar[i].SetActive(i < p3GerekenMaket);
        }
        else
        {
            for (int i = 0; i < p3MaketTaslar.Count; i++) if (p3MaketTaslar[i]) p3MaketTaslar[i].SetActive(false);
        }
            
        if (oyuncuSayisi.Value >= 4 || !oyunBasladiNet.Value)
        {
            for (int i = 0; i < p4MaketTaslar.Count; i++) if (p4MaketTaslar[i]) p4MaketTaslar[i].SetActive(i < p4GerekenMaket);
        }
        else
        {
            for (int i = 0; i < p4MaketTaslar.Count; i++) if (p4MaketTaslar[i]) p4MaketTaslar[i].SetActive(false);
        }
    }

    [ClientRpc]
    void KivilcimOlusturClientRpc(Vector3 pos)
    {
        GameObject kivilcim = new GameObject("Kivilcim");
        kivilcim.transform.position = pos;
        ParticleSystem ps = kivilcim.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 0.5f;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.7f, 0.1f); // Parlak turuncu
        main.gravityModifier = 1f;
        
        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[]{ new ParticleSystem.Burst(0f, 40) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        ParticleSystemRenderer renderer = kivilcim.GetComponent<ParticleSystemRenderer>();
        // Standart URP Particle materyali
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

        Destroy(kivilcim, 1.5f);
    }

    bool ButonaTiklandiMi()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = Input.mousePosition };
        var results = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);
        foreach (var r in results) if (butonImage != null && r.gameObject == butonImage.gameObject) return true;
        return false;
    }

    public void DurdurVeyaOynat()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            DurdurVeyaOynatServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DurdurVeyaOynatServerRpc(ulong kimBasti)
    {
        if (!oyunDurduNet.Value)
        {
            oyunDurduNet.Value = true;
            durduranKisi.Value = kimBasti;
        }
        else
        {
            if (durduranKisi.Value == kimBasti)
            {
                oyunDurduNet.Value = false;
            }
        }
    }
}