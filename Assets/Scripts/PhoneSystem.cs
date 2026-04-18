using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PhoneSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject phoneOverlay; 
    public Slider fomoSlider;       

    [Header("Hand Reference")]
    public HandController handController;

    [Header("Post-Processing Settings")]
    public Volume postProcessVolume; // Assign Global Volume here in Inspector
    public float maxVignetteIntensity = 1f; // How dark it gets at 0% FOMO
    public float vignettePower = 2f; // Higher values make the vignette appear more suddenly at low FOMO
    
    [Header("Values")]
    public float fomoValue = 100f;
    public float drainRate = 8f;   
    public float refillRate = 20f;  
    
    [Header("Camera Effects")]
    public Camera mainCamera;
    public float normalFOV = 60f;
    public float phoneFOV = 40f; 
    public float fovTransitionSpeed = 5f;

    private InputSystem_Actions controls;
    private bool isScrolling = false;
    private Vignette vignette;

    void Awake()
    {
        controls = new InputSystem_Actions();

        // Register for phone scrolling (Jump action on Space)
        controls.Player.Jump.performed += ctx => isScrolling = true;
        controls.Player.Jump.canceled += ctx => isScrolling = false;

        // Try to find Volume if not assigned
        if (postProcessVolume == null) postProcessVolume = GetComponent<Volume>();

        // Get Vignette override from Volume profile
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            if (!postProcessVolume.profile.TryGet(out vignette))
            {
                Debug.LogError("Vignette not found in Volume profile!");
            }
        }
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        if (fomoSlider != null)
        {
            fomoSlider.maxValue = 100;
            fomoSlider.value = fomoValue;
        }
    }

    void Update()
    {
        // 1. Update Post-Processing Visuals (exponential curve for more drama)
        if (vignette != null)
        {
            // Normalize FOMO (0 to 1)
            float fomoNormalized = fomoValue / 100f;
            
            // Apply power curve (Intensity factor is 1 when FOMO is 0, and 0 when FOMO is 1)
            float intensityFactor = 1f - Mathf.Pow(fomoNormalized, vignettePower);
            
            float targetIntensity = intensityFactor * maxVignetteIntensity;
            vignette.intensity.Override(targetIntensity);
        }

        // 2. Game State check
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        // 3. Movement & FOMO Logic
        if (isScrolling)
        {
            // Scrolling on phone
            if (phoneOverlay != null) phoneOverlay.SetActive(true);
            if (handController != null) handController.SetRaised(true);
            fomoValue += refillRate * Time.deltaTime;
            
            // Camera zoom-in effect
            if (mainCamera != null)
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, phoneFOV, Time.deltaTime * fovTransitionSpeed);
        }
        else
        {
            // Driving
            if (phoneOverlay != null) phoneOverlay.SetActive(false);
            if (handController != null) handController.SetRaised(false);
            fomoValue -= drainRate * Time.deltaTime;
            
            // Normal FOV
            if (mainCamera != null)
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, normalFOV, Time.deltaTime * fovTransitionSpeed);
        }

        // Clamp values
        fomoValue = Mathf.Clamp(fomoValue, 0f, 100f);
        
        // Update FOMO UI slider
        if (fomoSlider != null) fomoSlider.value = fomoValue;

        // Check for FOMO loss (game over at 0)
        if (fomoValue <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}
