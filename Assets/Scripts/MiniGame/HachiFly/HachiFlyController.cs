using UnityEngine;

// 해치 V 본체 — 4명이 각자 팔/다리 하나씩 조종
// 스틱으로 추진 방향 설정, Right 버튼으로 불꽃 뿜으며 추진
[RequireComponent(typeof(Rigidbody2D))]
public class HachiFlyController : MonoBehaviour
{
    [Header("물리")]
    [SerializeField] private float thrustForce  = 8f;    // Right 버튼 1회당 추진력
    [SerializeField] private float gravity      = 3f;    // 중력 (아래로 당기는 힘)
    [SerializeField] private float maxSpeed     = 12f;   // 최대 속도
    [SerializeField] private float damping      = 1.5f;  // 속도 감쇠

    [Header("이펙트")]
    [SerializeField] private string thrustEffectId = "ThrustJet";  // EffectManager 이펙트 ID

    // 4개 팔다리 추진 위치 (시각적으로 이펙트 위치 조절)
    [SerializeField] private Transform[] limbTransforms = new Transform[4];

    private Rigidbody2D _rb;
    private IPlayerInputReader[] _inputs = new IPlayerInputReader[4];
    private bool[] _pendingThrust = new bool[4];

    public void Init()
    {
        for (int i = 0; i < 4; i++)
            _inputs[i] = GameManager.Instance.GetPlayerInputReader(i + 1);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        // 회전 허용 — 팔/다리 위치에서 힘을 가하면 자연스럽게 기울어짐
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (_inputs[i] != null && _inputs[i].RightHeld)
                _pendingThrust[i] = true;
        }
    }

    private void FixedUpdate()
    {
        var vel = _rb.linearVelocity;

        // 중력
        vel.y -= gravity * Time.fixedDeltaTime;

        // 중력 적용
        _rb.linearVelocity = vel;

        // 각 플레이어 추진 — 팔/다리 위치에서 힘을 가해 회전 발생
        for (int i = 0; i < 4; i++)
        {
            if (!_pendingThrust[i]) continue;
            _pendingThrust[i] = false;

            // 팔/다리 로컬 up 방향으로 추진 (뒤집혀있으면 아래로 힘이 나감)
            bool hasLimb = limbTransforms != null && i < limbTransforms.Length && limbTransforms[i] != null;
            var forcePos = hasLimb ? (Vector2)limbTransforms[i].position : _rb.position;
            var dir      = hasLimb ? (Vector2)limbTransforms[i].up : Vector2.up;
            _rb.AddForceAtPosition(dir * thrustForce, forcePos, ForceMode2D.Impulse);

            // 이펙트
            MiniGameManager.Instance.Effects.Spawn(thrustEffectId, forcePos);

            Debug.Log($"[HachiFly] Player{i + 1} 추진! 방향: {dir} 위치: {forcePos}");
        }

        // 선속도 상한 + 감쇠
        if (_rb.linearVelocity.magnitude > maxSpeed)
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
        _rb.linearVelocity *= (1f - damping * Time.fixedDeltaTime);

        // 각속도 상한 + 감쇠
        _rb.angularVelocity = Mathf.Clamp(_rb.angularVelocity, -maxSpeed * 30f, maxSpeed * 30f);
        _rb.angularVelocity *= (1f - damping * Time.fixedDeltaTime);
    }
}
