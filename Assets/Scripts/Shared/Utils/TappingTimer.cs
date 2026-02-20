using UnityEngine;
using UnityEngine.Events;

namespace Julio.Utils
{
    public class TappingTimer : MonoBehaviour
    {
        [SerializeField] private float duration = 5f;
        [SerializeField] private GameObject countdownTextObject;
        [SerializeField] private float countdownDuration = 3f;
        public UnityEvent onMinigameEnd;
        public UnityEvent onMinigameStart;

        private float timeRemaining;
        private bool isRunning = false;
        private float countdownTimer;

        private void Start()
        {
            timeRemaining = duration;
            countdownTimer = countdownDuration;

            if (countdownTextObject != null)
                countdownTextObject.SetActive(true);
        }

        private void Update()
        {
            if (!isRunning)
            {
                countdownTimer -= Time.deltaTime;

                if (countdownTimer <= 0f)
                {
                    if (countdownTextObject != null)
                        countdownTextObject.SetActive(false);

                    isRunning = true;
                    onMinigameStart?.Invoke();
                }

                return;
            }

            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                EndMinigame();
            }
        }

        private void EndMinigame()
        {
            isRunning = false;
            onMinigameEnd?.Invoke();
            Debug.Log("Minigame Ended!");
        }
    }
}
