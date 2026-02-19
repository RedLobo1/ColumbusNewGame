using Julio.Core;
using UnityEngine;

namespace Julio.Minigames.AvoidWater
{ 
    /// <summary>
    /// Handles vertical lane-based movement and dynamic sorting order for the player.
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    { 
        [SerializeField] private float lerpSpeed = 10f;
        
        [Header("Rendering")]
        [SerializeField] private int baseSortingOrder = 1;

        private int _currentLane = 1;
        private AvoidWaterController _controller;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<AvoidWaterController>();
            // Cache the renderer from the child object
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        /// <summary>
        /// Called when the player object is activated.
        /// Ensures correct sorting order.
        /// </summary>
        private void OnEnable()
        {
            UpdateSortingOrder();
        }
    
        private void Update()
        {
            if (_controller != null && !_controller.IsActive) return;

            HandleInput();
            MovePlayer();
        }
    
        /// <summary>
        /// Processes player input for lane switching.
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (_currentLane < _controller.lanes.Length - 1)
                {
                    _currentLane++;
                    UpdateSortingOrder();
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                if (_currentLane > 0)
                {
                    _currentLane--;
                    UpdateSortingOrder();
                }
            }
        }
        
        /// <summary>
        /// Updates the SpriteRenderer's sorting order based on the current lane.
        /// </summary>
        private void UpdateSortingOrder()
        {
            if (_spriteRenderer != null)
            {
                // Lane 0 (bottom) -> Order 3
                // Lane 1 (middle) -> Order 2
                // Lane 2 (top)    -> Order 1
                _spriteRenderer.sortingOrder = baseSortingOrder + (_controller.lanes.Length - 1 - _currentLane);
            }
        }

        /// <summary>
        /// Smoothly moves the player to the target lane position.
        /// </summary>
        private void MovePlayer()
        {
            Vector3 targetPos = new Vector3(transform.position.x, _controller.lanes[_currentLane], 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * lerpSpeed);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Obstacle"))
            {
                _controller?.EndMinigame(false);
            }
        }
    }
}