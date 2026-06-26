using System.Collections.Generic;
using UnityEngine;

public abstract class MiniGameProcessorBase : MonoBehaviour
{
    public abstract int NightmareDelta { get; protected set; }
}

public abstract class SoloBattleBase : MiniGameProcessorBase
{
    public abstract int RankPlayer1 { get; protected set; }
    public abstract int RankPlayer2 { get; protected set; }
    public abstract int RankPlayer3 { get; protected set; }
    public abstract int RankPlayer4 { get; protected set; }
}

public abstract class OneVsThreeBase : MiniGameProcessorBase
{
    public abstract bool IsOneWin { get; protected set; }
    public abstract int OnePlayerId { get; protected set; }
}

public abstract class TwoVsTwoBase : MiniGameProcessorBase
{
    public enum TwoVsTwoWinner
    {
        Draw,
        Team1,
        Team2,
    }
    
    public abstract List<int> PlayerIdsTeam1 { get; protected set; }
    public abstract List<int> PlayerIdsTeam2 { get; protected set; }
    public abstract TwoVsTwoWinner Winner { get; protected set; }
}

public abstract class AffectionBattleBase : MiniGameProcessorBase
{
    public abstract int AffectionDeltaPlayer1 { get; protected set; }
    public abstract int AffectionDeltaPlayer2 { get; protected set; }
    public abstract int AffectionDeltaPlayer3 { get; protected set; }
    public abstract int AffectionDeltaPlayer4 { get; protected set; }
}

public abstract class CooperativeBase : MiniGameProcessorBase
{
    public abstract bool IsSuccess {  get; protected set; } 
}