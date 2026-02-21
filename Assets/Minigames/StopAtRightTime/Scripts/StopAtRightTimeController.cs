using Julio.Core;
using UnityEngine;

namespace Julio.Minigames.StopAtRightTime
{
    public class StopAtRightTimeController : BaseMinigameController
    {
        protected override void OnLose()
        {
            GameManager.Instance.CurrentLives = 0;
        }
    }
}
