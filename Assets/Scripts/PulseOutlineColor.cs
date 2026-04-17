using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PulseOutlineColor : MonoBehaviour
{
    [Header("Efekt Nastavenia")]
    public float speed = 1f;
    public bool useRainbowRGB = false;

    [Header("Dve farby (ak je Rainbow vypnutý)")]
    public Color color1 = Color.red;
    public Color color2 = Color.blue;

    private TextMeshProUGUI textMesh;
    private Material fontMaterialInstance;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        
        // Vytvoríme si vlastnú kópiu materiálu len pre tento nápis,
        // aby sme omylom nemenili farbu VŠETKÝM textom v hre.
        fontMaterialInstance = new Material(textMesh.fontSharedMaterial);
        textMesh.fontSharedMaterial = fontMaterialInstance;
    }

    void Update()
    {
        if (fontMaterialInstance == null) return;

        Color targetColor;

        if (useRainbowRGB)
        {
            // Plynulá "RGB RGB" Dúha
            float hue = Mathf.Repeat(Time.time * speed, 1f);
            targetColor = Color.HSVToRGB(hue, 1f, 1f);
        }
        else
        {
            // Plynulé "Ping pong" prelievanie z modrej do červenej a späť
            float t = Mathf.PingPong(Time.time * speed, 1f);
            targetColor = Color.Lerp(color1, color2, t);
        }

        // Nastaví vypočítanú farbu priamo do Outline vlastnosti materiálu
        fontMaterialInstance.SetColor("_OutlineColor", targetColor);
    }
}
