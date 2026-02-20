using UnityEngine;
using UnityEngine.UI;

public class ExplodeBomb : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject objectToEnable;
    [SerializeField] private GameObject objectToDisable;

    [Header("Slider")]
    [SerializeField] private Slider slider;

    void Start()
    {

    }

    private void Update()
    {
        if(slider.value <= 0.1f)
        {
            objectToEnable.SetActive(true);
            objectToDisable.SetActive(false);
        }
    }
}
