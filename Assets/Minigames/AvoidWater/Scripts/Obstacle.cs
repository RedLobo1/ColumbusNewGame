using Julio.Core;
using UnityEngine;

namespace Julio.Minigames.AvoidWater
{ 
    /// <summary>
    /// Moves the obstacle to the left and manages its sorting order based on its lane.
    /// </summary>
    public class Obstacle : MonoBehaviour
    { 
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private float leftLimit = -15f;
        
        [Header("Rendering")]
        [SerializeField] private int baseSortingOrder = 1;
        
        private AvoidWaterController _controller;
        private SpriteRenderer _spriteRenderer;

        [Header("Difficulty")]
        [SerializeField] private float speedIncreasePerTwoGames = 0.1f;

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<AvoidWaterController>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        private void Start()
        {
            UpdateSortingOrder();
        }



        void Update()
        {
            float currentSpeed = baseSpeed;

            if (GameManager.Instance != null)
            {
                currentSpeed *= GameManager.Instance.globalSpeedMultiplier;
                currentSpeed *= 1 + (GameManager.Instance.successfulGames / 2) * speedIncreasePerTwoGames;
            }

            transform.Translate(Vector2.left * currentSpeed * Time.deltaTime);
            if (transform.position.x < leftLimit)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Assigns the correct sorting order based on the Y position of the obstacle.
        /// </summary>
        private void UpdateSortingOrder()
        {
            if (_spriteRenderer == null) return;

            // Find which lane index this obstacle belongs to based on Y position
            int laneIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < _controller.lanes.Length; i++)
            {
                float distance = Mathf.Abs(transform.position.y - _controller.lanes[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    laneIndex = i;
                }
            }

            // Apply the same logic as PlayerMovement: 
            // Lowest Y (lane 0) gets highest order.
            _spriteRenderer.sortingOrder = baseSortingOrder + (_controller.lanes.Length - 1 - laneIndex);
        }
    }
}