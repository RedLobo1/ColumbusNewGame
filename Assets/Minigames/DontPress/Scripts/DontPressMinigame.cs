using UnityEngine;

namespace Julio.Minigames.DontPress
{
    public class DontPressMinigame : MonoBehaviour
    {
        bool hasGameStarted = false;
        bool hasPressed;


        [SerializeField] GameObject loseSprite;
        [SerializeField] GameObject winSprite;
        [SerializeField] GameObject baseSprite;
        
        private DontPressController _controller;

        private void Awake()
        {
            _controller = Object.FindAnyObjectByType<DontPressController>();
        }

        void Update()
        {
            if (!hasGameStarted) return;
            if (Input.anyKey)
            {
                hasPressed = true;
                loseSprite.SetActive(true);
                baseSprite.SetActive(false);
                
                _controller.EndMinigame(false);
            }
        }

        public void EndGame()
        {
            if (!hasPressed)
            {
                baseSprite.SetActive(false);
                winSprite.SetActive(true);
                
                _controller.EndMinigame(true);
            }
        }

        public void StartGame()
        {
            hasGameStarted = true;
        }
    }
}