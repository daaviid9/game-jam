using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPaused = false;

    [Header("Score System")]
    public float currentScore = 0f;
    private float highscore = 0f;
    [Tooltip("UI Text pre aktuálne skóre")]
    public TMP_Text scoreText;
    [Tooltip("UI Text pre najvyššie skóre (highscore)")]
    public TMP_Text highscoreText;
    [Tooltip("UI Text pre finálne skóre po prehre")]
    public TMP_Text finalDistanceText;

    [Header("Difficulty Over Time")]
    [Tooltip("O koľko sa rýchlosť hry zvýši každú sekundu")]
    public float speedIncreaseRate = 0.1f;
    [Tooltip("Maximálna povolená rýchlosť sveta (limit)")]
    public float maxSpeed = 30f;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    [Header("Scene Settings")]
    [Tooltip("Názov scény, do ktorej sa má hráč vrátiť (napríklad LoadingScreenMenu)")]
    public string mainMenuSceneName = "LoadingScreenMenu";

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Load Highscore on start
        highscore = PlayerPrefs.GetFloat("Highscore", 0f);
        // Highscore UI je dočasne skryté
        // if (highscoreText != null)
        // {
        //     highscoreText.text = "Highscore: " + Mathf.FloorToInt(highscore).ToString();
        // }
    }

    private void Start()
    {
        // Poistka: Zakaždým, keď sa načíta scéna, uisti sa, že je rýchlosť posunu sveta spustená.
        // Zabraňuje problému, ak sa hráč vrátil do editora po Game Overi a rýchlosť by ostala na 0.
        WorldMover.moveSpeed = 12f;
    }

    private void Update()
    {
        if (isGameOver) return;

        // Kontrola pre zapnutie/vypnutie pauzy (využíva New Input System, keďže projekt ho plne používa)
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        // Ak hra je pozastavená, skóre ani prekážky nebudú rásť tu
        if (isPaused) return;

        // 1. Zvyšovanie skóre na základe prejdenej vzdialenosti za čas
        currentScore += WorldMover.moveSpeed * Time.deltaTime;
        
        if (scoreText != null)
        {
            scoreText.text = Mathf.FloorToInt(currentScore).ToString() + " m";
        }

        if (finalDistanceText != null)
        {
            finalDistanceText.text = "You managed to scroll for " + Mathf.FloorToInt(currentScore).ToString() + " meters";
        }

        // 2. Postupné zrýchľovanie hry
        if (WorldMover.moveSpeed < maxSpeed)
        {
            WorldMover.moveSpeed += speedIncreaseRate * Time.deltaTime;
            // Ohraničenie maximálnej rýchlosti
            if (WorldMover.moveSpeed > maxSpeed)
            {
                WorldMover.moveSpeed = maxSpeed;
            }
        }
    }

    public void GameOver()
    {
        if (isGameOver) return; // Prevent double trigger
        
        isGameOver = true;
        Debug.Log("Game Over!");

        // Uloženie Highscore, ak hráč prekonal svoj rekord
        if (currentScore > highscore)
        {
            highscore = currentScore;
            // Highscore ukladanie do PlayerPrefs a updatovanie UI je momentálne vypnuté
            // PlayerPrefs.SetFloat("Highscore", highscore);
            // PlayerPrefs.Save();
            
            // if (highscoreText != null)
            // {
            //     highscoreText.text = "Highscore: " + Mathf.FloorToInt(highscore).ToString();
            // }
        }

        // Show UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pause the world speed via the static variable
        WorldMover.moveSpeed = 0f;
    }

    public void TogglePause()
    {
        if (isGameOver) return; // Nemôžeš pauzovať po Game Over
        
        isPaused = !isPaused;

        // Aktivuj/deaktivuj Pause UI panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        // Zmraziť / odmraziť čas v Unity
        Time.timeScale = isPaused ? 0f : 1f;

        // Zapnúť / Vypnúť všetky zvuky celoplošne (AudioListener.pause)
        AudioListener.pause = isPaused;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Vrátenie času do normálu, ak by bol hráč v pauze
        AudioListener.pause = false; // Vrátenie audia, aby zvuky opäť išli
        // Reset speed and reload
        WorldMover.moveSpeed = 12f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f; // Vrátenie času do normálu, ak by bol hráč v pauze
        AudioListener.pause = false; // Vrátenie audia, aby zvuky opäť išli
        // Obnovenie rýchlosti pre novú hru a načítanie menu scény
        WorldMover.moveSpeed = 12f; 
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
