using UnityEngine;

public class TopViewPhysics : MiniGamePhysicsBase
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool stickOnly;

    private Bounds _moveBounds;
    private bool _hasMoveBounds;
    private Collider2D _collider;

    public void SetMoveBounds(Bounds moveBounds)
    {
        _moveBounds = moveBounds;
        _hasMoveBounds = true;
        _collider = GetComponent<Collider2D>();
    }

    public override Vector2 UpdatePhysics(float deltaTime)
    {
        Vector2 move;
        if (stickOnly)
        {
            move = new Vector2(_input.Stick.x, _input.Stick.y) * moveSpeed;
        }
        else
        {
            move = new Vector2(
                !_input.Stick.x.Equals(0) ? _input.Stick.x : _input.RightHeld ? 1 : _input.LeftHeld ? -1 : 0,
                !_input.Stick.y.Equals(0) ? _input.Stick.y : _input.UpHeld ? 1 : _input.DownHeld ? -1 : 0
            ) * moveSpeed;
        }

        if (_hasMoveBounds)
        {
            Vector2 targetPosition = _rb.position + move * deltaTime;
            targetPosition = ClampToMoveBounds(targetPosition);
            _rb.MovePosition(targetPosition);
            return move;
        }

        if (_rb.bodyType == RigidbodyType2D.Kinematic)
            _rb.MovePosition(_rb.position + move * deltaTime);
        else
            _rb.linearVelocity = move;

        return move;
    }

    private Vector2 ClampToMoveBounds(Vector2 position)
    {
        GetColliderOffsets(out Vector2 minOffset, out Vector2 maxOffset);
        float minX = _moveBounds.min.x - minOffset.x;
        float maxX = _moveBounds.max.x - maxOffset.x;
        float minY = _moveBounds.min.y - minOffset.y;
        float maxY = _moveBounds.max.y - maxOffset.y;

        if (minX > maxX)
            position.x = _moveBounds.center.x;
        else
            position.x = Mathf.Clamp(position.x, minX, maxX);

        if (minY > maxY)
            position.y = _moveBounds.center.y;
        else
            position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private void GetColliderOffsets(out Vector2 minOffset, out Vector2 maxOffset)
    {
        if (_collider == null)
            _collider = GetComponent<Collider2D>();

        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        if (_collider == null || _rb == null)
        {
            minOffset = Vector2.zero;
            maxOffset = Vector2.zero;
            return;
        }

        Vector2 origin = _rb.position;
        minOffset = (Vector2)_collider.bounds.min - origin;
        maxOffset = (Vector2)_collider.bounds.max - origin;
    }
}
