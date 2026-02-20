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

        void Update()
        {
            if (!hasGameStarted) return;
            if (Input.anyKey)
            {
                hasPressed = true;
                loseSprite.SetActive(true);
                baseSprite.SetActive(false);
            }
        }

        public void EndGame()
        {
            if (!hasPressed)
            {
                baseSprite.SetActive(false);
                winSprite.SetActive(true);
            }
        }

        public void StartGame()
        {
            hasGameStarted = true;
        }
    }
}