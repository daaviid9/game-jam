using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class VHSOverlay : MonoBehaviour
{
    [Header("VHS Nastavenia (Scanlines)")]
    [Tooltip("Priehľadnosť čiar (1 = úplná tma, napr. 0.1 je ideál pre jemný efekt)")]
    [Range(0f, 1f)]
    public float opacity = 0.15f;
    
    [Tooltip("Hustota čiar na obrazovke")]
    public float lineDensity = 250f; 

    [Tooltip("Rýchlosť padania čiar (záporné číslo ide dole)")]
    public float scrollSpeed = -0.5f;

    private RawImage rawImage;
    private Texture2D scanlineTexture;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.color = new Color(1, 1, 1, opacity);

        // Vytvoríme si automaticky procedurálnu textúru (nemusíš sťahovať žiadny obrázok!)
        // Máme len 2 pixely: jeden čierny a druhý priesvitný
        scanlineTexture = new Texture2D(1, 2);
        scanlineTexture.SetPixel(0, 0, new Color(0, 0, 0, 0)); // Spodný = priehľadný
        scanlineTexture.SetPixel(0, 1, new Color(0, 0, 0, 1)); // Vrchný = čierny
        scanlineTexture.Apply();

        scanlineTexture.wrapMode = TextureWrapMode.Repeat;
        scanlineTexture.filterMode = FilterMode.Point; // Zabezpečí absolútnu ostrosť pixelov

        // Priradíme textúru
        rawImage.texture = scanlineTexture;
        
        // Zrušíme Raycasty, aby VHS filter neblokoval klikanie na tlačidlá, ktoré môžu byť "za" ním
        rawImage.raycastTarget = false; 
    }

    void Update()
    {
        // Vypočítame rolovaciu pozíciu (uv.y)
        float yPos = Time.time * scrollSpeed;
        
        // Vďaka parametrom zopakujeme túto 2-pixelovú textúru "lineDensity" krát po celej dĺžke obrazovky.
        rawImage.uvRect = new Rect(0, yPos, 1f, lineDensity);
    }
}
