public interface ISoloBattleResult
{
    void QuitMiniGame(int firstId, int secondId, int thirdId, int fourthId, int nightmareDelta);
}

public interface IOneVsThreeResult
{
    void QuitMiniGameOneWin(int onePlayerId, int nightmareDelta);
    void QuitMiniGameThreeWin(int threePlayerId1, int threePlayerId2, int threePlayerId3, int nightmareDelta);
}

public interface ITwoVsTwoResult
{
    void QuitMiniGame(int winPlayerId1, int winPlayerId2, int nightmareDelta);
}

public interface IAffectionBattleResult
{
    void QuitMiniGame(int player1Affection, int player2Affection, int player3Affection, int player4Affection, int nightmareDelta);
}

public interface ICooperativeResult
{
    void QuitMiniGame(bool isSuccess, int nightmareDelta);
}
