using UnityEngine;

public class MiniGameCharacterController : MonoBehaviour
{
    public int PlayerId { get; private set; }
    private MiniGamePhysicsBase _physics;
    private PlayableCharacterVisual _visual;
    
    private void Awake()
    {
        enabled = false; // Init 호출 전까지 Update 차단
        _physics = GetComponent<MiniGamePhysicsBase>();
        _visual = GetComponentInChildren<PlayableCharacterVisual>(true);
    }
    
    public void Init(int playerId)
    {
        PlayerId      = playerId;
        enabled = true;
        
        if (_physics != null)
            _physics.Init(playerId);

        if (_visual != null)
            _visual.Init(playerId);
    }
    
    private void FixedUpdate()
    {
        _physics?.UpdatePhysics(Time.fixedDeltaTime);
    }
}
