using System;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour,
    ISoloBattleResult,
    IOneVsThreeResult,
    ITwoVsTwoResult,
    IAffectionBattleResult,
    ICooperativeResult
{
    [SerializeField] private MiniGameResultContainer miniGameResultContainer;
    
    public static MiniGameManager Instance { get; private set; }
    public EffectManager Effects { get; private set; }

    private void Awake()
    {
        Instance = this;
        Effects = GetComponent<EffectManager>();
    }

    /// <summary>
    /// 미니게임 종료 시 호출. container의 타입에 맞는 인터페이스만 반환됨.
    /// 예) GetResultHandler&lt;ISoloBattleResult&gt;()
    /// </summary>
    public T GetResultHandler<T>() where T : class
    {
        if (this is T handler)
            return handler;

        Debug.LogError($"[MiniGameManager] {miniGameResultContainer.Type}에서 {typeof(T).Name} 호출 불가");
        return null;
    }

    void ISoloBattleResult.QuitMiniGame(int firstId, int secondId, int thirdId, int fourthId, int nightmareDelta)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { (StateTypes)firstId, miniGameResultContainer.SoloDelta.first },
            { (StateTypes)secondId, miniGameResultContainer.SoloDelta.second },
            { (StateTypes)thirdId, miniGameResultContainer.SoloDelta.third },
            { (StateTypes)fourthId, miniGameResultContainer.SoloDelta.fourth },
            { StateTypes.Nightmare, nightmareDelta }
        };
        
        CommonQuitMiniGame(delta);
    }

    void IOneVsThreeResult.QuitMiniGameOneWin(int onePlayerId, int nightmareDelta)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { (StateTypes)onePlayerId, miniGameResultContainer.OneVsThreeDelta.oneWin },
            { StateTypes.Nightmare, nightmareDelta }
        };
        
        CommonQuitMiniGame(delta);
    }

    void IOneVsThreeResult.QuitMiniGameThreeWin(int threePlayerId1, int threePlayerId2, int threePlayerId3, int nightmareDelta)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { (StateTypes)threePlayerId1, miniGameResultContainer.OneVsThreeDelta.threeWin },
            { (StateTypes)threePlayerId2, miniGameResultContainer.OneVsThreeDelta.threeWin },
            { (StateTypes)threePlayerId3, miniGameResultContainer.OneVsThreeDelta.threeWin },
            { StateTypes.Nightmare, nightmareDelta }
        };
        
        CommonQuitMiniGame(delta);
    }

    void ITwoVsTwoResult.QuitMiniGame(int winPlayerId1, int winPlayerId2, int nightmareDelta)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { (StateTypes)winPlayerId1, miniGameResultContainer.TwoVsTwoDelta },
            { (StateTypes)winPlayerId2, miniGameResultContainer.TwoVsTwoDelta },
            { StateTypes.Nightmare, nightmareDelta }, 
        };
        
        CommonQuitMiniGame(delta);
    }

    void IAffectionBattleResult.QuitMiniGame(int player1Affection, int player2Affection, int player3Affection, int player4Affection, int nightmareDelta)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { (StateTypes)1, player1Affection },
            { (StateTypes)2,  player2Affection },
            { (StateTypes)3,  player3Affection },
            { (StateTypes)4, player4Affection },
            { StateTypes.Nightmare, nightmareDelta }
        };
        
        CommonQuitMiniGame(delta);
    }

    void ICooperativeResult.QuitMiniGame(bool isSuccess, int nightmareDelta)
    {
        Dictionary<StateTypes, int> delta;
        
        delta = isSuccess ? new Dictionary<StateTypes, int>()
        {
            { (StateTypes)1, miniGameResultContainer.CooperativeDelta},
            { (StateTypes)2, miniGameResultContainer.CooperativeDelta },
            { (StateTypes)3, miniGameResultContainer.CooperativeDelta },
            { (StateTypes)4, miniGameResultContainer.CooperativeDelta },
            { StateTypes.Nightmare, nightmareDelta },
        } : new Dictionary<StateTypes, int>()
        {
            { StateTypes.Nightmare, nightmareDelta },
        };
        
        CommonQuitMiniGame(delta);
    }

    private void CommonQuitMiniGame(Dictionary<StateTypes, int> delta)
    {
        GameManager.Instance.QuitMiniGame(delta);
    }
}
