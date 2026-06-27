using System;
using UnityEngine;

public class HechiShootingCrosshair : CrosshairBase
{
    [SerializeField] private float shootRadius = 0.5f;
    [SerializeField] private float shootCooldown = 0.3f;

    private int _playerId;
    private Action<int> _onNightmareHit;   // playerId
    private Action _onHachiHit;
    private float _cooldownTimer;

    public void Init(int playerId, Action<int> onNightmareHit, Action onHachiHit)
    {
        base.Init(playerId);
        _playerId = playerId;
        _onNightmareHit = onNightmareHit;
        _onHachiHit = onHachiHit;
    }

    protected override void OnUpdate()
    {
        _cooldownTimer -= Time.deltaTime;
        if (!Input.Right || _cooldownTimer > 0f) return;

        _cooldownTimer = shootCooldown;

        var hits = Physics2D.OverlapCircleAll(transform.position, shootRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<HechiShootingHachi>(out var hachi))
            {
                hachi.Hit();
                _onHachiHit?.Invoke();
                return;
            }

            if (hit.TryGetComponent<HechiShootingNightmare>(out var nightmare))
            {
                _onNightmareHit?.Invoke(_playerId);
                nightmare.Hit(_playerId);
                return;
            }
        }
    }
}
