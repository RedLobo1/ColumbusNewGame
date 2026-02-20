using System.Collections;
using Julio.Core;
using TMPro;
using UnityEngine;

namespace Julio.Minigames.AvoidWater
{
    /// <summary>
    /// Local controller for the Avoid Water minigame logic.
    /// </summary>
    public class AvoidWaterController : BaseMinigameController 
    {
        [Header("Logic Data")]
        public float[] lanes = { -2f, 0f, 2f };
        
        [SerializeField] private Animator animator;

        protected override void OnLose()
        {
            animator.Play("PlayerHit");
        }
    }
}
