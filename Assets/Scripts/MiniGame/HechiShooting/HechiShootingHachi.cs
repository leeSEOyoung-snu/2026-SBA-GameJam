using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class HechiShootingHachi : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private Vector2 moveDurationRange = new(0.8f, 2f);
    [SerializeField] private Vector2 idleDurationRange = new(0.3f, 1.2f);
    [SerializeField] private Vector2 mapMin = new(-8f, -4.5f);
    [SerializeField] private Vector2 mapMax = new(8f, 4.5f);

    public static readonly int MaxHits = 3;

    private Rigidbody2D _rb;
    private Vector2 _moveDir;
    private float _stateTimer;
    private bool _isMoving;
    private int _hitCount;
    private Action _onGameOver;

    public void Init(Action onGameOver)
    {
        _onGameOver = onGameOver;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        EnterMove();
    }

    private void FixedUpdate()
    {
        _stateTimer -= Time.fixedDeltaTime;
        if (_stateTimer <= 0f)
        {
            if (_isMoving) EnterIdle();
            else EnterMove();
        }

        if (!_isMoving) return;

        var pos = _rb.position;
        if (pos.x <= mapMin.x || pos.x >= mapMax.x) _moveDir.x = -_moveDir.x;
        if (pos.y <= mapMin.y || pos.y >= mapMax.y) _moveDir.y = -_moveDir.y;

        pos += _moveDir * (moveSpeed * Time.fixedDeltaTime);
        pos.x = Mathf.Clamp(pos.x, mapMin.x, mapMax.x);
        pos.y = Mathf.Clamp(pos.y, mapMin.y, mapMax.y);
        _rb.MovePosition(pos);
    }

    public void Hit()
    {
        _hitCount++;
        Debug.Log($"[HechiShooting] 해치 피격 {_hitCount}/{MaxHits}");
        if (_hitCount >= MaxHits)
            _onGameOver?.Invoke();
    }

    private void EnterMove()
    {
        _isMoving = true;
        _moveDir = Random.insideUnitCircle.normalized;
        _stateTimer = Random.Range(moveDurationRange.x, moveDurationRange.y);
    }

    private void EnterIdle()
    {
        _isMoving = false;
        _rb.linearVelocity = Vector2.zero;
        _stateTimer = Random.Range(idleDurationRange.x, idleDurationRange.y);
    }
}
