using UnityEngine;

public class MiniGameCharacterController : MonoBehaviour
{
    public int PlayerId { get; private set; }
    private MiniGamePhysicsBase _physics;
    
    private void Awake()
    {
        enabled = false; // Init 호출 전까지 Update 차단
        _physics = GetComponent<MiniGamePhysicsBase>();
    }
    
    public void Init(int playerId)
    {
        PlayerId      = playerId;
        enabled = true;
        
        if (_physics != null)
            _physics.Init(playerId);
    }
    
    private void FixedUpdate()
    {
        _physics?.UpdatePhysics(Time.fixedDeltaTime);
    }
}
