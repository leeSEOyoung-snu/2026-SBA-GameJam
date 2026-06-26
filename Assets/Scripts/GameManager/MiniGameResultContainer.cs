using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MiniGameResultContainer", menuName = "Scriptable Objects/MiniGameResultContainer")]
public class MiniGameResultContainer : ScriptableObject
{
    [SerializeField] private MiniGameTypes type;
    public MiniGameTypes Type => type;
    
    // Solo
    [Serializable]
    private struct SoloBattleResultByRanking
    {
        public int first;
        public int second;
        public int third;
        public int fourth;
    }
    [SerializeField] private SoloBattleResultByRanking soloDelta;
    
    // 1 vs 3
    [Serializable]
    private struct OneVsThreeResultByWinner
    {
        public int oneWin;
        public int threeWin;
    }
    [SerializeField] private OneVsThreeResultByWinner oneVsThreeDelta;
    
    // 2 vs 2
    [SerializeField] private int twoVsTwoDelta;

    // Affection Battle

    // Cooperative
    [SerializeField] private int cooperativeDelta;
}