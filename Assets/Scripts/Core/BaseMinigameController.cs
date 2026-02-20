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
        [SerializeField] private GameObject minigameContainer;
        [SerializeField] protected float gameDuration = 5f;
        [SerializeField] private bool keepContainerActiveOnEnd = true;
        
        [Header("Render Texture Settings")]
        [SerializeField] private RenderTexture minigameTargetTexture;
        public Camera MinigameCamera;
        
        [Header("Instruction Settings")]
        [SerializeField] protected float instructionDuration = 1.5f;
        [SerializeField] private GameObject instructionCanvas;
        [SerializeField] private TextMeshProUGUI instructionDisplayText; 
        [SerializeField] private string instructionText = "DO SOMETHING!";
        [SerializeField] private Color instructionColor = Color.white;
        [SerializeField] private string instructionAnimName = "InstructionShow";
        [SerializeField] private Slider timeProgressBar;

        [Header("Result Settings")]
        [SerializeField] private float resultDuration = 1.5f; 
        [SerializeField] private GameObject winPrefab;
        [SerializeField] private GameObject losePrefab;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip instructionMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip winAudio;
        [SerializeField] private AudioClip loseAudio;
        
        protected float _timeLeft;
        protected bool _isGameActive;
        
        private AudioSource _audioSource;
        private Animation _instructionAnim;
        private Animation _backgroundAnim;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            
            // Direct scene-based camera search
            MinigameCamera = FindLocalCamera();
            
            if (MinigameCamera == null)
            {
                Debug.LogWarning($"No camera found in the minigame scene: {gameObject.scene.name}");
            }
            else if (minigameTargetTexture != null)
            {
                MinigameCamera.targetTexture = minigameTargetTexture;
                MinigameCamera.clearFlags = CameraClearFlags.SolidColor;
                MinigameCamera.backgroundColor = Color.black;
            }
        }

        protected virtual void Start()
        {
            _timeLeft = gameDuration;
            
            _backgroundAnim = instructionCanvas?.GetComponent<Animation>();
            _instructionAnim = instructionDisplayText?.GetComponent<Animation>();
            
            SetupInitialState();
            StartCoroutine(MinigameRoutine());
        }
        
        /// <summary>
        /// Finds the camera within the same scene as this object.
        /// </summary>
        private Camera FindLocalCamera()
        {
            Camera[] cams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in cams)
            {
                // Check if the camera is in the same scene as the Spawner
                if (cam.gameObject.scene == gameObject.scene)
                {
                    return cam;
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes UI and hides the minigame world until the instruction phase ends.
        /// </summary>
        private void SetupInitialState()
        {
            if (instructionCanvas != null) instructionCanvas.SetActive(true);
            if (instructionDisplayText != null) instructionDisplayText.text = instructionText;
            if (instructionDisplayText != null) instructionDisplayText.color = instructionColor;
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
            
            if (minigameContainer != null && !keepContainerActiveOnEnd) 
            {
                minigameContainer.SetActive(false);
            }
            
            GameObject targetResult = wasSuccessful ? winPrefab : losePrefab;

            if (targetResult != null)
            {
                StartCoroutine(ResultRoutine(targetResult, wasSuccessful));
            }
            else
            {
                FinalizeMinigame(wasSuccessful);
            }
        }

        private IEnumerator ResultRoutine(GameObject resultObject, bool wasSuccessful)
        {
            Instantiate(resultObject, gameObject.scene);
            
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