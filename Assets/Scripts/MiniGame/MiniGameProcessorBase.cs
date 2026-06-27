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
    public int OnePlayerId { get; private set; }
    
    public void SetRandomPlayer(int onePlayerId)
    {
        OnePlayerId = onePlayerId;
    }
}

public abstract class TwoVsTwoBase : MiniGameProcessorBase
{
    public enum TwoVsTwoWinner
    {
        Draw,
        Team1,
        Team2,
    }
    
    public List<int> PlayerIdsTeam1 { get; private set; }
    public List<int> PlayerIdsTeam2 { get; private set; }
    public abstract TwoVsTwoWinner Winner { get; protected set; }

    public void SetRandomPlayer(List<int> team1)
    {
        PlayerIdsTeam1 = team1;
        PlayerIdsTeam2 = new List<int>(new int[] { 1, 2, 3, 4 });
        PlayerIdsTeam2.RemoveAll(id => team1.Contains(id));
    }
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