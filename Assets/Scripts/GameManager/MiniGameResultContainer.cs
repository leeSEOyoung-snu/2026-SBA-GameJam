using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MiniGameResultContainer", menuName = "Scriptable Objects/MiniGameResultContainer")]
public class MiniGameResultContainer : ScriptableObject
{
    [SerializeField] private MiniGameTypes type;
    public MiniGameTypes Type => type;

    [SerializeField] private string gameTitle;
    public string GameTitle => gameTitle;

    [SerializeField, TextArea] private string desc;
    public string Desc => desc;

    [SerializeField] private bool isTimeAttack;
    public bool IsTimeAttack => isTimeAttack;

    [SerializeField] private int timeAttackSeconds;
    public int TimeAttackSeconds => timeAttackSeconds;

    [Serializable]
    public struct TwoVsTwoTutorialText
    {
        [TextArea] public string twoWinCondition;
        [TextArea] public string nightmareCondition;
    }

    [Serializable]
    public struct SoloBattleTutorialText
    {
        [TextArea] public string soloWinCondition;
        [TextArea] public string nightmareCondition;
    }

    [Serializable]
    public struct CooperativeTutorialText
    {
        [TextArea] public string coopWinCondition;
        [TextArea] public string nightmareCondition;
    }
    
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

    [SerializeField] private SoloBattleTutorialText soloBattleTutorialText;
    public SoloBattleTutorialText SoloBattleTutorial => soloBattleTutorialText;

    public int GetDeltaByRankSoloBattle(int rank)
    {
        return rank == 1 ? soloDelta.first : rank == 2 ? soloDelta.second : rank == 3 ? soloDelta.third : soloDelta.fourth;
    }
    
    // 1 vs 3
    [Serializable]
    public struct OneVsThreeResultByWinner
    {
        public int oneWin;
        public int threeWin;
    }
    [SerializeField] private OneVsThreeResultByWinner oneVsThreeDelta;
    public  OneVsThreeResultByWinner OneVsThreeDelta => oneVsThreeDelta;

    [Serializable]
    public struct OneVsThreeTutorialText
    {
        [TextArea] public string oneWinCondition;
        [TextArea] public string threeWinCondition;
    }
    [SerializeField] private OneVsThreeTutorialText oneVsThreeTutorialText;
    public OneVsThreeTutorialText OneVsThreeTutorial => oneVsThreeTutorialText;
    
    // 2 vs 2
    [SerializeField] private int twoVsTwoDelta;
    public int TwoVsTwoDelta => twoVsTwoDelta;

    [SerializeField] private TwoVsTwoTutorialText twoVsTwoTutorialText;
    public TwoVsTwoTutorialText TwoVsTwoTutorial => twoVsTwoTutorialText;

    // Affection Battle

    // Cooperative
    [SerializeField] private int cooperativeDelta;
    public int CooperativeDelta => cooperativeDelta;

    [SerializeField] private CooperativeTutorialText cooperativeTutorialText;
    public CooperativeTutorialText CooperativeTutorial => cooperativeTutorialText;
}
