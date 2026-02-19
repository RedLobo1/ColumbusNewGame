using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Julio.Core
{
    /// <summary>
    /// Base class for all minigames handling lifecycle and instruction UI.
    /// </summary>
    public abstract class BaseMinigameController : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] protected float gameDuration = 5f;
        [SerializeField] protected float instructionDuration = 1.5f;

        [Header("Base UI References")]
        [SerializeField] private Slider timeProgressBar;
        [SerializeField] private Image instructionImageDisplay;
        [SerializeField] private GameObject uiOverlayPanel;

        [Header("Base Content")]
        [SerializeField] private GameObject minigameContainer;
        [SerializeField] private Sprite instructionSprite;
        [SerializeField] private string instructionAnimName = "InstructionShow";
        [SerializeField] private string backgroundAnimName = "BackgroundFade";

        protected float _timeLeft;
        protected bool _isGameActive;
        
        private Animation _instructionAnim;
        private Animation _backgroundAnim;

        protected virtual void Start()
        {
            _timeLeft = gameDuration;
            
            _backgroundAnim = uiOverlayPanel?.GetComponent<Animation>();
            _instructionAnim = instructionImageDisplay?.GetComponent<Animation>();
            
            SetupInitialState();
            StartCoroutine(MinigameRoutine());
        }

        /// <summary>
        /// Initializes UI and hides the minigame world until the instruction phase ends.
        /// </summary>
        private void SetupInitialState()
        {
            if (uiOverlayPanel != null) uiOverlayPanel.SetActive(true);
            if (instructionImageDisplay != null) instructionImageDisplay.sprite = instructionSprite;
            if (minigameContainer != null) minigameContainer.SetActive(false);
            if (timeProgressBar != null)
            {
                timeProgressBar.gameObject.SetActive(false);
                timeProgressBar.value = 1.0f;
            }
        }

        /// <summary>
        /// Handles the lifecycle of the minigame: Instruction -> Play -> Result.
        /// </summary>
        private IEnumerator MinigameRoutine()
        {
            if (_backgroundAnim != null && _backgroundAnim.GetClip(backgroundAnimName) != null)
            {
                _backgroundAnim.Play(backgroundAnimName);
            }
            
            if (_instructionAnim != null && _instructionAnim.GetClip(instructionAnimName) != null)
            {
                _instructionAnim.Play(instructionAnimName);
            }
            
            yield return new WaitForSeconds(instructionDuration);

            // Transition to active gamepley
            if (uiOverlayPanel != null) uiOverlayPanel.SetActive(false);
            if (minigameContainer != null) minigameContainer.SetActive(true);
            if (timeProgressBar != null) timeProgressBar.gameObject.SetActive(true);
            
            _isGameActive = true;

            while (_timeLeft > 0)
            {
                if (!_isGameActive) yield break;

                _timeLeft -= Time.deltaTime;
                UpdateProgressUI();
                yield return null;
            }

            EndMinigame(true);
        }

        /// <summary>
        /// Update visual time progress indicator.
        /// </summary>
        private void UpdateProgressUI()
        {
            if (timeProgressBar != null)
            {
                timeProgressBar.value = _timeLeft / gameDuration;
            }
        }

        /// <summary>
        /// Reports the minigame result to the global GameManager.
        /// </summary>
        public virtual void EndMinigame(bool wasSuccessful)
        {
            if (!_isGameActive) return;
            _isGameActive = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMinigameEnd(wasSuccessful);
            }
        }

        public bool IsActive => _isGameActive;
    }
}