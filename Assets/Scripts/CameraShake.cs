using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // Singleton - robíme to takto, aby si v iných skriptoch nemusel prácne hľadať kameru
    public static CameraShake Instance;

    private Vector3 originalPos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        // Zakaždým, keď dostaneme novú ranu, prestaneme prípadný starý tras a začneme nový
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Náhodný malinký pohyb po X a Y
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        // Keď trasenie skončí, kameru upraceme na miesto
        transform.localPosition = originalPos;
    }
}
