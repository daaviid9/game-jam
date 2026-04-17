using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover efekt (Vyskočenie)")]
    [Tooltip("1.1 znamená o 10% väčšie pri nabehnutí myšou")]
    public float hoverScaleSize = 1.1f;
    [Tooltip("Ako rýchlo tlačidlo zmení veľkosť")]
    public float scaleSpeed = 15f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        // Uložíme pôvodnú veľkosť tlačidla (väčšinou to je 1x1x1)
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Plynulo pendlujeme medzi základnou veľkosťou a zväčšenou
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    // Táto funkcia sa spustí SAMA hneď ako kurzor vojde nad tlačidlo
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cieľom bude zväčšenie
        targetScale = originalScale * hoverScaleSize;
    }

    // Keď myš vyjde von
    public void OnPointerExit(PointerEventData eventData)
    {
        // Cieľom bude pôvodný stav
        targetScale = originalScale;
    }
}
