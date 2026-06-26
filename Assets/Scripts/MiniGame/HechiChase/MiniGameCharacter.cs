using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MiniGameCharacter : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    
    public int PlayerId { get; private set; }
    public bool IsHechi { get; private set; }

    private IPlayerInputReader _input;
    private Rigidbody2D _rb;
    private Action<MiniGameCharacter> _onEliminated;

    private void Awake()
    {
        enabled = false; // Init 호출 전까지 Update 차단
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Init(int playerId, bool isHechi, Action<MiniGameCharacter> onEliminated)
    {
        PlayerId      = playerId;
        IsHechi       = isHechi;
        _onEliminated = onEliminated;
        _input  = GameManager.Instance.GetPlayerInputReader(playerId);
        enabled = true;
    }

    private void FixedUpdate()
    {
        var move = new Vector2(_input.Stick.x, _input.Stick.y) * moveSpeed;

        if (_rb.bodyType == RigidbodyType2D.Kinematic)
            _rb.MovePosition(_rb.position + move * Time.fixedDeltaTime);
        else
            _rb.linearVelocity = move;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // 해치만 충돌 제거 로직을 처리 (플레이어 간 물리 충돌은 그대로 진행)
        if (!IsHechi) return;
        if (col.gameObject.TryGetComponent<MiniGameCharacter>(out var other) && !other.IsHechi)
            other.Eliminate();
    }

    public void Eliminate()
    {
        _onEliminated?.Invoke(this);
        Destroy(gameObject);
    }
}
