using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image displayImage;
    public Image fadeOverlay;
    public CanvasGroup menuPanel;

    [Header("Loading Settings")]
    public List<Sprite> loadingSprites;
    public float displayDuration = 5f;
    public float fadeDuration = 2f;
    public float menuFadeDuration = 1f; // Samostatné nastavenie pre dĺžku fade-in menu
    public float zoomSpeed = 0.02f;
    public float maxZoom = 1.3f; // Maximálne priblíženie (1.3 = 130%)
    public float minDarkness = 0.3f; // Minimálne stmavenie (0-1)

    [Header("Scene Management")]
    public string gameSceneName = "EnviromentScene";

    private int currentSpriteIndex = 0;
    private bool isZoomingIn = true; // Striedanie smeru zoomu

    void Awake()
    {
        // Skryjeme veci úplne OKAMŽITE ešte predtým, ako Unity stihne vykresliť prvý frame hry
        if (fadeOverlay != null)
            fadeOverlay.color = Color.black; // 100% tma
            
        if (menuPanel != null)
        {
            menuPanel.alpha = 0f; // 0% viditeľnosť textu
            menuPanel.blocksRaycasts = false;
        }
    }

    void Start()
    {
        if (loadingSprites == null || loadingSprites.Count == 0)
        {
            Debug.LogWarning("No sprites assigned to LoadingScreenManager!");
            return;
        }

        // Inicializácia prvého obrázka
        displayImage.sprite = loadingSprites[0];
        displayImage.color = Color.white;
        displayImage.transform.localScale = Vector3.one;

        StartCoroutine(FadeThroughBlackCycle());
        StartCoroutine(FadeInMenu());
    }

    IEnumerator FadeInMenu()
    {
        if (menuPanel == null) yield break;

        float elapsed = 0;
        while (elapsed < menuFadeDuration)
        {
            elapsed += Time.deltaTime;
            menuPanel.alpha = Mathf.Lerp(0f, 1f, elapsed / menuFadeDuration);
            yield return null;
        }

        menuPanel.alpha = 1f;
        menuPanel.blocksRaycasts = true;
    }

    IEnumerator FadeThroughBlackCycle()
    {
        // 0. POČIATOČNÉ ROZSVETLENIE (Iba prvá fotka, menu má vlastný fade)
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            if (fadeOverlay != null)
            {
                float alpha = Mathf.Lerp(1f, minDarkness, t);
                fadeOverlay.color = new Color(0, 0, 0, alpha);
            }
            yield return null;
        }

        while (true)
        {
            // 1. Čakanie, kým je fotka zobrazená
            yield return new WaitForSeconds(displayDuration);

            // 2. STMIEVANIE (Fade to Black)
            elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float alpha = Mathf.Lerp(minDarkness, 1f, t);
                
                if (fadeOverlay != null)
                    fadeOverlay.color = new Color(0, 0, 0, alpha);
                
                yield return null;
            }

            // 3. ZMENA OBRÁZKA A SMERU ZOOMU
            currentSpriteIndex = (currentSpriteIndex + 1) % loadingSprites.Count;
            displayImage.sprite = loadingSprites[currentSpriteIndex];
            
            isZoomingIn = !isZoomingIn; 
            
            // Nastavenie počiatočnej veľkosti
            displayImage.transform.localScale = isZoomingIn ? Vector3.one : Vector3.one * maxZoom;

            // 4. ROZSVETĽOVANIE (Fade from Black)
            elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float alpha = Mathf.Lerp(1f, minDarkness, t);
                
                if (fadeOverlay != null)
                    fadeOverlay.color = new Color(0, 0, 0, alpha);
                
                yield return null;
            }
        }
    }

    void Update()
    {
        if (displayImage != null)
        {
            // Zoomujeme plynule v závislosti od smeru
            if (isZoomingIn)
                displayImage.transform.localScale += Vector3.one * (zoomSpeed * Time.deltaTime);
            else
                displayImage.transform.localScale -= Vector3.one * (zoomSpeed * Time.deltaTime);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
