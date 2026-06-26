using UnityEngine;

public class TopViewPhysics : MiniGamePhysicsBase
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool stickOnly;

    public override void UpdatePhysics(float deltaTime)
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

        if (_rb.bodyType == RigidbodyType2D.Kinematic)
            _rb.MovePosition(_rb.position + move * deltaTime);
        else
            _rb.linearVelocity = move;
    }
}
