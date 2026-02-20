using Julio.Utils;
using UnityEngine;

namespace Julio.Minigames.Overlap
{
    public class OverlapMinigame : MonoBehaviour
    {
        [Header("Interpolated Item")]
        [SerializeField] private InterpolatedItem interpolatedItem;

        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float interpolationDuration = 2f;
        [SerializeField] private bool pingPong = true;

        [Header("Player")][SerializeField] private Transform player;
        [SerializeField] private float playerMoveSpeed = 3f;
        [SerializeField] private float playerMinX = -5f;
        [SerializeField] private float playerMaxX = 5f;

        [Header("Player Countdown GameObjects")]
        [SerializeField]
        private GameObject idleObject;
        [SerializeField] private GameObject almostDoneObject;
        [SerializeField] private GameObject finishedObject;
        [SerializeField] private float almostDoneThreshold = 0.33f;

        [Header("Countdown")][SerializeField] private float countdownDuration = 5f;

        // Result
        public bool hasSucceeded { get; private set; } = false;

        // Internal state
        private float countdownTimer;
        private float interpolationTimer;
        private bool isRunning = false;
        private GameObject _currentCountdownObject = null; // Track what's currently shown
        
        private OverlapController _controller;

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<OverlapController>();
        }

        private void OnEnable()
        {
            StartMinigame();
        }

        public void StartMinigame()
        {
            hasSucceeded = false;
            countdownTimer = countdownDuration;
            interpolationTimer = 0f;
            isRunning = true;
            _currentCountdownObject = null; // Reset so the first call goes through

            SetActiveCountdownObject(idleObject);
        }

        private void Update()
        {
            if (!isRunning) return;

            HandleItemInterpolation();
            HandlePlayerMovement();
            HandleCountdown();
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

        private void HandlePlayerMovement()
        {
            if (player == null) return;

            float input = Input.GetAxis("Horizontal");
            Vector3 pos = player.position;
            pos.x += input * playerMoveSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, playerMinX, playerMaxX);
            player.position = pos;
        }

        private void HandleCountdown()
        {
            countdownTimer -= Time.deltaTime;
            float fraction = countdownTimer / countdownDuration;

            if (fraction <= 0f)
                SetActiveCountdownObject(finishedObject);
            else if (fraction <= almostDoneThreshold)
                SetActiveCountdownObject(almostDoneObject);
            else
                SetActiveCountdownObject(idleObject);

            if (countdownTimer <= 0f)
            {
                countdownTimer = 0f;
                isRunning = false;
                CheckOverlap();
            }
        }

        /// <summary>
        /// Only swaps the active countdown GameObject if it has actually changed.
        /// </summary>
        private void SetActiveCountdownObject(GameObject target)
        {
            if (target == _currentCountdownObject) return; // Already showing this one, do nothing

            if (idleObject != null) idleObject.SetActive(false);
            if (almostDoneObject != null) almostDoneObject.SetActive(false);
            if (finishedObject != null) finishedObject.SetActive(false);

            if (target != null) target.SetActive(true);

            _currentCountdownObject = target;
        }

        private void CheckOverlap()
        {
            if (interpolatedItem == null) return;

            hasSucceeded = interpolatedItem.isOverlapping;
            Debug.Log($"[OverlapMinigame] Result: {(hasSucceeded ? "SUCCESS" : "FAIL")}");
            
            _controller.EndMinigame(hasSucceeded);
        }

        private void OnDrawGizmosSelected()
        {
            if (pointA != null && pointB != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pointA.position, pointB.position);
                Gizmos.DrawWireSphere(pointA.position, 0.15f);
                Gizmos.DrawWireSphere(pointB.position, 0.15f);
            }
        }
    }
}