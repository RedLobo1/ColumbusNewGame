using System.Collections;
using Julio.Core;
using TMPro;
using UnityEngine;

namespace Julio.Minigames.AvoidWater
{
    /// <summary>
    /// Local controller for the Avoid Water minigame logic.
    /// </summary>
    public class AvoidWaterController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float gameDuration = 5f;
        [SerializeField] private string instructionText = "AVOID!";

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerTextMesh;
        [SerializeField] private TextMeshProUGUI instructionTextMesh;

        private float _timeLeft;
        private bool _isGameActive;
        
        [Header("Logic Data")]
        public float[] lanes = { -2f, 0f, 2f };

        private void Start()
        {
            _timeLeft = gameDuration;
            StartCoroutine(MinigameRoutine());
        }

        /// <summary>
        /// Handles the lifecycle of the minigame: Instruction -> Play -> Result.
        /// </summary>
        private IEnumerator MinigameRoutine()
        {
            // Show instruction
            instructionTextMesh.text = instructionText;
            yield return new WaitForSeconds(1f);
            instructionTextMesh.gameObject.SetActive(false);

            _isGameActive = true;

            while (_timeLeft > 0)
            {
                if (!_isGameActive) yield break;

                _timeLeft -= Time.deltaTime;
                timerTextMesh.text = Mathf.CeilToInt(_timeLeft).ToString();
                yield return null;
            }

            EndMinigame(true);
        }
        
        /// <summary>
        /// Terminates the minigame and reports results to the global GameManager.
        /// </summary>
        /// <param name="wasSuccessful">Result of the game.</param>
        public void EndMinigame(bool wasSuccessful)
        {
            if (!_isGameActive) return;
        
            _isGameActive = false;
            
            // Notify the global manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMinigameEnd(wasSuccessful);
            }

            Debug.Log(wasSuccessful ? "Minigame Won!" : "Minigame Failed!");
        }

        public bool IsActive => _isGameActive;
    }
}
