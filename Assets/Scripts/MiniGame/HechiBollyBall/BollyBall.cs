using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BollyBall : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 12f;
    [SerializeField] private float maxSpeed    = 24f;
    [SerializeField] private float minSpeed    = 7f;

    [Header("튕김감")]
    [SerializeField] private float hachiBounceBoost     = 8f;
    [SerializeField] private float characterBounceBoost = 3.5f;
    [SerializeField] private float playerHitMaxSpeed    = 16f;
    [SerializeField] private float wallBounceBoost      = 1f;
    [SerializeField] private float spinDegreesPerVelocity = 90f;

    [SerializeField] private float rightGoalX =  9f;
    [SerializeField] private float leftGoalX  = -9f;

    public event Action OnHachiScored;
    public event Action OnThreeScored;

    private Rigidbody2D _rb;
    public bool Scored { get; private set; }
    private bool _started;
    private bool _hachiServes = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Freeze();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        bool hitCharacter = col.gameObject.TryGetComponent<BollyBallCharacterController>(out var ctrl);
        if (!_started && hitCharacter && ctrl.IsHachi == _hachiServes)
        {
            _started = true;
            Unfreeze();
            SpawnHachiHitEffect(col);
            LaunchFromServer(col);
            Debug.Log($"[BollyBall] {(ctrl.IsHachi ? "해치" : "3인팀")}가 공을 침 → 게임 시작!");
            return;
        }

        if (_started && !Scored)
            ApplyBounceFeel(col, ctrl);
    }

    private void FixedUpdate()
    {
        if (!_started || Scored) return;

        ClampSpeed();
        ApplySpin();

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

    // 다음 서브권 설정 (true = 해치, false = 3인팀)
    public void SetServer(bool hachiServes)
    {
        _hachiServes = hachiServes;
    }

    private void ApplyBounceFeel(Collision2D col, BollyBallCharacterController character)
    {
        bool hitCharacter = character != null;
        bool hitHachi = hitCharacter && character.IsHachi;

        Vector2 dir = _rb.linearVelocity.sqrMagnitude > 0.01f
            ? _rb.linearVelocity.normalized
            : Vector2.right;

        if (hitCharacter)
        {
            Vector2 awayFromCharacter = (transform.position - col.transform.position).normalized;
            if (awayFromCharacter.sqrMagnitude > 0.01f)
                dir = Vector2.Lerp(dir, awayFromCharacter, 0.55f).normalized;

            if (hitHachi)
                SpawnHachiHitEffect(col);
        }
        else if (col.contactCount > 0)
        {
            Vector2 bounceNormal = col.GetContact(0).normal;
            Vector2 blendedDir = Vector2.Lerp(dir, bounceNormal, 0.25f);
            dir = blendedDir.sqrMagnitude > 0.01f ? blendedDir.normalized : bounceNormal;
        }

        float boost = hitHachi ? hachiBounceBoost : hitCharacter ? characterBounceBoost : wallBounceBoost;
        float speedCap = hitHachi || !hitCharacter ? maxSpeed : playerHitMaxSpeed;
        float speed = Mathf.Clamp(_rb.linearVelocity.magnitude + boost, minSpeed, speedCap);
        _rb.linearVelocity = dir * speed;
        ApplySpin();
    }

    private void ClampSpeed()
    {
        float speed = _rb.linearVelocity.magnitude;
        if (speed > maxSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
        }
        else if (speed > 0.01f && speed < minSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * minSpeed;
        }
    }

    private void ApplySpin()
    {
        _rb.angularVelocity = -_rb.linearVelocity.x * spinDegreesPerVelocity;
    }

    private void LaunchFromServer(Collision2D col)
    {
        Vector2 dir = (transform.position - col.transform.position).normalized;
        if (dir.sqrMagnitude <= 0.01f)
        {
            float angle = UnityEngine.Random.Range(12f, 68f);
            if (UnityEngine.Random.value < 0.5f) angle = -angle;
            float radians = angle * Mathf.Deg2Rad;
            dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        // 해치 서브 → 오른쪽(상대방 쪽), 3인팀 서브 → 왼쪽(해치 쪽)
        dir.x = _hachiServes ? Mathf.Abs(dir.x) : -Mathf.Abs(dir.x);
        _rb.linearVelocity = dir.normalized * Mathf.Min(launchSpeed + hachiBounceBoost, maxSpeed);
        ApplySpin();
    }

    private void SpawnHachiHitEffect(Collision2D col)
    {
        Vector3 effectPosition = col.contactCount > 0
            ? col.GetContact(0).point
            : transform.position;

        MiniGameManager.Instance?.Effects?.Spawn("Boom", effectPosition);
    }

    private void Freeze()
    {
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
    }

    private void Unfreeze()
    {
        _rb.constraints = RigidbodyConstraints2D.None;
    }
}
