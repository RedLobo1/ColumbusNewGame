using Julio.Core;
using UnityEngine;

namespace Julio.Minigames.StopAtRightTime
{
    public class StopAtRightTimeMinigame : MonoBehaviour
    {
        [Header("Interpolated Item")] [SerializeField]
        private StopAtRightTimeItem interpolatedItem;

        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float interpolationDuration = 2f;
        [SerializeField] private bool pingPong = true;

        [Header("Timer")] [SerializeField] private float timerDuration = 5f;

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

        
        private void OnEnable()
        {
            StartMinigame();
        }

        [Header("Difficulty")]
        [SerializeField] private float speedIncreasePerTwoGames = 0.2f;

        private float _baseInterpolationDuration;

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<StopAtRightTimeController>();
            _baseInterpolationDuration = interpolationDuration;
        }

        public void StartMinigame()
        {
            hasSucceeded = false;
            interpolationTimer = 0f;
            timer = timerDuration;
            isRunning = true;
            isStopped = false;

            interpolationDuration = _baseInterpolationDuration / (1 + (GameManager.Instance.successfulGames / 2) * speedIncreasePerTwoGames);

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

            CheckOverlap();
        }

        private void CheckOverlap()
        {
            if (interpolatedItem == null) return;
            hasSucceeded = interpolatedItem.isOverlapping;
            Debug.Log($"[StopMinigame] Result: {(hasSucceeded ? "SUCCESS" : "FAIL")}");
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
