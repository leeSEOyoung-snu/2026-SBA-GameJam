using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 3종 장애물을 주기적으로 스폰
// HachiFlyGame과 같은 오브젝트에 붙임
public class HachiFlyObstacleSpawner : MonoBehaviour
{
    [Header("장애물 프리팹")]
    [SerializeField] private GameObject largeObstaclePrefab;
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private GameObject chaserPrefab;

    [Header("스폰 설정")]
    [SerializeField] private float spawnRadius      = 12f;
    [SerializeField] private float minSpawnDistance = 4f;

    [Header("스폰 간격 (초)")]
    [SerializeField] private float largeInterval  = 6f;
    [SerializeField] private float turretInterval = 10f;
    [SerializeField] private float chaserInterval = 14f;

    [Header("최대 동시 장애물 수")]
    [SerializeField] private int maxLarge  = 3;
    [SerializeField] private int maxTurret = 2;
    [SerializeField] private int maxChaser = 2;

    [Header("전방 스폰 금지")]
    [Tooltip("진행 방향 기준 좌우 몇 도를 스폰 금지 구역으로 막을지")]
    [SerializeField] private float frontBlockAngle = 60f;
    [Tooltip("속도가 이 값 이상일 때만 전방 금지 적용 (느릴 땐 무시)")]
    [SerializeField] private float minVelocityForBlock = 1f;

    [Header("카메라 밖 스폰")]
    [Tooltip("카메라 뷰포트 경계에서 얼마나 더 밖에서 스폰할지 (월드 단위)")]
    [SerializeField] private float cameraEdgeMargin = 1f;

    private Camera _cam;

    private Transform _player;
    private Rigidbody2D _playerRb;
    private bool _running;

    private readonly List<GameObject> _largePool  = new();
    private readonly List<GameObject> _turretPool = new();
    private readonly List<GameObject> _chaserPool = new();

    public void Init(Transform player)
    {
        _player   = player;
        _playerRb = player.GetComponent<Rigidbody2D>();
        _cam      = Camera.main;
        _running  = true;
        StartCoroutine(SpawnRoutine(largeObstaclePrefab,  largeInterval,  _largePool,  maxLarge));
        StartCoroutine(SpawnRoutine(turretPrefab,         turretInterval, _turretPool, maxTurret));
        StartCoroutine(SpawnRoutine(chaserPrefab,         chaserInterval, _chaserPool, maxChaser));
    }

    private IEnumerator SpawnRoutine(GameObject prefab, float interval, List<GameObject> pool, int max)
    {
        yield return new WaitForSeconds(interval * 0.5f);

        while (_running)
        {
            pool.RemoveAll(o => o == null); // 파괴된 항목 정리
            if (pool.Count < max)
                SpawnAround(prefab, pool);
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnAround(GameObject prefab, List<GameObject> pool)
    {
        if (prefab == null || _player == null) return;

        Vector2 moveDir = _playerRb != null ? _playerRb.linearVelocity : Vector2.zero;
        bool blockFront = moveDir.magnitude >= minVelocityForBlock;
        float moveDirAngle = blockFront ? Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg : 0f;

        // 카메라 뷰포트 반범위 계산 (orthographic 기준)
        float camHalfH = 0f, camHalfW = 0f;
        if (_cam != null && _cam.orthographic)
        {
            camHalfH = _cam.orthographicSize + cameraEdgeMargin;
            camHalfW = camHalfH * _cam.aspect + cameraEdgeMargin;
        }

        Vector2 pos = (Vector2)_player.position + Vector2.right * minSpawnDistance; // fallback
        for (int attempts = 0; attempts < 30; attempts++)
        {
            float spawnAngle = Random.Range(0f, 360f);

            // 진행 방향 전방 구역이면 건너뜀
            if (blockFront && Mathf.Abs(Mathf.DeltaAngle(spawnAngle, moveDirAngle)) < frontBlockAngle)
                continue;

            float rad  = spawnAngle * Mathf.Deg2Rad;
            float dist = Random.Range(minSpawnDistance, spawnRadius);
            Vector2 candidate = (Vector2)_player.position + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * dist;

            // 카메라 뷰포트 안쪽이면 건너뜀
            if (_cam != null && camHalfH > 0f)
            {
                Vector2 camCenter = (Vector2)_cam.transform.position;
                Vector2 localPos  = candidate - camCenter;
                if (Mathf.Abs(localPos.x) < camHalfW && Mathf.Abs(localPos.y) < camHalfH)
                    continue;
            }

            pos = candidate;
            break;
        }

        var obj = Instantiate(prefab, pos, Quaternion.identity);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, gameObject.scene);
        pool.Add(obj);

        if (obj.TryGetComponent<HachiFlyTurret>(out var turret))     turret.Init(_player);
        if (obj.TryGetComponent<HachiFlyChaser>(out var chaser))     chaser.Init(_player);
        if (obj.TryGetComponent<HachiFlyObstacle>(out var obstacle)) obstacle.Init();
    }
}
