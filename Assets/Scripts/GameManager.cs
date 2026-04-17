using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameOver = false;

    [Header("UI Panels")]
    public GameObject gameOverPanel;

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
    }

    public void GameOver()
    {
        if (isGameOver) return; // Prevent double trigger
        
        isGameOver = true;
        Debug.Log("Game Over!");

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
}
