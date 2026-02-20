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
        [SerializeField] private int maxLives = 3;
        private int _currentLives;
        [SerializeField] public int successfulGames = 0;
        
        [Header("Difficulty")]
        public float globalSpeedMultiplier = 1f;
        
        public int CurrentLives => _currentLives;

        public bool lastGameWon;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _currentLives = maxLives;
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
                lastGameWon = true;
            }
            else
            {
                _currentLives--; 
                // Update UI on the map immediately
                WorldMapController map = Object.FindAnyObjectByType<WorldMapController>();
                if (map != null) map.UpdateHeartsUI(_currentLives);

                lastGameWon=false;

                if (_currentLives <= 0)
                {
                    Debug.Log("Game Over!");
                    // To-do: Trigger Game Over Screen
                }
            }
            
            WorldMapController controller = Object.FindAnyObjectByType<WorldMapController>();
            if (controller != null)
            {
                controller.UnloadMinigame();
            }
        }
    }
}