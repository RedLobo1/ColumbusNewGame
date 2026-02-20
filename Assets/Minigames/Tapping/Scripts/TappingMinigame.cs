using Julio.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Julio.Minigames.Tapping
{
    public class TappingMinigame : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private float decayRate = 5f;
        [SerializeField] private float tapIncrease = 10f;
        bool shouldDecay = true;
        bool hasStarted = false;

        bool hasLost = false;

        [SerializeField] GameObject runningObjects;
        [SerializeField] GameObject deadSprite;
        [SerializeField] Animator handleAnim;
        
        private TappingController _controller;

        [Header("Difficulty")]
        [SerializeField] private float decayIncreasePerTwoGames = 0.5f;

        private float _baseDecayRate;

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<TappingController>();
            _baseDecayRate = decayRate;
        }

        private void OnEnable()
        {
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 60f;

            decayRate = _baseDecayRate + (GameManager.Instance.successfulGames / 2) * decayIncreasePerTwoGames;
        }

        
        private void Update()
        {
            if (!hasStarted)
            {
                slider.value -= 8f * Time.deltaTime;
            }


            if (!hasStarted) return;
            if (!shouldDecay) return;
            slider.value -= decayRate * Time.deltaTime;

            if (Input.anyKeyDown)
            {
                slider.value += tapIncrease;
            }

            if (slider.value <= 0f)
            {
                FailGame();
            }
        }

        private void FailGame()
        {
            Debug.Log("Game Failed!");
            shouldDecay = false;
            hasLost = true;
            // Your fail logic here

            runningObjects.SetActive(false);
            deadSprite.SetActive(true);
            
            _controller.EndMinigame(false);
        }

        public void StopDecay()
        {
            shouldDecay = false;
            handleAnim.Play("RunAwayAnim");
            
            _controller.EndMinigame(true);
        }

        public void StartGame()
        {
            hasStarted = true;
        }
    }
}