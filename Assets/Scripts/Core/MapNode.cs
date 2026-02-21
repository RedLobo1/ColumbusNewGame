using UnityEngine;

namespace Julio.Core
{
    public class MapNode : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private GameObject defaultVisual; // The 'X' sprite
        [SerializeField] private GameObject winVisual;     // The flag
        [SerializeField] private GameObject loseVisual;    // The failure cross

        public bool IsVisited { get; private set; }

        public void SetResult(bool won)
        {
            IsVisited = true;
            if (defaultVisual != null) defaultVisual.SetActive(false);
            
            if (won && winVisual != null) winVisual.SetActive(true);
            else if (!won && loseVisual != null) loseVisual.SetActive(true);
        }
    }
}
