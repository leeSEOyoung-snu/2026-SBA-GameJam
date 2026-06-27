using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BollyBall : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float maxSpeed    = 16f;

    [SerializeField] private float rightGoalX =  9f;
    [SerializeField] private float leftGoalX  = -9f;

    public event Action OnHachiScored;
    public event Action OnThreeScored;

    private Rigidbody2D _rb;
    public bool Scored { get; private set; }
    private bool _started;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Freeze();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!_started && col.gameObject.TryGetComponent<BollyBallCharacterController>(out var ctrl) && ctrl.IsHachi)
        {
            _started = true;
            Unfreeze();
            LaunchToward(right: true);
            Debug.Log("[BollyBall] 해치가 공을 침 → 게임 시작!");
        }
    }

    private void FixedUpdate()
    {
        if (!_started || Scored) return;

        if (_rb.linearVelocity.magnitude > maxSpeed)
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;

        float x = transform.position.x;
        if (x >= rightGoalX)
        {
            Scored = true;
            Debug.Log("[BollyBall] 오른쪽 골 → 해치 득점!");
            OnHachiScored?.Invoke();
        }
        else if (x <= leftGoalX)
        {
            Scored = true;
            Debug.Log("[BollyBall] 왼쪽 골 → 3명 득점!");
            OnThreeScored?.Invoke();
        }
    }

    // 외부(HechiBollyBallGame)에서 리셋 위치로 이동 후 호출
    public void ResetToPosition(Vector3 pos)
    {
        transform.position = pos;
        Scored = false;
        _started = false;
        Freeze();
    }

    private void LaunchToward(bool right)
    {
        float angle = UnityEngine.Random.Range(15f, 75f);
        if (UnityEngine.Random.value < 0.5f) angle = -angle;
        float radians = angle * Mathf.Deg2Rad;
        var dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        dir.x = Mathf.Abs(dir.x) * (right ? 1f : -1f);
        _rb.linearVelocity = dir * launchSpeed;
    }

    private void Freeze()
    {
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        _rb.linearVelocity = Vector2.zero;
    }

    private void Unfreeze()
    {
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
