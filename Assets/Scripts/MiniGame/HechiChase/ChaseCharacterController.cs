using System;
using UnityEngine;

public class ChaseCharacterController : MonoBehaviour
{
    public bool IsHechi { get; private set; }

    private PlayableCharacterVisual _visual;
    private Action<ChaseCharacterController> _onEliminated;

    private void Awake()
    {
        _visual = GetComponentInChildren<PlayableCharacterVisual>(true);
    }

    public void Init(bool isHechi, Action<ChaseCharacterController> onEliminated)
    {
        IsHechi       = isHechi;
        _onEliminated = onEliminated;
        enabled = true;
    }
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        // 해치만 충돌 제거 로직을 처리 (플레이어 간 물리 충돌은 그대로 진행)
        if (!IsHechi) return;
        if (col.gameObject.TryGetComponent<ChaseCharacterController>(out var other) && !other.IsHechi)
            other.Eliminate();
    }

    public void Eliminate()
    {
        _visual.Effects.Spawn("Boom", transform.position);
        _onEliminated?.Invoke(this);
        Destroy(gameObject);
    }
}
