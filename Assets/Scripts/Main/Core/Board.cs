using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private Transform cellParent;
    [SerializeField] private List<Sprite> cellSprites = new();

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
            {
                ApplyRandomCellSprite(cell);
                _cells.Add(cell);
            }
        }

        for (int i = 0; i < _cells.Count; i++)
            _cells[i].Index = i;

        // 시작점: 가장 우하단 셀
        StartCell = _cells[FindBottomRightIndex()];

        // 시작점부터 가장 가까운 칸을 따라 보드 루프 연결
        BoardPathBuilder.Build(_cells, StartCell);

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

    private void ApplyRandomCellSprite(CellInfo cell)
    {
        if (cellSprites.Count == 0)
            return;

        if (cell.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            spriteRenderer.sprite = cellSprites[Random.Range(0, cellSprites.Count)];
    }
}
