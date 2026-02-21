using UnityEngine;

namespace Julio.Core
{
    public class ResultSceneController : MonoBehaviour
    {
        private void Awake()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetGame();
            }
        }       
    }
}
