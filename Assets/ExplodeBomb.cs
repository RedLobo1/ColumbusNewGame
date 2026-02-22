using UnityEngine;
using UnityEngine.UI;

public class ExplodeBomb : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject objectToEnable;
    [SerializeField] private GameObject objectToDisable;
    [SerializeField] private AudioSource hissSound;
    [SerializeField] private AudioSource explosionSound;

    [Header("Slider")]
    [SerializeField] private Slider slider;

    void Start()
    {
        hissSound.Play();
    }

    private void Update()
    {
        if(slider.value <= 0.1f)
        {
            explosionSound.Play();
            objectToEnable.SetActive(true);
            objectToDisable.SetActive(false);
        }
    }
}
