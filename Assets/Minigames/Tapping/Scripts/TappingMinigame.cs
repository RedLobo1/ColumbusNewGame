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

        private void Start()
        {
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 60f;
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
        }

        public void StopDecay()
        {
            shouldDecay = false;
            handleAnim.Play("RunAwayAnim");
        }

        public void StartGame()
        {
            hasStarted = true;
        }
    }
}