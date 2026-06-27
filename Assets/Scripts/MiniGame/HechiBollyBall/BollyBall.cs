using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BollyBall : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float maxSpeed    = 16f;
    [SerializeField] private float resetDelay  = 1f;

    [SerializeField] private float rightGoalX =  9f;
    [SerializeField] private float leftGoalX  = -9f;

    public event Action OnHachiScored;
    public event Action OnThreeScored;

    private Rigidbody2D _rb;
    private bool _scored;
    private bool _started;       // 해치가 공을 치기 전까지 false
    private Vector3 _resetPos;   // ResetBall 호출 시 돌아올 위치

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 처음엔 정지 상태 — 해치가 충돌하면 시작
        _rb.linearVelocity = Vector2.zero;
        _rb.isKinematic = true;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // 아직 시작 전이고 해치가 공을 건드리면 게임 시작
        if (!_started && col.gameObject.TryGetComponent<BollyBallCharacterController>(out var ctrl) && ctrl.IsHachi)
        {
            _started = true;
            _rb.isKinematic = false;
            // 충돌 방향 기준으로 오른쪽(플레이어 진영)으로 발사
            LaunchToward(right: true);
            Debug.Log("[BollyBall] 해치가 공을 침 → 게임 시작!");
        }
    }

    private void FixedUpdate()
    {
        if (!_started || _scored) return;

        if (_rb.linearVelocity.magnitude > maxSpeed)
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;

        float x = transform.position.x;
        if (x >= rightGoalX)
        {
            _scored = true;
            Debug.Log("[BollyBall] 오른쪽 골 → 해치 득점");
            OnHachiScored?.Invoke();
        }
        else if (x <= leftGoalX)
        {
            _scored = true;
            Debug.Log("[BollyBall] 왼쪽 골 → 3명 득점");
            OnThreeScored?.Invoke();
        }
    }

    // 득점 후 게임에서 호출
    public void ResetBall(Vector3 resetPos)
    {
        _resetPos = resetPos;
        StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.isKinematic = true;
        gameObject.SetActive(false);

        yield return new WaitForSeconds(resetDelay);

        transform.position = _resetPos;
        gameObject.SetActive(true);
        _scored = false;
        _started = false;   // 다시 해치가 쳐야 시작
        Debug.Log("[BollyBall] 공 리셋 — 해치가 다시 치면 시작");
    }

    // right=true: 오른쪽(플레이어 진영)으로, false: 왼쪽(해치 진영)으로
    private void LaunchToward(bool right)
    {
        float angle = UnityEngine.Random.Range(15f, 75f);
        if (UnityEngine.Random.value < 0.5f) angle = -angle;
        float radians = angle * Mathf.Deg2Rad;
        var dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        dir.x = Mathf.Abs(dir.x) * (right ? 1f : -1f);  // X 방향 강제
        _rb.linearVelocity = dir * launchSpeed;
    }
}
