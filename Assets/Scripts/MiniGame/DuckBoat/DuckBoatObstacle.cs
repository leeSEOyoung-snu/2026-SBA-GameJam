using UnityEngine;

// 위에서 아래로 내려오는 장애물
[RequireComponent(typeof(Rigidbody2D))]
public class DuckBoatObstacle : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float destroyY  = -8f;  // 이 Y 이하로 나가면 제거

    private DuckBoat _owner;
    private Rigidbody2D _rb;
    private bool _hit;

    public void Init(DuckBoat owner)
    {
        _owner = owner;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = Vector2.down * moveSpeed;

        if (transform.position.y <= destroyY)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_hit) return;
        if (!col.TryGetComponent<DuckBoat>(out var boat)) return;
        if (boat != _owner) return;

        _hit = true;
        boat.TakeHit();
        Destroy(gameObject);
    }
}
