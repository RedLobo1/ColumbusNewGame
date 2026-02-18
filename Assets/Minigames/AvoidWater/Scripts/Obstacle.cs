using Julio.Core;
using UnityEngine;

namespace Julio.Minigames.AvoidWater
{
    /// <summary>
    /// Moves the obstacle to the left and handles its lifecycle.
    /// </summary>   
    public class Obstacle : MonoBehaviour
    { 
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private float leftLimit = -15f;

        void Update()
        { 
            // Use the global speed multiplier from the core GameManager
            float currentSpeed = baseSpeed;
            
            if (GameManager.Instance != null)
            {
                currentSpeed *= GameManager.Instance.globalSpeedMultiplier;
            }

            transform.Translate(Vector2.left * currentSpeed * Time.deltaTime);

            if (transform.position.x < leftLimit)
            {
                Destroy(gameObject);
            }
        }
    }
}