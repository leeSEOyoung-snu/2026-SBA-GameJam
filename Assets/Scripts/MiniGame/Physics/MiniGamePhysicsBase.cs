using UnityEngine;

public abstract class MiniGamePhysicsBase : MonoBehaviour
{
    protected IPlayerInputReader _input;
    protected Rigidbody2D _rb;

    public void Init(int playerId)
    {
        _input  = GameManager.Instance.GetPlayerInputReader(playerId);
        _rb  = GetComponent<Rigidbody2D>();
    }
    
    public abstract Vector2 UpdatePhysics(float deltaTime);
}
