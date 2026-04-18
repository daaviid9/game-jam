using UnityEngine;

public enum ObstacleType { Car, Barricade, Edge }

public class ObstacleData : MonoBehaviour
{
    public ObstacleType obstacleType;
    
    [Tooltip("Základné poškodenie (ak ho skript hráča neprepíše zatiaľ)")]
    public int damageAmount = 100;

    [Header("Audio")]
    public AudioClip engineLoop;
    [Range(0f, 1f)]
    public float engineVolume = 0.1f;
}
