using UnityEngine;
using System.Collections;

namespace Julio.Core
{
    public class ResultSceneController : MonoBehaviour
    {
        [SerializeField] private float autoReturnDelay = 5.0f;

        void Start()
        {
            StartCoroutine(ReturnToMenuRoutine());
        }

        private IEnumerator ReturnToMenuRoutine()
        {
            yield return new WaitForSeconds(autoReturnDelay);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetGame();
            }
        }
    }
}
