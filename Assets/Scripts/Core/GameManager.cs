using UnityEngine;
using UnityEngine.SceneManagement;

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
        
        public int CurrentLives
        {
            get => _currentLives;
            set => _currentLives = value;
        }

        public bool IsGameOver => _currentLives <= 0;

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
                lastGameWon=false;
            }
            
            WorldMapController mapController = Object.FindAnyObjectByType<WorldMapController>();
            if (mapController != null)
            {
                mapController.RegisterResult(won);
                mapController.UnloadMinigame();
                mapController.UpdateHeartsUI(_currentLives);
            }
        }
        
        public void SetMinigameUIOrder(bool show)
        {
            WorldMapController map = Object.FindAnyObjectByType<WorldMapController>();
            if (map != null)
            {
                map.SetMinigameFrameVisibility(show);
            }
        }
        
        public void GoToWinScene() => SceneManager.LoadScene("Win");
        public void GoToLoseScene() => SceneManager.LoadScene("Lose");
        
        public void ResetGame()
        {
            _currentLives = maxLives;
            successfulGames = 0;
            globalSpeedMultiplier = 1f;
            lastGameWon = false;
        }
    }
}