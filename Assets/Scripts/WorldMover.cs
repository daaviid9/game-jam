using UnityEngine;

public class WorldMover : MonoBehaviour
{
    [Tooltip("Global movement speed of the world.")]
    public static float moveSpeed = 12f;

    void Update()
    {
        // Don't move if game is over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        // Move the object backwards (towards the player)
        this.transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        // Cleanup: If the object represents an obstacle and is well behind the player, destroy it
        if (transform.position.z < -15f && gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
