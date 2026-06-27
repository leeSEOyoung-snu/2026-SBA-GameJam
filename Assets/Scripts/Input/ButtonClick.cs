using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    public enum PlayerButton
    {
        Up,
        Down,
        Left,
        Right,
        SR,
        SL,
        Swing
    }

    [SerializeField, Range(1, 4)] private int playerId = 1;
    [SerializeField] private PlayerButton button;
    [SerializeField] private Transform punchTarget;
    [SerializeField] private Vector3 punchScale = new(0.18f, 0.18f, 0f);
    [SerializeField] private float duration = 0.18f;
    [SerializeField] private int vibrato = 8;
    [SerializeField] private float elasticity = 0.8f;

    private IPlayerInputReader _input;
    private Vector3 _originScale;

    private void Awake()
    {
        if (punchTarget == null)
            punchTarget = transform;

        _originScale = punchTarget.localScale;
    }

    private IEnumerator Start()
    {
        if (GameManager.Instance != null)
            _input = GameManager.Instance.GetPlayerInputReader(playerId);

        while (true)
        {
            if (_input != null && IsPressed())
                PlayPunch();

            yield return null;
        }
    }

    private void OnDisable()
    {
        punchTarget.DOKill();
        punchTarget.localScale = _originScale;
    }

    private bool IsPressed()
    {
        return button switch
        {
            PlayerButton.Up => _input.Up,
            PlayerButton.Down => _input.Down,
            PlayerButton.Left => _input.Left,
            PlayerButton.Right => _input.Right,
            PlayerButton.SR => _input.SR,
            PlayerButton.SL => _input.SL,
            PlayerButton.Swing => _input.Swing,
            _ => false,
        };
    }

    private void PlayPunch()
    {
        punchTarget.DOKill();
        punchTarget.localScale = _originScale;
        punchTarget.DOPunchScale(punchScale, duration, vibrato, elasticity);
    }
}
