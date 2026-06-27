using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class EvolutionCellBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject proximityUI;
    [SerializeField] private int proximityRange = 5;

    private CellInfo _cell;

    private void Awake()
    {
        _cell = GetComponent<CellInfo>();
        SetProximityUIVisible(false);
    }

    // MainGameLoop에서 말이 한 칸 이동할 때마다 호출
    public void UpdateProximityUI(CellInfo from)
    {
        SetProximityUIVisible(IsWithinRange(from));
    }

    private bool IsWithinRange(CellInfo from)
    {
        // BFS로 from → _cell 방향 탐색, proximityRange 이내면 true
        if (from == _cell) return true;

        var visited = new System.Collections.Generic.HashSet<CellInfo>();
        var queue = new System.Collections.Generic.Queue<(CellInfo cell, int depth)>();
        queue.Enqueue((from, 0));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (depth > proximityRange) continue;
            if (!visited.Add(current)) continue;

            foreach (var next in current.nextCells)
            {
                if (next == _cell) return true;
                queue.Enqueue((next, depth + 1));
            }
        }

        return false;
    }

    private void SetProximityUIVisible(bool visible)
    {
        if (proximityUI != null)
            proximityUI.SetActive(visible);
    }
}
