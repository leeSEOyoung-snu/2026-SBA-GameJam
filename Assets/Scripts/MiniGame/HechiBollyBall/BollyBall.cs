using System;
using System.Collections;
using UnityEngine;

// 공 물리 + 골 감지 + 리셋
[RequireComponent(typeof(Rigidbody2D))]
public class BollyBall : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float maxSpeed = 16f;
    [SerializeField] private float resetDelay = 1f;

    // 골 라인 X좌표 (인스펙터에서 씬에 맞게 설정)
    [SerializeField] private float rightGoalX =  9f;   // 오른쪽 벽 통과 → 해치 득점
    [SerializeField] private float leftGoalX  = -9f;   // 왼쪽 벽 통과  → 3명 득점

    public event Action OnHachiScored;  // 공이 오른쪽 골라인 통과 → 해치 득점
    public event Action OnThreeScored;  // 공이 왼쪽 골라인 통과  → 3명 득점

    private Rigidbody2D _rb;
    private bool _scored;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        Launch();
    }

    private void FixedUpdate()
    {
        if (_scored) return;

        // 속도 상한 (튕기다 보면 가속될 수 있어서)
        if (_rb.linearVelocity.magnitude > maxSpeed)
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;

        // 골 라인 감지
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

    // 게임에서 호출 — 딜레이 후 공 중앙 리셋
    public void ResetBall()
    {
        StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        gameObject.SetActive(false);

        yield return new WaitForSeconds(resetDelay);

        transform.position = Vector3.zero;
        gameObject.SetActive(true);
        _scored = false;
        Launch();
    }

    private void Launch()
    {
        // 완전 수직/수평을 피해 15~75° 사이 랜덤 각도로 발사
        float angle = UnityEngine.Random.Range(15f, 75f);
        // 위 또는 아래, 좌 또는 우 랜덤
        if (UnityEngine.Random.value < 0.5f) angle = -angle;
        float radians = angle * Mathf.Deg2Rad;
        var dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        if (UnityEngine.Random.value < 0.5f) dir.x = -dir.x;

        _rb.linearVelocity = dir * launchSpeed;
    }
}
