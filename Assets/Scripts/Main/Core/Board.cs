using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private Transform cellParent;

    private readonly List<CellInfo> _cells = new();

    public CellInfo CurrentCell { get; private set; }
    public CellInfo StartCell { get; private set; }
    public int CellCount => _cells.Count;

    private void Awake()
    {
        InitBoard();
    }

    private void InitBoard()
    {
        _cells.Clear();

        foreach (Transform child in cellParent)
        {
            if (child.TryGetComponent<CellInfo>(out var cell))
                _cells.Add(cell);
        }

        for (int i = 0; i < _cells.Count; i++)
            _cells[i].Index = i;

        // 월드 좌표 기반 다음 칸 자동 연결 (윷놀이 규칙 적용)
        BoardPathBuilder.Build(_cells);

        // 시작점: 가장 우하단 셀
        StartCell = _cells[FindBottomRightIndex()];
        CurrentCell = StartCell;

        Debug.Log($"[Board] 총 {_cells.Count}개 칸 초기화 / 시작: {CurrentCell.name}({CurrentCell.Index})");
    }

    private int FindBottomRightIndex()
    {
        int best = 0;
        for (int i = 1; i < _cells.Count; i++)
        {
            Vector3 a = _cells[i].transform.position;
            Vector3 b = _cells[best].transform.position;
            if (a.x > b.x || (Mathf.Approximately(a.x, b.x) && a.y < b.y))
                best = i;
        }
        return best;
    }

    public void SetCurrentCell(CellInfo cell) => CurrentCell = cell;
}
