using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Video;

public class PhoneSystem : MonoBehaviour
{
    [Header("UI References")]
    public Slider fomoSlider;       
    public ReelsManager reelsManager;  // Prepojenie na tvoj ReelsManager skript

    [Header("Hand Reference")]
    public HandController handController;

    [Header("Post-Processing Settings")]
    public Volume postProcessVolume; 
    public float maxVignetteIntensity = 1f;
    public float vignettePower = 2f;
    
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
    private ColorAdjustments colorAdjustments;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Jump.performed += ctx => isScrolling = true;
        controls.Player.Jump.canceled += ctx => isScrolling = false;

        if (postProcessVolume == null) postProcessVolume = GetComponent<Volume>();
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out colorAdjustments);
        }
    }

    void OnEnable()
    {
        if (controls != null) controls.Enable();
    }

    void OnDisable()
    {
        if (controls != null) controls.Disable();
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
        // 1. Post-Processing
        if (vignette != null)
        {
            float fomoNormalized = fomoValue / 100f;
            float intensityFactor = 1f - Mathf.Pow(fomoNormalized, vignettePower);
            
            vignette.intensity.Override(intensityFactor * maxVignetteIntensity);
            vignette.smoothness.Override(Mathf.Lerp(0.2f, 1f, intensityFactor));

            if (colorAdjustments != null)
            {
                float exposureFactor = Mathf.Pow(intensityFactor, 3f);
                colorAdjustments.postExposure.Override(Mathf.Lerp(0f, -10f, exposureFactor));
            }
        }

        // 2. Logic
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) 
        {
            if (reelsManager != null) reelsManager.SetActive(false);
            return;
        }

        if (isScrolling)
        {
            if (reelsManager != null) reelsManager.SetActive(true);
            if (handController != null) handController.SetRaised(true);
            fomoValue += refillRate * Time.deltaTime;
            
            if (mainCamera != null)
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, phoneFOV, Time.deltaTime * fovTransitionSpeed);
        }
        else
        {
            if (reelsManager != null) reelsManager.SetActive(false);
            if (handController != null) handController.SetRaised(false);
            fomoValue -= drainRate * Time.deltaTime;
            
            if (mainCamera != null)
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, normalFOV, Time.deltaTime * fovTransitionSpeed);
        }

        fomoValue = Mathf.Clamp(fomoValue, 0f, 100f);
        if (fomoSlider != null) fomoSlider.value = fomoValue;
    }
}
