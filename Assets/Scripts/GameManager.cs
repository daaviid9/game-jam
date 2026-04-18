using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameOver = false;

    [Header("Score System")]
    public float currentScore = 0f;
    private float highscore = 0f;
    [Tooltip("UI Text pre aktuálne skóre")]
    public TMP_Text scoreText;
    [Tooltip("UI Text pre najvyššie skóre (highscore)")]
    public TMP_Text highscoreText;

    [Header("Difficulty Over Time")]
    [Tooltip("O koľko sa rýchlosť hry zvýši každú sekundu")]
    public float speedIncreaseRate = 0.1f;
    [Tooltip("Maximálna povolená rýchlosť sveta (limit)")]
    public float maxSpeed = 30f;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
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
        if (highscoreText != null)
        {
            highscoreText.text = "Highscore: " + Mathf.FloorToInt(highscore).ToString();
        }
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

        // 1. Zvyšovanie skóre na základe prejdenej vzdialenosti za čas
        currentScore += WorldMover.moveSpeed * Time.deltaTime;
        
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(currentScore).ToString();
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
            PlayerPrefs.SetFloat("Highscore", highscore);
            PlayerPrefs.Save();
            
            if (highscoreText != null)
            {
                highscoreText.text = "Highscore: " + Mathf.FloorToInt(highscore).ToString();
            }
        }

        // Show UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pause the world speed via the static variable
        WorldMover.moveSpeed = 0f;
    }

    public void RestartGame()
    {
        // Reset speed and reload
        WorldMover.moveSpeed = 12f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        // Obnovenie rýchlosti pre novú hru a načítanie menu scény
        WorldMover.moveSpeed = 12f; 
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
