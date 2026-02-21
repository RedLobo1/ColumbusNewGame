using Julio.Core;
using UnityEngine;

namespace Julio.Minigames.StopAtRightTime
{
    public class StopAtRightTimeMinigame : MonoBehaviour
    {
        [Header("Interpolated Item")]
        [SerializeField] private StopAtRightTimeItem interpolatedItem;
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float interpolationDuration = 2f;
        [SerializeField] private bool pingPong = true;

        [Header("Timer")]
        [SerializeField] private float timerDuration = 5f;

        // Result
        public bool hasSucceeded { get; private set; } = false;

        // Internal state
        private float interpolationTimer;
        private float timer;
        private bool isRunning = false;
        private bool isStopped = false;

        private StopAtRightTimeController _controller;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip themeClip;
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip failClip;

        [Header("Difficulty")]
        [SerializeField] private float speedIncreasePerTwoGames = 0.2f;
        private float _baseInterpolationDuration;

        [Header("Characters")]
        [SerializeField] private SpriteRenderer char1SpriteRenderer;
        [SerializeField] private SpriteRenderer char2SpriteRenderer;
        [SerializeField] private SpriteRenderer char3SpriteRenderer;

        [SerializeField] private Sprite char1IdleSprite;
        [SerializeField] private Sprite char2IdleSprite;
        [SerializeField] private Sprite char3IdleSprite;

        [SerializeField] private Sprite char1FailSprite;
        [SerializeField] private Sprite char2FailSprite;
        [SerializeField] private Sprite char3FailSprite;

        [Header("Animator")]
        [SerializeField] private Animator flagAnimator;
        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int PutDownHash = Animator.StringToHash("PutDown");

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<StopAtRightTimeController>();
            _baseInterpolationDuration = interpolationDuration;
        }

        private void OnEnable()
        {
            InitCharacters();
            StartMinigame();
        }

        private void InitCharacters()
        {
            char1SpriteRenderer.sprite = char1IdleSprite;
            char2SpriteRenderer.sprite = char2IdleSprite;
            char3SpriteRenderer.sprite = char3IdleSprite;
        }

        private void ActivateFailSprites()
        {
            char1SpriteRenderer.sprite = char1FailSprite;
            char2SpriteRenderer.sprite = char2FailSprite;
            char3SpriteRenderer.sprite = char3FailSprite;
        }

        public void StartMinigame()
        {
            hasSucceeded = false;
            interpolationTimer = 0f;
            timer = timerDuration;
            isRunning = true;
            isStopped = false;

            interpolationDuration = _baseInterpolationDuration / (1 + (GameManager.Instance.successfulGames / 2) * speedIncreasePerTwoGames);

            flagAnimator?.Play("Idle");
            PlayClip(themeClip);
        }

        private void Update()
        {
            if (!isRunning) return;

            if (!isStopped)
            {
                HandleItemInterpolation();

                if (Input.anyKeyDown)
                    Stop();
            }

            HandleTimer();
        }

        private void HandleItemInterpolation()
        {
            interpolationTimer += Time.deltaTime;

            float t = pingPong
                ? Mathf.PingPong(interpolationTimer / interpolationDuration, 1f)
                : (interpolationTimer % interpolationDuration) / interpolationDuration;

            if (interpolatedItem != null && pointA != null && pointB != null)
                interpolatedItem.transform.position = Vector3.Lerp(pointA.position, pointB.position, t);
        }

        private void HandleTimer()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                timer = 0f;
                Stop();
            }
        }

        private void Stop()
        {
            if (isStopped) return;

            isStopped = true;
            isRunning = false;

            if (interpolatedItem != null)
                interpolatedItem.PlayStopAnimation();

            flagAnimator?.Play("PutDown");
            CheckOverlap();
        }

        private void CheckOverlap()
        {
            if (interpolatedItem == null) return;

            hasSucceeded = interpolatedItem.isOverlapping;
            Debug.Log($"[StopMinigame] Result: {(hasSucceeded ? "SUCCESS" : "FAIL")}");

            if (!hasSucceeded)
                ActivateFailSprites();

            PlayClip(hasSucceeded ? successClip : failClip);
            _controller.EndMinigame(hasSucceeded);
        }

        private void PlayClip(AudioClip clip)
        {
            if (audioSource == null || clip == null) return;
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }

        private void OnDrawGizmosSelected()
        {
            if (pointA != null && pointB != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(pointA.position, pointB.position);
                Gizmos.DrawWireSphere(pointA.position, 0.15f);
                Gizmos.DrawWireSphere(pointB.position, 0.15f);
            }
        }
    }
}