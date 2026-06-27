using UnityEngine;

// 크고 고정된 장애물 — 충돌 시 데미지
[RequireComponent(typeof(Rigidbody2D))]
public class HachiFlyObstacle : MonoBehaviour
{
    [SerializeField] private float destroyDistance = 40f;

    private HachiFlyGame _game;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Init()
    {
        _game = Object.FindAnyObjectByType<HachiFlyGame>();
    }

    private void Update()
    {
        if (_game == null) return;
        if (Vector2.Distance(transform.position, _game.HachiTransform.position) > destroyDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<HachiFlyController>(out _))
            _game?.TakeHit();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.TryGetComponent<HachiFlyController>(out _))
            _game?.TakeHit();
    }
}
