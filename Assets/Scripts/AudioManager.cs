using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Menu Music")]
    public AudioSource menuMusicSource;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMenuMusic(bool play)
    {
        if (menuMusicSource == null) return;
        
        if (play && !menuMusicSource.isPlaying) menuMusicSource.Play();
        else if (!play && menuMusicSource.isPlaying) menuMusicSource.Stop();
    }
}
