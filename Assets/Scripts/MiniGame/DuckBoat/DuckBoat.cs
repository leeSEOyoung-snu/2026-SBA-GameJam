using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 보트 1대 — 위로 자동 전진, 좌우로 피하기
// PlayerIds[0] Right → 왼쪽
// PlayerIds[1] Right → 오른쪽
[RequireComponent(typeof(Rigidbody2D))]
public class DuckBoat : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float rowForwardForce = 5f;   // 노 젓기 → 앞(Y+) 힘
    [SerializeField] private float rowSteerForce   = 3f;   // 노 젓기 → 좌우(X) 힘
    [SerializeField] private float currentForce    = 2f;   // 강물이 뒤로 미는 힘 (Y-)
    [SerializeField] private float damping         = 2.5f; // 속도 감쇠

    [Header("장애물 스폰")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private float spawnInterval    = 1.5f;
    [SerializeField] private float spawnIntervalMin = 0.5f;
    [SerializeField] private float spawnAccel       = 0.05f;
    [SerializeField] private float obstacleSpawnYOffset = 12f;  // 보트 위쪽으로 얼마나 떨어진 곳에 스폰

    [Header("HP")]
    [SerializeField] private int maxHp = 5;

    public event Action OnBoatDead;

    public bool  IsDead   { get; private set; }
    public float Distance { get; private set; }

    private Rigidbody2D _rb;
    private IPlayerInputReader _inputLeft;   // Right 누르면 왼쪽으로
    private IPlayerInputReader _inputRight;  // Right 누르면 오른쪽으로
    private float _laneXMin;
    private float _laneXMax;
    private int   _hp;
    private float _currentSpawnInterval;

    // Update에서 감지한 입력을 FixedUpdate로 전달
    private bool _pendingLeft;
    private bool _pendingRight;

    public void Init(List<int> playerIds, float laneXMin, float laneXMax)
    {
        _inputLeft  = GameManager.Instance.GetPlayerInputReader(playerIds[0]);
        _inputRight = GameManager.Instance.GetPlayerInputReader(playerIds[1]);
        _laneXMin   = laneXMin;
        _laneXMax   = laneXMax;
        _hp         = maxHp;
        _currentSpawnInterval = spawnInterval;

        StartCoroutine(ObstacleSpawnRoutine());
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        // 순간 입력은 Update에서 감지 (FixedUpdate에서 놓치지 않도록)
        if (_inputLeft  != null && _inputLeft.Right)  _pendingLeft  = true;
        if (_inputRight != null && _inputRight.Right) _pendingRight = true;
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        var vel = _rb.linearVelocity;

        // 강물: 계속 뒤로(Y-) 밀기
        vel.y -= currentForce * Time.fixedDeltaTime;

        // 왼쪽 노 (Player0): 앞으로 + 오른쪽(+X)
        if (_pendingLeft)
        {
            vel.y += rowForwardForce;
            vel.x += rowSteerForce;
            _pendingLeft = false;
        }

        // 오른쪽 노 (Player1): 앞으로 + 왼쪽(-X)
        if (_pendingRight)
        {
            vel.y += rowForwardForce;
            vel.x -= rowSteerForce;
            _pendingRight = false;
        }

        // 감쇠
        vel *= (1f - damping * Time.fixedDeltaTime);

        _rb.linearVelocity = vel;

        // 거리 누적 (앞으로 간 만큼만)
        if (vel.y > 0) Distance += vel.y * Time.fixedDeltaTime;

        // 레인 X 클램프
        var pos = _rb.position;
        pos.x = Mathf.Clamp(pos.x, _laneXMin, _laneXMax);
        _rb.position = pos;
    }

    public void TakeHit()
    {
        if (IsDead) return;
        _hp--;
        Debug.Log($"[DuckBoat] HP: {_hp}/{maxHp}");

        if (_hp <= 0)
        {
            IsDead = true;
            Debug.Log("[DuckBoat] 보트 격침!");
            OnBoatDead?.Invoke();
        }
    }

    private IEnumerator ObstacleSpawnRoutine()
    {
        while (!IsDead)
        {
            yield return new WaitForSeconds(_currentSpawnInterval);
            if (IsDead) break;

            SpawnObstacle();
            _currentSpawnInterval = Mathf.Max(spawnIntervalMin, _currentSpawnInterval - spawnAccel);
        }
    }

    private void SpawnObstacle()
    {
        if (obstaclePrefab == null) return;

        // 레인 X 범위 안 랜덤 위치, 보트 위쪽에 스폰
        float spawnX = UnityEngine.Random.Range(_laneXMin, _laneXMax);
        var pos = new Vector3(spawnX, transform.position.y + obstacleSpawnYOffset, 0f);
        var obj = Instantiate(obstaclePrefab, pos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        obj.GetComponent<DuckBoatObstacle>().Init(this);
    }
}
