using System;
using UnityEngine;

// DontWorry 전용 크로스헤어 — SR 버튼으로 발사, 해치/가짜 해치 히트 판정
public class DontWorryCrosshair : CrosshairBase
{
    [SerializeField] private float shootRadius = 0.5f;

    private Action<bool> _onShotFired; // true = 진짜 해치 맞춤

    public void Init(int playerId, Action<bool> onShotFired, Sprite sprite)
    {
        base.Init(playerId);
        _onShotFired = onShotFired;
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    protected override void OnUpdate()
    {
        if (!Input.Right) return;

        var hits = Physics2D.OverlapCircleAll(transform.position, shootRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<DontWorryAIHachi>(out _))
            {
                _onShotFired?.Invoke(true);
                return;
            }

            if (hit.TryGetComponent<DontWorryFakeHachiController>(out var fake))
            {
                fake.Eliminate();
                _onShotFired?.Invoke(false);
                return;
            }
        }
    }
}
