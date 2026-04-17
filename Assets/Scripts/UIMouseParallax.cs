using UnityEngine;
using UnityEngine.InputSystem;

public class UIMouseParallax : MonoBehaviour
{
    [Header("Nastavenia Parallax")]
    [Tooltip("Maximálny počet pixelov, o ktoré sa obrázok pohne.")]
    public float parallaxAmount = 20f; 
    
    [Tooltip("Ako rýchlo bude obrázok 'dobiehať' myš (mäkkosť pohybu).")]
    public float smoothSpeed = 5f;     

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Zapamätáme si základný bod
            startPos = rectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        if (rectTransform == null) return;

        // 1. Získať pozíciu myši (Nový Input System)
        Vector2 mousePos = Vector2.zero;
        if (Mouse.current != null)
        {
            mousePos = Mouse.current.position.ReadValue();
        }

        // 2. Normalizácia na rozsah od -1 (okraj) do 1 (opačný okraj obrazovky)
        float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
        float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;

        // Poistka, aby to "neušlo", ak kurzor opustí obrazovku 
        normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
        normalizedY = Mathf.Clamp(normalizedY, -1f, 1f);

        // 3. Vypočítať novú pozíciu. 
        // Všimni si mínus: ak sa myš pohne vpravo, obrázok ide vľavo, to robí tú ilúziu 3D.
        float targetX = startPos.x - (normalizedX * parallaxAmount);
        float targetY = startPos.y - (normalizedY * parallaxAmount);
        
        Vector2 targetPos = new Vector2(targetX, targetY);

        // 4. Plynulý (Lerpovaný) prechod k novej pozícii
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * smoothSpeed);
    }
}
