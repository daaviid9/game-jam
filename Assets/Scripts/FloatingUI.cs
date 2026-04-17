using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    [Header("Nastavenia Dýchania")]
    public float amplitude = 10f; // O koľko pixelov to vyletí hore a dole
    public float speed = 2f;      // Ako rýchlo to pláva

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Zapamätáme si pôvodnú pozíciu
            startPos = rectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        if (rectTransform != null)
        {
            // Pomocou Sine vlny počítame jemný ladný pohyb
            float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
            
            // Nastavíme novú pozíciu
            rectTransform.anchoredPosition = new Vector2(startPos.x, newY);
        }
    }
}
