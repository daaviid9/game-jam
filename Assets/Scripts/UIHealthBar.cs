using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UIHealthBar : MonoBehaviour
{
    private Slider healthSlider;
    private PlayerController player;

    void Start()
    {
        healthSlider = GetComponent<Slider>();
        
        // Automaticky si to nájde hráča v scéne
        player = Object.FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            healthSlider.maxValue = player.maxHealth;
            healthSlider.value = player.maxHealth; // Hráč začína s plným zdravím
            
            // "Prilepíme" túto funkciu na Health Event hráča
            player.OnHealthChanged += UpdateHealthBar;
        }
        else
        {
            Debug.LogWarning("[HealthBar] Hráč na scéne chýba! Posuvník zdravia sa nenapojil.");
        }
    }

    // Táto funkcia sa zavolá ÚPLNE SAMA kedykoľvek hráč dostane Damage v PlayerController.cs
    void UpdateHealthBar(int newHealth)
    {
        healthSlider.value = newHealth;
    }
    
    void OnDestroy()
    {
        // Dobrá prax pre pamäť, odlepíme Event po zničení scény
        if (player != null)
            player.OnHealthChanged -= UpdateHealthBar;
    }
}
