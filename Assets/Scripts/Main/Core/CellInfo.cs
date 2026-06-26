using System.Collections.Generic;
using UnityEngine;

public class CellInfo : MonoBehaviour
{
    public CellType type;

    [HideInInspector] public List<CellInfo> nextCells = new();

    public int Index { get; internal set; }
}
