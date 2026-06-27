using UnityEngine;

// 뱀서 스타일 — 플레이어를 서서히 추적하는 적
[RequireComponent(typeof(Rigidbody2D))]
public class HachiFlyChaser : MonoBehaviour
{
    [SerializeField] private float moveSpeed     = 3f;
    [SerializeField] private float acceleration  = 2f;   // 점점 빨라짐
    [SerializeField] private float maxSpeed      = 7f;
    [SerializeField] private float destroyMargin = 2f;

    private Transform    _player;
    private HachiFlyGame _game;
    private Rigidbody2D  _rb;
    private float        _currentSpeed;
    private bool         _hit;
    private bool         _enteredCamera;

    public void Init(Transform player)
    {
        _player = player;
        _game   = Object.FindAnyObjectByType<HachiFlyGame>();
        _currentSpeed = moveSpeed;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (_player == null) return;

        bool outside = HachiFlyUtils.IsOutsideCamera(transform.position, destroyMargin);
        if (!outside) _enteredCamera = true;
        if (_enteredCamera && outside) { Destroy(gameObject); return; }

        // 플레이어 방향으로 점점 빠르게 추적
        _currentSpeed = Mathf.Min(_currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);
        var dir = ((Vector2)(_player.position - transform.position)).normalized;
        _rb.linearVelocity = dir * _currentSpeed;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_hit) return;
        if (!col.TryGetComponent<HachiFlyController>(out _)) return;
        _hit = true;
        _game?.TakeHit();
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_hit) return;
        if (!col.gameObject.TryGetComponent<HachiFlyController>(out _)) return;
        _hit = true;
        _game?.TakeHit();
        Destroy(gameObject);
    }
}
