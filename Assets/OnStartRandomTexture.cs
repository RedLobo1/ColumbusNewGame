using UnityEngine;

public class OnStartRandomTexture : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("[OnStartRandomTexture] No sprites assigned!", this);
            return;
        }

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            Debug.LogWarning("[OnStartRandomTexture] No SpriteRenderer found on this GameObject!", this);
            return;
        }

        _spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
    }
}