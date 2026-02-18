using Julio.Core;
using UnityEngine;

namespace Julio.Minigames.AvoidWater
{
    /// <summary>
    /// Handles vertical lane-based movement for the player in avoidance minigames.
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    { 
        [SerializeField] private float[] lanes = { -2f, 0f, 2f };
        [SerializeField] private float lerpSpeed = 10f;

        private int _currentLane = 1;
        private AvoidWaterController _controller;

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<AvoidWaterController>();
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
                if (_currentLane < lanes.Length - 1) _currentLane++;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                if (_currentLane > 0) _currentLane--;
            }
        }

        /// <summary>
        /// Smoothly moves the player to the target lane position.
        /// </summary>
        private void MovePlayer()
        {
            Vector3 targetPos = new Vector3(transform.position.x, lanes[_currentLane], 0);
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