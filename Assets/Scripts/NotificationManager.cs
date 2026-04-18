using UnityEngine;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("Settings")]
    public AudioSource audioSource;
    public AudioClip[] notificationClips;
    public float minInterval = 10f;
    public float maxInterval = 30f;

    void Start()
    {
        if (notificationClips.Length > 0 && audioSource != null)
        {
            StartCoroutine(DistractionRoutine());
        }
    }

    IEnumerator DistractionRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            if (GameManager.Instance != null && GameManager.Instance.isGameOver) yield break;

            AudioClip clip = notificationClips[Random.Range(0, notificationClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}
