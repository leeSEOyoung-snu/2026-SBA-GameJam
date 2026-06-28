using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleAnimationPlayer : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameDuration = 0.1f;

    private SpriteRenderer _spriteRenderer;
    private int _currentFrame;
    private float _elapsed;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;

        _elapsed += Time.deltaTime;
        if (_elapsed >= frameDuration)
        {
            _elapsed -= frameDuration;
            _currentFrame = (_currentFrame + 1) % frames.Length;
            _spriteRenderer.sprite = frames[_currentFrame];
        }
    }
}
