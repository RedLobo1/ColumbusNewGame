using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Julio.Core
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 1.5f;
        
        private bool _isStarting = false;

        void Update()
        {
            // Detect any key or mouse click to start
            if (Input.anyKeyDown && !_isStarting)
            {
                StartCoroutine(StartGameRoutine());
            }
        }

        private IEnumerator StartGameRoutine()
        {
            _isStarting = true;
            Debug.Log("Starting Game...");

            // UIAnimator.Play("FadeOut");
            
            yield return StartCoroutine(FadeRoutine(1f));
            
            SceneManager.LoadScene("Map");
        }
        
        private IEnumerator FadeRoutine(float targetAlpha)
        {
            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}
