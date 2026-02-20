using Julio.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Julio.Minigames.Dancing
{
    public class DancingMinigame : MonoBehaviour
    {
        // ─── Inspector Variables ───────────────────────────────────────────────────

        [Header("Confetti")] public GameObject confetti1;
        public GameObject confetti2;

        [Header("Dancers")] public SpriteRenderer dancer1SpriteRenderer;
        public SpriteRenderer dancer2SpriteRenderer;

        [Header("Dancer Sprites")] public Sprite dancer1Neutral;
        public Sprite dancer1Left;
        public Sprite dancer1Right;

        public Sprite dancer2Neutral;
        public Sprite dancer2Left;
        public Sprite dancer2Right;

        [Header("Player")] public SpriteRenderer playerSpriteRenderer;
        public Sprite playerNeutral;
        public Sprite playerLeft;
        public Sprite playerRight;
        public Sprite playerWin;
        public Sprite playerLose;

        [Header("Win / Lose Sprites")] public Sprite dancer1Win;
        public Sprite dancer1Lose;
        public Sprite dancer2Win;
        public Sprite dancer2Lose;

        [Header("Audio")] public AudioSource audioSource;
        public AudioClip leftSound;
        public AudioClip rightSound;
        public AudioClip successSound;
        public AudioClip failureSound;

        [Header("UI Indicators")] public GameObject watchIndicator; // "WATCH" image — shown during demo
        public GameObject danceIndicator; // "DANCE" image — shown during player input

        [Header("Difficulty / Timing")] [Tooltip("Base number of moves in a combo at difficulty 1")]
        public int baseComboLength = 3;

        [Tooltip("Each difficulty level adds this many extra moves")]
        public int movesPerDifficultyLevel = 1;

        [Tooltip("Current difficulty (1 = easiest)")]
        public int difficulty = 1;

        [Tooltip("How many full combo sets the player must replicate")]
        public int requiredSets = 1;

        [Tooltip("Seconds between each dancer move during the demo")]
        public float demoMoveInterval = 0.6f;

        [Tooltip("Seconds the dancer holds a pose before returning to neutral")]
        public float poseDuration = 0.35f;

        [Tooltip("Seconds to wait after demo before player input begins")]
        public float preInputDelay = 1f;

        [Tooltip("Seconds player has per input before timing out")]
        public float inputTimeout = 3f;

        // ─── Public State ──────────────────────────────────────────────────────────

        [HideInInspector] public bool hasSucceeded = false;

        // ─── Private State ─────────────────────────────────────────────────────────

        private List<int> currentCombo = new List<int>(); // 0 = left, 1 = right
        private bool waitingForInput = false;
        private bool setCompletedSuccessfully = false;
        private int playerInputIndex = 0;
        private int setsCompleted = 0;
        private bool gameActive = false;
        
        private DancingController _controller;

        // ──────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<DancingController>();
        }

        private void OnEnable()
        {
            StartMinigame();
        }

        void Update()
        {
            if (!waitingForInput || !gameActive) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                HandlePlayerInput(0);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                HandlePlayerInput(1);
        }

        // ─── Public Entry Point ────────────────────────────────────────────────────

        /// <summary>Call this to (re)start the minigame.</summary>
        public void StartMinigame()
        {
            difficulty = 1 + (GameManager.Instance.successfulGames / 4);
            hasSucceeded = false;
            setsCompleted = 0;
            gameActive = true;
            SetNeutral();
            SetIndicator(false, false);
            StartCoroutine(RunMinigame());
        }


        // ─── Core Routine ──────────────────────────────────────────────────────────

        private IEnumerator RunMinigame()
        {
            int comboLength = baseComboLength + (difficulty - 1) * movesPerDifficultyLevel;
            comboLength = Mathf.Max(1, comboLength);

            for (int set = 0; set < requiredSets; set++)
            {
                // Generate a fresh combo for each set
                GenerateCombo(comboLength);

                // ── Demo Phase ──
                SetIndicator(true, false);
                yield return StartCoroutine(PlayDemoCombo());

                // ── Wait before player input ──
                SetIndicator(false, false);
                yield return new WaitForSeconds(preInputDelay);
                SetNeutral();

                // ── Input Phase ──
                SetIndicator(false, true);
                playerInputIndex = 0;
                setCompletedSuccessfully = false;
                waitingForInput = true;

                float timer = 0f;
                while (waitingForInput && gameActive)
                {
                    timer += Time.deltaTime;
                    if (timer >= inputTimeout)
                    {
                        OnFailure();
                        yield break;
                    }

                    yield return null;
                }

                if (!gameActive) yield break;
                if (!setCompletedSuccessfully) yield break;

                // Let the last pose animation finish before moving on
                yield return new WaitForSeconds(poseDuration);
            }

            // All sets completed successfully
            OnSuccess();
        }

        // ─── Demo ──────────────────────────────────────────────────────────────────

        private IEnumerator PlayDemoCombo()
        {
            foreach (int move in currentCombo)
            {
                PlayMove(move);
                yield return new WaitForSeconds(poseDuration);
                SetNeutral();
                yield return new WaitForSeconds(demoMoveInterval - poseDuration);
            }
            //confetti1.SetActive(true);
        }

        // ─── Player Input ──────────────────────────────────────────────────────────

        private void HandlePlayerInput(int input)
        {
            if (!waitingForInput || !gameActive) return;

            PlayMove(input);
            SetPlayerSprite(input);
            StartCoroutine(ReturnToNeutralAfterDelay(poseDuration));

            if (input != currentCombo[playerInputIndex])
            {
                OnFailure();
                return;
            }

            playerInputIndex++;

            if (playerInputIndex >= currentCombo.Count)
            {
                setCompletedSuccessfully = true;
                waitingForInput = false;
                setsCompleted++;
            }
        }

        private IEnumerator ReturnToNeutralAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!gameActive) yield break; // Don't overwrite win/lose sprites
            SetNeutral();
        }

        // ─── Indicators ────────────────────────────────────────────────────────────

        private void SetIndicator(bool watch, bool dance)
        {
            if (watchIndicator) watchIndicator.SetActive(watch);
            if (danceIndicator) danceIndicator.SetActive(dance);
        }

        // ─── Combo Generation ──────────────────────────────────────────────────────

        private void GenerateCombo(int length)
        {
            currentCombo.Clear();
            for (int i = 0; i < length; i++)
                currentCombo.Add(Random.Range(0, 2)); // 0 = left, 1 = right
        }

        // ─── Sprite + Audio ────────────────────────────────────────────────────────

        private void PlayMove(int direction)
        {
            if (direction == 0)
            {
                SetSprites(dancer1Left, dancer2Left);
                PlaySound(leftSound);
            }
            else
            {
                SetSprites(dancer1Right, dancer2Right);
                PlaySound(rightSound);
            }
        }

        private void SetPlayerSprite(int direction)
        {
            if (!playerSpriteRenderer) return;
            playerSpriteRenderer.sprite = direction == 0 ? playerLeft : playerRight;
        }

        private void SetNeutral()
        {
            SetSprites(dancer1Neutral, dancer2Neutral);
            if (playerSpriteRenderer) playerSpriteRenderer.sprite = playerNeutral;
        }

        private void SetSprites(Sprite s1, Sprite s2)
        {
            if (dancer1SpriteRenderer) dancer1SpriteRenderer.sprite = s1;
            if (dancer2SpriteRenderer) dancer2SpriteRenderer.sprite = s2;
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource && clip)
                audioSource.PlayOneShot(clip);
        }

        // ─── Outcome ───────────────────────────────────────────────────────────────

        private void OnSuccess()
        {
            hasSucceeded = true;
            gameActive = false;
            waitingForInput = false;
            SetIndicator(false, false);
            SetSprites(dancer1Win, dancer2Win);
            if (playerSpriteRenderer) playerSpriteRenderer.sprite = playerWin;
            PlaySound(successSound);
            confetti2.SetActive(true);
            Debug.Log("Dance Minigame: SUCCESS!");
            // Hook in your own success logic / event here
            
            _controller.EndMinigame(true);
        }

        private void OnFailure()
        {
            hasSucceeded = false;
            gameActive = false;
            waitingForInput = false;
            SetIndicator(false, false);
            SetSprites(dancer1Lose, dancer2Lose);
            if (playerSpriteRenderer) playerSpriteRenderer.sprite = playerLose;
            PlaySound(failureSound);
            Debug.Log("Dance Minigame: FAILURE!");
            // Hook in your own failure logic / event here
            
            _controller.EndMinigame(false);
        }
    }
}
