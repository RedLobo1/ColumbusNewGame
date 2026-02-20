using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace Julio.Core
{
    /// <summary>
    /// Manages the world map logic, ship movement, and minigame transitions.
    /// </summary>
    public class WorldMapController : MonoBehaviour
    {
        [Header("Movement Settings")] 
        [SerializeField] private Transform shipTransform;
        [SerializeField] private GameObject shipVisual;   
        [SerializeField] private List<Transform> nodePoints;
        [SerializeField] private float travelDuration = 3.0f;
        [SerializeField] private float adjustedTravelDuration
        {
            get { return this.travelDuration / (1 + GameManager.Instance.successfulGames * 0.2f); }
        }

        [SerializeField] private float waitAfterArrival = 1.0f;
        [SerializeField] private float waitAfterMinigame = 1.0f;

        [Header("Minigame Settings")]
        [SerializeField] private List<string> minigameSceneNames;
        
        [Header("Blur Settings")]
        [SerializeField] private GameObject blurCamera;
        [SerializeField] private GameObject blurOverlay;
        [SerializeField] private Material blurMaterial;
        [SerializeField] private float blurTransitionDuration = 0.5f;
        [SerializeField] private float maxBlurSize = 3.0f;
        
        [Header("UI - Lives")]
        [SerializeField] private List<GameObject> heartIcons;

        private int _lastNodeIndex = -1;
        private string _lastMinigameScene;
        private string _currentLoadedScene;
        private Coroutine _blurCoroutine;

        [SerializeField] UnityEvent onMinigameLoad;
        [SerializeField] UnityEvent onMinigameUnload;

        private void Start()
        {
            if (nodePoints.Count > 0)
            {
                shipTransform.position = nodePoints[0].position;
                _lastNodeIndex = 0;
            } 
            
            if (GameManager.Instance != null)
            {
                UpdateHeartsUI(GameManager.Instance.CurrentLives);
            }
            
            StartCoroutine(MapLoopRoutine());
        }
        
        private IEnumerator MapLoopRoutine()
        {
            while (true)
            {
                // 1. Move to random node
                int nextNode = GetRandomNodeIndex();
                yield return StartCoroutine(MoveShipRoutine(nodePoints[nextNode].position));
                _lastNodeIndex = nextNode;
                yield return new WaitForSeconds(waitAfterArrival);

                // 2. Load Minigame
                string sceneToLoad = GetRandomMinigameScene();
                _lastMinigameScene = sceneToLoad;
                
                yield return StartCoroutine(LoadMinigameAdditive(sceneToLoad));

                // 3. Wait until minigame finishes (GameManager will notify us)
                yield return new WaitUntil(() => _currentLoadedScene == null);
                yield return new WaitForSeconds(waitAfterMinigame);
            }
        }

        private IEnumerator MoveShipRoutine(Vector3 targetPosition)
        {
            float elapsed = 0;
            Vector3 startPos = shipTransform.position;

            while (elapsed < adjustedTravelDuration)
            {
                elapsed += Time.deltaTime;
                shipTransform.position = Vector3.Lerp(startPos, targetPosition, elapsed / adjustedTravelDuration);
                yield return null;
            }
            
            shipTransform.position = targetPosition;
        }

        private IEnumerator LoadMinigameAdditive(string sceneName)
        {
            if (blurMaterial != null)
            {
                if (_blurCoroutine != null) StopCoroutine(_blurCoroutine);
                _blurCoroutine = StartCoroutine(FadeBlur(maxBlurSize));
            }
            
            if (blurCamera != null) blurCamera.SetActive(true);
            if (blurOverlay != null) blurOverlay.SetActive(true);
            if (shipVisual != null) shipVisual.SetActive(false);

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;
            
            _currentLoadedScene = sceneName;
            onMinigameLoad?.Invoke();
            
        }

        /// <summary>
        /// Unloads the current minigame and returns focus to the map.
        /// Called via events or by the GameManager.
        /// </summary>
        public void UnloadMinigame()
        {
            if (string.IsNullOrEmpty(_currentLoadedScene)) return;

            StartCoroutine(UnloadRoutine());
        }

        private IEnumerator UnloadRoutine()
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(_currentLoadedScene);
            while (!unloadOp.isDone) yield return null;
            
            _currentLoadedScene = null;
            
            // Start blur transition back to 0
            if (blurMaterial != null)
            {
                if (_blurCoroutine != null) StopCoroutine(_blurCoroutine);
                _blurCoroutine = StartCoroutine(FadeBlur(0f));
            }
            
            yield return new WaitForSeconds(blurTransitionDuration);
            
            if (shipVisual != null) shipVisual.SetActive(true);
            if (blurOverlay != null) blurOverlay.SetActive(false);
            if (blurCamera != null) blurCamera.SetActive(false);
            onMinigameUnload?.Invoke();

        }

        private int GetRandomNodeIndex()
        {
            int index = _lastNodeIndex;
            
            while (index == _lastNodeIndex && nodePoints.Count > 1)
            {
                index = Random.Range(0, nodePoints.Count);
            }
            return index;
        }
        
        private string GetRandomMinigameScene()
        {
            string scene = _lastMinigameScene;

            while (scene == _lastMinigameScene && minigameSceneNames.Count > 1)
            {
                scene = minigameSceneNames[Random.Range(0, minigameSceneNames.Count)];
            }
            
            return scene;
        }
        
        public void UpdateHeartsUI(int currentLives)
        {
            for (int i = 0; i < heartIcons.Count; i++)
            {
                if (heartIcons[i] != null)
                {
                    if (i == currentLives) 
                    {
                        OnLifeLostVisuals(heartIcons[i]);
                    }
                    
                    heartIcons[i].SetActive(i < currentLives);
                }
            }
        }
        
        /// <summary>
        /// Triggered whenever a heart is lost. 
        /// Use this to play animations, shakes, or particle effects.
        /// </summary>
        private void OnLifeLostVisuals(GameObject heartObject)
        {
            Debug.Log($"Life lost! Triggering feedback for: {heartObject.name}");
    
            // FUTURE: Add camera shake here: Camera.main.GetComponent<ScreenShake>().Shake();
            // FUTURE: heartObject.GetComponent<Animation>().Play("HeartBreak");
        }
        
        /// <summary>
        /// Smoothly transitions the blur size over time.
        /// </summary>
        private IEnumerator FadeBlur(float targetSize)
        {
            float startSize = blurMaterial.GetFloat("_Size");
            float elapsed = 0;

            while (elapsed < blurTransitionDuration)
            {
                elapsed += Time.deltaTime;
                float currentSize = Mathf.Lerp(startSize, targetSize, elapsed / blurTransitionDuration);
                blurMaterial.SetFloat("_Size", currentSize);
                yield return null;
            }
            blurMaterial.SetFloat("_Size", targetSize);
        }
    }
}
