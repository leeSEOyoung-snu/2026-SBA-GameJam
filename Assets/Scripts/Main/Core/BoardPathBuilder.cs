using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BoardPathBuilder
{
    private static float _minX, _maxX, _minY, _maxY, _eps;

    public static void Build(List<CellInfo> cells)
    {
        if (cells.Count < 2) return;

        _minX = cells.Min(c => c.transform.position.x);
        _maxX = cells.Max(c => c.transform.position.x);
        _minY = cells.Min(c => c.transform.position.y);
        _maxY = cells.Max(c => c.transform.position.y);
        _eps  = Mathf.Min(_maxX - _minX, _maxY - _minY) * 0.15f;

        foreach (var cell in cells)
            cell.nextCells.Clear();

        foreach (var cell in cells)
            cell.nextCells = ComputeNext(cell, cells);

        LogPaths(cells);
    }

    static bool OnRight(Vector3 p)  => Mathf.Abs(p.x - _maxX) < _eps;
    static bool OnLeft(Vector3 p)   => Mathf.Abs(p.x - _minX) < _eps;
    static bool OnTop(Vector3 p)    => Mathf.Abs(p.y - _maxY) < _eps;
    static bool OnBottom(Vector3 p) => Mathf.Abs(p.y - _minY) < _eps;

    static List<CellInfo> ComputeNext(CellInfo cell, List<CellInfo> all)
    {
        var p = cell.transform.position;
        var result = new List<CellInfo>();

        bool onRight  = OnRight(p);
        bool onLeft   = OnLeft(p);
        bool onTop    = OnTop(p);
        bool onBottom = OnBottom(p);

        if      (onRight && onBottom) Add(result, Closest(p, all, c => OnRight(c.transform.position)  && c.transform.position.y > p.y)); // BR → 위
        else if (onRight && onTop)    Add(result, Closest(p, all, c => OnTop(c.transform.position)    && c.transform.position.x < p.x)); // TR → 왼쪽
        else if (onLeft  && onTop)    Add(result, Closest(p, all, c => OnLeft(c.transform.position)   && c.transform.position.y < p.y)); // TL → 아래
        else if (onLeft  && onBottom) Add(result, Closest(p, all, c => OnBottom(c.transform.position) && c.transform.position.x > p.x)); // BL → 오른쪽
        else if (onRight)             Add(result, Closest(p, all, c => OnRight(c.transform.position)  && c.transform.position.y > p.y)); // R열 → 위
        else if (onTop)               Add(result, Closest(p, all, c => OnTop(c.transform.position)    && c.transform.position.x < p.x)); // T행 → 왼쪽
        else if (onLeft)              Add(result, Closest(p, all, c => OnLeft(c.transform.position)   && c.transform.position.y < p.y)); // L열 → 아래
        else if (onBottom)            Add(result, Closest(p, all, c => OnBottom(c.transform.position) && c.transform.position.x > p.x)); // B행 → 오른쪽

        return result;
    }

    static CellInfo Closest(Vector3 from, List<CellInfo> all, Func<CellInfo, bool> filter)
    {
        CellInfo best = null;
        float bestDist = float.MaxValue;

        foreach (var c in all)
        {
            if (!filter(c)) continue;
            float dist = Vector3.Distance(from, c.transform.position);
            if (dist < 0.01f) continue;
            if (dist < bestDist) { bestDist = dist; best = c; }
        }

        return best;
    }

    static void Add(List<CellInfo> list, CellInfo cell)
    {
        if (cell != null && !list.Contains(cell))
            list.Add(cell);
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
