using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DontWorryAIHachi : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float startMoveDelay = 2f;
    [SerializeField] private Vector2 moveDurationRange = new(0.8f, 2f);   // 이동 지속 시간
    [SerializeField] private Vector2 idleDurationRange = new(0.3f, 1.2f); // 멈춤 지속 시간
    [SerializeField] private Vector2 mapMin = new(-8f, -4.5f);
    [SerializeField] private Vector2 mapMax = new(8f, 4.5f);

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private PlayableCharacterVisual _visual;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _moveDir;
    private float _stateTimer;
    private bool _isMoving;
    private bool _canMove;
    private bool _isWaitingToMove;
    private Bounds _moveBounds;
    private bool _hasMoveBounds;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _visual = GetComponentInChildren<PlayableCharacterVisual>(true);
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        _rb.gravityScale = 0f;
    }

    public void StartMovingAfterDelay()
    {
        _isMoving = false;
        _canMove = false;
        _isWaitingToMove = true;
        _stateTimer = startMoveDelay;
        _rb.linearVelocity = Vector2.zero;
    }

    public void SetMoveBounds(Bounds moveBounds)
    {
        _moveBounds = moveBounds;
        _hasMoveBounds = true;
    }

    private void FixedUpdate()
    {
        if (!_canMove)
        {
            if (!_isWaitingToMove)
                return;

            _stateTimer -= Time.fixedDeltaTime;
            if (_stateTimer <= 0f)
            {
                _canMove = true;
                _isWaitingToMove = false;
                EnterMove();
            }

            return;
        }

        _stateTimer -= Time.fixedDeltaTime;
        if (_stateTimer <= 0f)
        {
            if (_isMoving) EnterIdle();
            else EnterMove();
        }

        if (!_isMoving) return;

        // 맵 경계에서 방향 반전
        var pos = _rb.position;
        GetMoveLimits(out Vector2 min, out Vector2 max);
        if (pos.x <= min.x || pos.x >= max.x) _moveDir.x = -_moveDir.x;
        if (pos.y <= min.y || pos.y >= max.y) _moveDir.y = -_moveDir.y;

        pos += _moveDir * (moveSpeed * Time.fixedDeltaTime);
        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);
        _rb.MovePosition(pos);
        SetFacingByMoveX(_moveDir.x);
    }

    private void GetMoveLimits(out Vector2 min, out Vector2 max)
    {
        if (!_hasMoveBounds)
        {
            min = mapMin;
            max = mapMax;
            return;
        }

        GetColliderOffsets(out Vector2 minOffset, out Vector2 maxOffset);
        min = (Vector2)_moveBounds.min - minOffset;
        max = (Vector2)_moveBounds.max - maxOffset;

        if (min.x > max.x)
            min.x = max.x = _moveBounds.center.x;

        if (min.y > max.y)
            min.y = max.y = _moveBounds.center.y;
    }

    private void GetColliderOffsets(out Vector2 minOffset, out Vector2 maxOffset)
    {
        if (_collider == null)
            _collider = GetComponent<Collider2D>();

        if (_collider == null)
        {
            minOffset = Vector2.zero;
            maxOffset = Vector2.zero;
            return;
        }

        Vector2 origin = _rb.position;
        minOffset = (Vector2)_collider.bounds.min - origin;
        maxOffset = (Vector2)_collider.bounds.max - origin;
    }

    private void EnterMove()
    {
        _isMoving = true;
        _moveDir = Random.insideUnitCircle.normalized;
        SetFacingByMoveX(_moveDir.x);
        _stateTimer = Random.Range(moveDurationRange.x, moveDurationRange.y);
    }

    private void EnterIdle()
    {
        _isMoving = false;
        _rb.linearVelocity = Vector2.zero;
        _stateTimer = Random.Range(idleDurationRange.x, idleDurationRange.y);
    }

    private void SetFacingByMoveX(float moveX)
    {
        if (Mathf.Abs(moveX) <= 0.01f)
            return;

        _visual?.SetFacingByMoveX(moveX);

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (_spriteRenderer != null)
            _spriteRenderer.flipX = moveX < 0f;
    }
}
