using UnityEngine;

public class HorizontalScrollPhysics : MiniGamePhysicsBase
{
    [SerializeField] private float moveSpeed;
    
    public override void UpdatePhysics(float deltaTime)
    {
        var move = new Vector2(
            !_input.Stick.x.Equals(0) ? _input.Stick.x : _input.RightHeld ? 1 : _input.LeftHeld ? -1 : 0,
            0) * moveSpeed;

        if (_rb.bodyType == RigidbodyType2D.Kinematic)
            _rb.MovePosition(_rb.position + move * deltaTime);
        else
            _rb.linearVelocity = move;
    }
}
