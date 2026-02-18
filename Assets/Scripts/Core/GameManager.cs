using UnityEngine;

namespace Julio.Core
{
    /// <summary>
    /// Global manager that persists across scenes to track overall game progress.
    /// </summary>
    public class GameManager : MonoBehaviour
    { 
        public static GameManager Instance { get; private set; }

        [Header("Player Stats")]
        [SerializeField] private int lives = 3;
        [SerializeField] private int successfulGames = 0;
        
        [Header("Difficulty")]
        public float globalSpeedMultiplier = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Called when a minigame finishes to update global state.
        /// </summary>
        /// <param name="won">Whether the minigame was successful.</param>
        public void OnMinigameEnd(bool won)
        {
            if (won)
            {
                successfulGames++;
                // globalSpeedMultiplier += 0.1f;
            }
            else
            {
                lives--;
                if (lives <= 0) Debug.Log("Game Over!");
            }
            
            // Logic to load next scene or map node
            // SceneManager.LoadScene("S_MapNode");
        }
    }
}