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
        //[SerializeField] private Color instructionColor = Color.white;
        [SerializeField] private string instructionAnimName = "InstructionShow";
        [SerializeField] private Slider timeProgressBar;

        [Header("Result Settings")]
        [SerializeField] private float winDuration = 1.5f;
        [SerializeField] private GameObject winPrefab;
        [SerializeField] private GameObject winPrefabParent;
        [SerializeField] private Vector3 winPosition = Vector3.zero;
        [Space]
        [SerializeField] private float loseDuration = 2.0f;
        [SerializeField] private GameObject losePrefab;
        [SerializeField] private GameObject losePrefabParent;
        [SerializeField] private Vector3 losePosition = Vector3.zero;

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
        /// Logic triggered when the player wins the minigame.
        /// </summary>
        protected virtual void OnWin() { }

        /// <summary>
        /// Logic triggered when the player loses the minigame.
        /// </summary>
        protected virtual void OnLose() { }
        
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
            //if (instructionDisplayText != null) instructionDisplayText.color = instructionColor;
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
            if (GameManager.Instance != null)
                GameManager.Instance.SetMinigameUIOrder(true);
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
            
            if (wasSuccessful) OnWin();
            else OnLose();
            
            if (_audioSource != null) _audioSource.Stop();
            if (timeProgressBar != null) timeProgressBar.gameObject.SetActive(false);
            
            if (minigameContainer != null && !keepContainerActiveOnEnd) 
            {
                minigameContainer.SetActive(false);
            }
            
            GameObject targetPrefab = wasSuccessful ? winPrefab : losePrefab;
            GameObject targetPrefabParent = wasSuccessful ? winPrefabParent : losePrefabParent;
            float targetDuration = wasSuccessful ? winDuration : loseDuration;
            Vector3 targetPosition = wasSuccessful ? winPosition : losePosition;

            if (targetDuration > 0)
            {
                StartCoroutine(ResultRoutine(targetPrefab, targetPrefabParent, targetPosition, targetDuration, wasSuccessful));
            }
            else
            {
                FinalizeMinigame(wasSuccessful);
            }
        }

        private IEnumerator ResultRoutine(GameObject prefab, GameObject prefabParent, Vector3 position, float duration, bool wasSuccessful)
        {
            if (prefab != null)
            {
                Transform parent = prefabParent != null ? prefabParent.transform : gameObject.transform;
                Instantiate(prefab, position, Quaternion.identity, parent);
            }
            
            PlayOptionalMusic(wasSuccessful ? winAudio : loseAudio);
            
            yield return new WaitForSeconds(duration);
            
            FinalizeMinigame(wasSuccessful);
        }

        private void FinalizeMinigame(bool wasSuccessful)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMinigameEnd(wasSuccessful);
                GameManager.Instance.SetMinigameUIOrder(false);
            }
        }

        public bool IsActive => _isGameActive;
    }
}