using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Julio.Core
{
    public class SceneTransitionController : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string targetSceneName = "Map";
        [SerializeField] private bool waitForInput = true;
        [SerializeField] private float autoDelay = 3f;

        [Header("Fade Settings")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 1.0f;
        [SerializeField] private bool fadeInAtStart = true;

        private bool _isTransitioning = false;

        private void Start()
        {
            // Initial setup for the fade group
            if (fadeCanvasGroup != null)
            {
                if (fadeInAtStart)
                {
                    fadeCanvasGroup.alpha = 1f;
                    StartCoroutine(FadeRoutine(0f));
                }
                else
                {
                    fadeCanvasGroup.alpha = 0f;
                }
            }

            // If we don't wait for input, start the auto timer
            if (!waitForInput)
            {
                StartCoroutine(AutoTransitionRoutine());
            }
        }

        private void Update()
        {
            // Only handle input if the mode is set and we aren't moving yet
            if (waitForInput && Input.anyKeyDown && !_isTransitioning)
            {
                PerformTransition();
            }
        }

        public void PerformTransition()
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionRoutine());
        }

        private IEnumerator AutoTransitionRoutine()
        {
            yield return new WaitForSeconds(autoDelay);
            PerformTransition();
        }

        private IEnumerator TransitionRoutine()
        {
            _isTransitioning = true;

            if (fadeCanvasGroup != null)
            {
                yield return StartCoroutine(FadeRoutine(1f));
            }

            SceneManager.LoadScene(targetSceneName);
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
