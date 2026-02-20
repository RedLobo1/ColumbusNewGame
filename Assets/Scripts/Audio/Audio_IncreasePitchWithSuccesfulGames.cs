using Julio.Core;
using UnityEngine;

public class Audio_IncreasePitchWithSuccesfulGames : MonoBehaviour
{
    [SerializeField] private float basePitch = 1.0f;
    [SerializeField] private float pitchIncreasePerGame = 0.05f;
    [SerializeField] private float maxPitch = 3.0f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = basePitch;
    }

    void Update()
    {
        float targetPitch = basePitch + (GameManager.Instance.successfulGames * pitchIncreasePerGame);
        audioSource.pitch = Mathf.Min(targetPitch, maxPitch);
    }
}
