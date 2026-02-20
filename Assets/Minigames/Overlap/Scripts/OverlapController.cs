using System.Collections;
using Julio.Utils;
using UnityEngine;

namespace Julio.Minigames.Overlap
{
    public class OverlapController : MonoBehaviour
    {
        [Header("Interpolated Item")] [SerializeField]
        private InterpolatedItem interpolatedItem;

        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float interpolationDuration = 2f;
        [SerializeField] private bool pingPong = true;

        [Header("Player")] [SerializeField] private Transform player;
        [SerializeField] private float playerMoveSpeed = 3f;
        [SerializeField] private float playerMinX = -5f;
        [SerializeField] private float playerMaxX = 5f;

        [Header("Player Countdown Sprites")] [SerializeField]
        private SpriteRenderer playerSpriteRenderer;

        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite almostDoneSprite;
        [SerializeField] private Sprite finishedSprite;
        [SerializeField] private float almostDoneThreshold = 0.33f; // fraction of countdown remaining

        [Header("Countdown")] [SerializeField] private float countdownDuration = 5f;


        // Result
        public bool hasSucceeded { get; private set; } = false;

        // Internal state
        private float countdownTimer;
        private float interpolationTimer;
        private bool isRunning = false;

        private void Start()
        {
            StartMinigame();
        }

        public void StartMinigame()
        {
            hasSucceeded = false;
            countdownTimer = countdownDuration;
            interpolationTimer = 0f;
            isRunning = true;

            if (playerSpriteRenderer != null && idleSprite != null)
                playerSpriteRenderer.sprite = idleSprite;
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
            float t = (interpolationTimer % interpolationDuration) / interpolationDuration;

            if (pingPong)
                t = Mathf.PingPong(interpolationTimer / interpolationDuration, 1f);

            if (interpolatedItem != null && pointA != null && pointB != null)
                interpolatedItem.transform.position = Vector3.Lerp(pointA.position, pointB.position, t);
        }

        private void HandlePlayerMovement()
        {
            float input = Input.GetAxis("Horizontal");
            if (player != null)
            {
                Vector3 pos = player.position;
                pos.x += input * playerMoveSpeed * Time.deltaTime;
                pos.x = Mathf.Clamp(pos.x, playerMinX, playerMaxX);
                player.position = pos;
            }
        }

        private void HandleCountdown()
        {
            countdownTimer -= Time.deltaTime;
            float fraction = countdownTimer / countdownDuration;

            // Update sprite based on countdown progress
            if (playerSpriteRenderer != null)
            {
                if (fraction <= 0f)
                {
                    if (finishedSprite != null)
                        playerSpriteRenderer.sprite = finishedSprite;
                }
                else if (fraction <= almostDoneThreshold)
                {
                    if (almostDoneSprite != null)
                        playerSpriteRenderer.sprite = almostDoneSprite;
                }
                else
                {
                    if (idleSprite != null)
                        playerSpriteRenderer.sprite = idleSprite;
                }
            }

            if (countdownTimer <= 0f)
            {
                countdownTimer = 0f;
                isRunning = false;
                CheckOverlap();
            }
        }

        private void CheckOverlap()
        {
            if (interpolatedItem == null) return;

            hasSucceeded = interpolatedItem.isOverlapping;

            Debug.Log($"[OverlapMinigame] Result: {(hasSucceeded ? "SUCCESS" : "FAIL")}");
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize interpolation path
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