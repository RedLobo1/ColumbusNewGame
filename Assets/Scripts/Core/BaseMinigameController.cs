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
    [RequireComponent(typeof(AudioSource))]
    public abstract class BaseMinigameController : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] protected float gameDuration = 5f;
        [SerializeField] protected float instructionDuration = 1.5f;

        [Header("Result Logic")]
        [SerializeField] private bool useWinResultState = true;
        [SerializeField] private bool useLoseResultState = true;
        [SerializeField] private float resultDuration = 1.5f;

        [Header("Result UI References")]
        [SerializeField] private GameObject resultCanvas;
        [SerializeField] private Image resultImage;
        [SerializeField] private Sprite winSprite;
        [SerializeField] private Sprite loseSprite;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip instructionMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip winAudio;
        [SerializeField] private AudioClip loseAudio;

        [Header("Base UI References")]
        [SerializeField] private Slider timeProgressBar;
        [SerializeField] private GameObject instructionCanvas;
        [SerializeField] private Image instructionImage;

        [Header("Base Content References")]
        [SerializeField] private GameObject minigameContainer;
        [SerializeField] private Sprite instructionSprite;
        
        [Header( "Animation Settings")]
        [SerializeField] private string instructionAnimName = "InstructionShow";
        [SerializeField] private string backgroundAnimName = "BackgroundFade";
        [SerializeField] private string resultAnimName = "ResultShow";

        protected float _timeLeft;
        protected bool _isGameActive;
        private AudioSource _audioSource;
        private Animation _instructionAnim;
        private Animation _backgroundAnim;
        private Animation _resultAnim;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
        }

        protected virtual void Start()
        {
            _timeLeft = gameDuration;
            
            _backgroundAnim = instructionCanvas?.GetComponent<Animation>();
            _instructionAnim = instructionImage?.GetComponent<Animation>();
            _resultAnim = resultImage?.GetComponent<Animation>();
            
            SetupInitialState();
            StartCoroutine(MinigameRoutine());
        }

        /// <summary>
        /// Initializes UI and hides the minigame world until the instruction phase ends.
        /// </summary>
        private void SetupInitialState()
        {
            if (instructionCanvas != null) instructionCanvas.SetActive(true);
            if (resultCanvas != null) resultCanvas.SetActive(false);
            if (instructionImage != null) instructionImage.sprite = instructionSprite;
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
            PlayOptionalMusic(instructionMusic);
            
            if (_backgroundAnim != null && _backgroundAnim.GetClip(backgroundAnimName) != null)
            {
                _backgroundAnim.Play(backgroundAnimName);
            }
            
            if (_instructionAnim != null && _instructionAnim.GetClip(instructionAnimName) != null)
            {
                _instructionAnim.Play(instructionAnimName);
            }
            
            yield return new WaitForSeconds(instructionDuration);
            
            if (_audioSource != null) _audioSource.Stop();
            PlayOptionalMusic(gameplayMusic);

            // Transition to active gamepley
            if (instructionCanvas != null) instructionCanvas.SetActive(false);
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

        private void PlayOptionalMusic(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.clip = clip;
                _audioSource.Play();
            }
        }

        /// <summary>
        /// Handles the end of the minigame logic and triggers optional result states.
        /// </summary>
        public virtual void EndMinigame(bool wasSuccessful)
        {
            if (!_isGameActive) return;
            _isGameActive = false;
            
            if (_audioSource != null) _audioSource.Stop();
            if (timeProgressBar != null) timeProgressBar.gameObject.SetActive(false);
            if (minigameContainer != null) minigameContainer.SetActive(false);
            
            bool shouldShowResult = (wasSuccessful && useWinResultState) || (!wasSuccessful && useLoseResultState);

            if (shouldShowResult)
            {
                StartCoroutine(ResultRoutine(wasSuccessful));
            }
            else
            {
                FinalizeMinigame(wasSuccessful);
            }
        }

        private IEnumerator ResultRoutine(bool wasSuccessful)
        {
            if (resultCanvas != null) resultCanvas.SetActive(true);
            resultImage.sprite = wasSuccessful ? winSprite : loseSprite;
            if (_resultAnim != null && _resultAnim.GetClip(resultAnimName) != null)
            {
                _resultAnim.Play(resultAnimName);
            }
            PlayOptionalMusic(wasSuccessful ? winAudio : loseAudio);
            
            yield return new WaitForSeconds(resultDuration);
            
            FinalizeMinigame(wasSuccessful);
        }

        private void FinalizeMinigame(bool wasSuccessful)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMinigameEnd(wasSuccessful);
            }
        }

        public bool IsActive => _isGameActive;
    }
}