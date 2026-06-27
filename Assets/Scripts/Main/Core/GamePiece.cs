using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float followDelay = 0.15f;
    [SerializeField] private float arrivalOffsetRadius = 0.25f;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private GameObject[] playersByIdOrder;  // 인덱스 0 = 플레이어 ID 1

    [SerializeField] private Color lowestRankColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private void Start()
    {
        int[] defaultRanking = { 1, 2, 3, 4 };
        ApplyRanking(playersByIdOrder[0].transform.position, defaultRanking);
    }

    public void PlaceAt(Vector3 worldPosition)
    {
        foreach (var player in playersByIdOrder)
            if (player != null) player.transform.position = worldPosition;
    }

    // 순위 변경 시 외부에서 호출 (sorting order, alpha, offset 갱신)
    public void ApplyRanking(Vector3 centerPosition, int[] playerIdByRanking)
    {
        UpdateVisuals(playerIdByRanking);
        ApplyArrivalOffsets(centerPosition, playerIdByRanking);
    }

    // waypoints: 거쳐가는 모든 칸의 위치 배열
    public IEnumerator MoveTo(Vector3[] waypoints, int[] playerIdByRanking)
    {
        Vector3 finalDestination = waypoints[^1];

        // 카메라 타겟 & 비주얼 업데이트
        UpdateVisuals(playerIdByRanking);

        // 각 플레이어의 최종 목적지에 랜덤 offset 적용 (옹기종기 모이도록)
        Vector3[] destinations = BuildDestinationsWithOffset(finalDestination, playerIdByRanking);

        // 각 플레이어를 딜레이 걸어 waypoints를 따라 연속 이동
        for (int rank = 0; rank < playerIdByRanking.Length; rank++)
        {
            GameObject player = playersByIdOrder[playerIdByRanking[rank] - 1];
            if (player != null)
                StartCoroutine(MovePlayerAlongPath(player, waypoints, destinations[rank], rank * followDelay));
        }

        // 마지막 플레이어가 도착할 때까지 대기
        GameObject lastPlayer = playersByIdOrder[playerIdByRanking[^1] - 1];
        float totalPathLength = CalculatePathLength(lastPlayer.transform.position, waypoints);
        float totalDuration = (playerIdByRanking.Length - 1) * followDelay + totalPathLength / moveSpeed;
        yield return new WaitForSeconds(totalDuration);
    }

    private Vector3[] BuildDestinationsWithOffset(Vector3 center, int[] playerIdByRanking)
    {
        var destinations = new Vector3[playerIdByRanking.Length];
        for (int rank = 0; rank < playerIdByRanking.Length; rank++)
        {
            Vector2 offset = rank == 0 ? Vector2.zero : Random.insideUnitCircle * arrivalOffsetRadius;
            destinations[rank] = center + new Vector3(offset.x, offset.y, 0f);
        }
        return destinations;
    }

    private void ApplyArrivalOffsets(Vector3 center, int[] playerIdByRanking)
    {
        Vector3[] destinations = BuildDestinationsWithOffset(center, playerIdByRanking);
        for (int rank = 0; rank < playerIdByRanking.Length; rank++)
        {
            GameObject player = playersByIdOrder[playerIdByRanking[rank] - 1];
            if (player != null)
                player.transform.position = destinations[rank];
        }
    }

    private void UpdateVisuals(int[] playerIdByRanking)
    {
        GameObject leader = playersByIdOrder[playerIdByRanking[0] - 1];
        if (cinemachineCamera != null && leader != null)
            cinemachineCamera.Follow = leader.transform;

        for (int rank = 0; rank < playerIdByRanking.Length; rank++)
        {
            GameObject player = playersByIdOrder[playerIdByRanking[rank] - 1];
            if (player == null) continue;

            if (player.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.sortingOrder = playerIdByRanking.Length - rank;  // 1등=4, 2등=3, ...
                float t = rank / (float)(playerIdByRanking.Length - 1);
                sr.color = Color.Lerp(Color.white, lowestRankColor, t);
            }
        }
    }

    private IEnumerator MovePlayerAlongPath(GameObject player, Vector3[] waypoints, Vector3 finalDestination, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // waypoints 순서대로 연속 이동
        foreach (Vector3 waypoint in waypoints)
        {
            Vector3 start = player.transform.position;
            float duration = Vector3.Distance(start, waypoint) / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                player.transform.position = Vector3.Lerp(start, waypoint, elapsed / duration);
                yield return null;
            }
        }

        // 마지막 칸에서 offset 위치로 이동
        Vector3 offsetStart = player.transform.position;
        float offsetDuration = Vector3.Distance(offsetStart, finalDestination) / moveSpeed;
        float offsetElapsed = 0f;

        while (offsetElapsed < offsetDuration)
        {
            offsetElapsed += Time.deltaTime;
            player.transform.position = Vector3.Lerp(offsetStart, finalDestination, offsetElapsed / offsetDuration);
            yield return null;
        }

        player.transform.position = finalDestination;
    }

    private float CalculatePathLength(Vector3 startPos, Vector3[] waypoints)
    {
        float length = 0f;
        Vector3 prev = startPos;
        foreach (Vector3 wp in waypoints)
        {
            length += Vector3.Distance(prev, wp);
            prev = wp;
        }
        return length;
    }
}
