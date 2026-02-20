using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

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
        [SerializeField] private float waitAfterArrival = 1.0f;

        [Header("Minigame Settings")]
        [SerializeField] private List<string> minigameSceneNames;
        [SerializeField] private GameObject blurOverlay;
        
        [Header("UI - Lives")]
        [SerializeField] private List<GameObject> heartIcons;

        private int _lastNodeIndex = -1;
        private string _lastMinigameScene;
        private string _currentLoadedScene;

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
            }
        }

        private IEnumerator MoveShipRoutine(Vector3 targetPosition)
        {
            float elapsed = 0;
            Vector3 startPos = shipTransform.position;

            while (elapsed < travelDuration)
            {
                elapsed += Time.deltaTime;
                shipTransform.position = Vector3.Lerp(startPos, targetPosition, elapsed / travelDuration);
                yield return null;
            }
            
            shipTransform.position = targetPosition;
        }

        private IEnumerator LoadMinigameAdditive(string sceneName)
        {
            if (blurOverlay != null) blurOverlay.SetActive(true);
            if (shipVisual != null) shipVisual.SetActive(false);

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;
            
            _currentLoadedScene = sceneName;
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
            
            if (shipVisual != null) shipVisual.SetActive(true);
            if (blurOverlay != null) blurOverlay.SetActive(false);
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
    }
}
