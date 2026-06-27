using UnityEngine;

// 플레이어를 향해 날아가는 미사일
[RequireComponent(typeof(Rigidbody2D))]
public class HachiFlyMissile : MonoBehaviour
{
    [SerializeField] private float speed        = 6f;
    [SerializeField] private float trackingTime = 1.5f;  // 이 시간 동안만 추적, 이후 직선
    [SerializeField] private float turnSpeed    = 120f;   // 추적 회전 속도 (도/초)
    [SerializeField] private float lifeTime     = 8f;

    private Transform    _player;
    private HachiFlyGame _game;
    private Rigidbody2D  _rb;
    private float        _elapsed;
    private bool         _hit;

    public void Init(Transform player, HachiFlyGame game)
    {
        _player = player;
        _game   = game;

        // 처음엔 플레이어 방향으로 발사
        if (_player != null)
        {
            var dir = ((Vector2)(_player.position - transform.position)).normalized;
            transform.up = dir;
        }

        Destroy(gameObject, lifeTime);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    private void FixedUpdate()
    {
        _elapsed += Time.fixedDeltaTime;

        // 추적 시간 동안 플레이어 방향으로 회전
        if (_elapsed < trackingTime && _player != null)
        {
            var targetDir = ((Vector2)(_player.position - transform.position)).normalized;
            float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg - 90f;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, turnSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        _rb.linearVelocity = transform.up * speed;
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
