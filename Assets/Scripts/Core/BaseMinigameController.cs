using System.Collections;
using TMPro;
using UnityEngine;
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
        [SerializeField] private TextMeshProUGUI timerTextMesh;
        [SerializeField] private Image instructionImageDisplay;
        [SerializeField] private GameObject uiOverlayPanel;

        [Header("Base Content")]
        [SerializeField] private Sprite instructionSprite;
        [SerializeField] private string animationClipName = "InstructionShow";

        protected float _timeLeft;
        protected bool _isGameActive;
        private Animation _uiAnimation;

        protected virtual void Start()
        {
            _timeLeft = gameDuration;
            _uiAnimation = instructionImageDisplay?.GetComponent<Animation>();
            
            SetupInitialUI();
            StartCoroutine(MinigameRoutine());
        }

        private void SetupInitialUI()
        {
            if (uiOverlayPanel != null) uiOverlayPanel.SetActive(true);
            if (instructionImageDisplay != null) instructionImageDisplay.sprite = instructionSprite;
        }

        /// <summary>
        /// Handles the lifecycle of the minigame: Instruction -> Play -> Result.
        /// </summary>
        private IEnumerator MinigameRoutine()
        {
            if (_uiAnimation != null && _uiAnimation.GetClip(animationClipName) != null)
            {
                _uiAnimation.Play(animationClipName);
            }
            
            yield return new WaitForSeconds(instructionDuration);

            if (uiOverlayPanel != null) uiOverlayPanel.SetActive(false);
            _isGameActive = true;

            while (_timeLeft > 0)
            {
                if (!_isGameActive) yield break;

                _timeLeft -= Time.deltaTime;
                UpdateTimerUI();
                yield return null;
            }

            EndMinigame(true);
        }

        private void UpdateTimerUI()
        {
            if (timerTextMesh != null)
                timerTextMesh.text = Mathf.CeilToInt(_timeLeft).ToString();
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