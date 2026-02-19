using UnityEngine;
using UnityEngine.EventSystems;

namespace Julio.Core
{
    /// <summary>
    /// Ensures only one AudioListener and EventSystem exists during additive loading.
    /// </summary>
    public class SceneCleaner : MonoBehaviour
    {
        private void Awake()
        {
            CleanAudioListeners();
            CleanEventSystems();
        }

        /// <summary>
        /// Keeps the Main Camera's listener and destroys any additional ones.
        /// </summary>
        private void CleanAudioListeners()
        {
            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            
            if (listeners.Length > 1)
            {
                foreach (var listener in listeners)
                {
                    // Destroy listener if it's NOT on the Main Camera (map camera)
                    if (!listener.CompareTag("MainCamera"))
                    {
                        Destroy(listener);
                    }
                }
            }
        }

        /// <summary>
        /// Ensures only the primary EventSystem remains active.
        /// </summary>
        private void CleanEventSystems()
        {
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

            if (eventSystems.Length > 1)
            {
                foreach (var es in eventSystems)
                {
                    // We keep the first one found or the one that isn't this local one
                    // If this script is attached to the minigame's EventSystem, we destroy its GameObject
                    if (es.gameObject != gameObject)
                    {
                        // Found another one, so this local one is redundant
                        Destroy(gameObject);
                        break;
                    }
                }
            }
        }
    }
}
