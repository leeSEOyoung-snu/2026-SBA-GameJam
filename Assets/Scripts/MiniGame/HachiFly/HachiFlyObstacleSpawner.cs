using System.Collections;
using UnityEngine;

// 3종 장애물을 주기적으로 스폰
// HachiFlyGame과 같은 오브젝트에 붙임
public class HachiFlyObstacleSpawner : MonoBehaviour
{
    [Header("장애물 프리팹")]
    [SerializeField] private GameObject largeObstaclePrefab;   // 크고 고정
    [SerializeField] private GameObject turretPrefab;          // 고정 + 미사일
    [SerializeField] private GameObject chaserPrefab;          // 추적 적

    [Header("스폰 설정")]
    [SerializeField] private float spawnRadius      = 12f;  // 플레이어 기준 스폰 반경
    [SerializeField] private float minSpawnDistance = 4f;   // 너무 가까이 스폰 방지

    [Header("스폰 간격")]
    [SerializeField] private float largeInterval  = 4f;
    [SerializeField] private float turretInterval = 6f;
    [SerializeField] private float chaserInterval = 8f;

    private Transform _player;
    private bool _running;

    public void Init(Transform player)
    {
        _player  = player;
        _running = true;
        StartCoroutine(SpawnRoutine(largeObstaclePrefab,  largeInterval));
        StartCoroutine(SpawnRoutine(turretPrefab,         turretInterval));
        StartCoroutine(SpawnRoutine(chaserPrefab,         chaserInterval));
    }

    private IEnumerator SpawnRoutine(GameObject prefab, float interval)
    {
        yield return new WaitForSeconds(interval * 0.5f); // 첫 스폰 약간 딜레이

        while (_running)
        {
            SpawnAround(prefab);
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnAround(GameObject prefab)
    {
        if (prefab == null || _player == null) return;

        // 플레이어 주변 랜덤 위치 (너무 가깝지 않게)
        Vector2 pos;
        int attempts = 0;
        do
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist  = Random.Range(minSpawnDistance, spawnRadius);
            pos = (Vector2)_player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
            attempts++;
        } while (attempts < 20 && Vector2.Distance(pos, _player.position) < minSpawnDistance);

        var obj = Instantiate(prefab, pos, Quaternion.identity);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, gameObject.scene);

        // 장애물에 플레이어 참조 전달
        if (obj.TryGetComponent<HachiFlyTurret>(out var turret))   turret.Init(_player);
        if (obj.TryGetComponent<HachiFlyChaser>(out var chaser))   chaser.Init(_player);
        if (obj.TryGetComponent<HachiFlyObstacle>(out var obstacle)) obstacle.Init();
    }
}
