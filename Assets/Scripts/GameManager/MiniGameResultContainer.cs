using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MiniGameResultContainer", menuName = "Scriptable Objects/MiniGameResultContainer")]
public class MiniGameResultContainer : ScriptableObject
{
    [SerializeField] private MiniGameTypes type;
    public MiniGameTypes Type => type;
    
    // Solo
    [Serializable]
    public struct SoloBattleResultByRanking
    {
        public int first;
        public int second;
        public int third;
        public int fourth;
    }
    [SerializeField] private SoloBattleResultByRanking soloDelta;
    public SoloBattleResultByRanking SoloDelta => soloDelta;
    
    // 1 vs 3
    [Serializable]
    public struct OneVsThreeResultByWinner
    {
        public int oneWin;
        public int threeWin;
    }
    [SerializeField] private OneVsThreeResultByWinner oneVsThreeDelta;
    public  OneVsThreeResultByWinner OneVsThreeDelta => oneVsThreeDelta;
    
    // 2 vs 2
    [SerializeField] private int twoVsTwoDelta;
    public int TwoVsTwoDelta => twoVsTwoDelta;

    // Affection Battle

    // Cooperative
    [SerializeField] private int cooperativeDelta;
    public int CooperativeDelta => cooperativeDelta;
}