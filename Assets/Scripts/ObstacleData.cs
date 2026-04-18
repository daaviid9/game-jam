using UnityEngine;

public enum ObstacleType { Car, Barricade, Edge }

public class ObstacleData : MonoBehaviour
{
    public ObstacleType obstacleType;
    
    [Tooltip("Základné poškodenie (ak ho skript hráča neprepíše zatiaľ)")]
    public int damageAmount = 100;
}
