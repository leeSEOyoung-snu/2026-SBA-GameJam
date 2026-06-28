using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BoardPathBuilder
{
    public static void Build(List<CellInfo> cells, CellInfo startCell)
    {
        if (cells.Count < 2) return;
        if (startCell == null) startCell = cells[0];

        foreach (var cell in cells)
            cell.nextCells.Clear();

        List<CellInfo> path = BuildNearestPath(cells, startCell);

        for (int i = 0; i < path.Count; i++)
            path[i].nextCells.Add(path[(i + 1) % path.Count]);

        LogPaths(cells);
    }

    private static List<CellInfo> BuildNearestPath(List<CellInfo> cells, CellInfo startCell)
    {
        var path = new List<CellInfo> { startCell };
        var unvisited = new List<CellInfo>(cells);
        unvisited.Remove(startCell);

        while (unvisited.Count > 0)
        {
            CellInfo current = path[path.Count - 1];
            CellInfo nearest = FindNearest(current, unvisited);
            path.Add(nearest);
            unvisited.Remove(nearest);
        }

        return path;
    }

    private static CellInfo FindNearest(CellInfo from, List<CellInfo> candidates)
    {
        CellInfo nearest = null;
        float nearestSqrDistance = float.MaxValue;
        Vector3 fromPosition = from.transform.position;

        foreach (var candidate in candidates)
        {
            float sqrDistance = (candidate.transform.position - fromPosition).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance)
                continue;

            nearest = candidate;
            nearestSqrDistance = sqrDistance;
        }

        return nearest;
    }

    static void LogPaths(List<CellInfo> cells)
    {
        foreach (var cell in cells)
        {
            var nexts = string.Join(", ", cell.nextCells.Select(c => c.name));
            Debug.Log($"[BoardPath] {cell.name}({cell.Index}) → [{(nexts.Length > 0 ? nexts : "없음")}]");
        }
    }
}
