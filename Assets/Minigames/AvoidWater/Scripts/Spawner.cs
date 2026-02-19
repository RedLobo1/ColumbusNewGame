using System.Collections;
using Julio.Core;
using UnityEngine;

namespace Julio.Minigames.AvoidWater
{
    /// <summary>
    /// Spawns obstacles in random lanes while the minigame is active.
    /// </summary>
    public class Spawner : MonoBehaviour
    { 
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private float baseSpawnRate = 0.5f;
        
        private AvoidWaterController _controller;

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<AvoidWaterController>();
        }

        private void OnEnable()
        {
            float screenRightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, 0)).x;
            transform.position = new Vector3(screenRightEdge, transform.position.y, 0);   
        
            // Restart the routine whenever the minigame starts
            StopAllCoroutines();
            StartCoroutine(SpawnRoutine());
        }

        /// <summary>
        /// Periodically instantiates obstacles based on the game state.
        /// </summary>
        IEnumerator SpawnRoutine()
        { 
            // Wait for the controller to become active (after instruction)
            yield return new WaitUntil(() => _controller != null && _controller.IsActive);

            while (_controller != null && _controller.IsActive)
            {
                SpawnObstacle();

                float speedMult = GameManager.Instance != null ? GameManager.Instance.globalSpeedMultiplier : 1f;
                float currentWaitTime = Mathf.Max(baseSpawnRate / speedMult, 0.3f);

                yield return new WaitForSeconds(currentWaitTime);
            }
        }

        void SpawnObstacle()
        {
            if (_controller == null) return;
            
            float laneY = _controller.lanes[Random.Range(0, _controller.lanes.Length)];
            Vector3 spawnPos = new Vector3(transform.position.x, laneY, 0);

            Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, transform.parent);
        }
    }
}