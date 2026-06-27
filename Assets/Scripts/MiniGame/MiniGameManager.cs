using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    [SerializeField] private MiniGameResultContainer miniGameResultContainer;
    [SerializeField] private float tutorialShowSeconds = 2.5f;
    
    public static MiniGameManager Instance { get; private set; }
    public EffectManager Effects { get; private set; }

    private void Awake()
    {
        Instance = this;
        Effects = GetComponent<EffectManager>();
        GameManager.Instance.SetActiveAllInput(false);
    }

    private IEnumerator Start()
    {
        yield return null;
        yield return MiniGameStartFlow();
        
        yield return null;
        GameManager.Instance.SetActiveAllInput(true);
    }

    private IEnumerator MiniGameStartFlow()
    {
        BasicMiniGameCanvas basicCanvas = FindAnyObjectByType<BasicMiniGameCanvas>();
        MiniGameProcessorBase processor = FindAnyObjectByType<MiniGameProcessorBase>();

        if (basicCanvas != null)
        {
            basicCanvas.SetTutorial(miniGameResultContainer, processor);
            yield return basicCanvas.OpenTutorial().WaitForCompletion();
            yield return new WaitForSeconds(tutorialShowSeconds);
            yield return basicCanvas.CloseTutorial().WaitForCompletion();
        }

        if (basicCanvas != null)
            yield return basicCanvas.HideCurtain().WaitForCompletion();

        if (basicCanvas != null)
            yield return basicCanvas.PlayGameStart().WaitForCompletion();

        if (basicCanvas != null && miniGameResultContainer != null && miniGameResultContainer.IsTimeAttack)
            basicCanvas.StartTimeAttack(miniGameResultContainer.TimeAttackSeconds);
    }

    public void QuitMiniGame()
    {
        GameManager.Instance.SetActiveAllInput(false);
        
        Dictionary<StateTypes, int> delta = CalculateStatesDelta();
        StartCoroutine(QuitMiniGameCoroutine(delta));
    }
    
    private IEnumerator QuitMiniGameCoroutine(Dictionary<StateTypes, int> delta)
    {
        // TODO: 결과창 canvas 연결

        yield return null;
        GameManager.Instance.QuitMiniGame(delta);
    }

    private Dictionary<StateTypes, int> CalculateStatesDelta()
    {
        Dictionary<StateTypes, int> delta = new();
        switch (miniGameResultContainer.Type)
        {
            case MiniGameTypes.SoloBattle:
                SoloBattleBase soloBattle = FindAnyObjectByType<SoloBattleBase>();
                if (soloBattle == null)
                    Debug.LogError($"SoloBattleBase not found: {miniGameResultContainer.Type}");
                else
                    delta = SoloBattleQuitMiniGame(soloBattle);
                break;
            
            case MiniGameTypes.OneVsThree:
                OneVsThreeBase oneVsThree = FindAnyObjectByType<OneVsThreeBase>();
                if (oneVsThree == null)
                    Debug.LogError($"OneVsThreeBase not found: {miniGameResultContainer.Type}");
                else
                    delta = OneVsThreeQuitMiniGame(oneVsThree);
                break;
            
            case MiniGameTypes.TwoVsTwo:
                TwoVsTwoBase twoVsTwo = FindAnyObjectByType<TwoVsTwoBase>();
                if (twoVsTwo == null)
                    Debug.LogError($"TwoVsTwoBase not found: {miniGameResultContainer.Type}");
                else
                    delta = TwoVsTwoQuitMiniGame(twoVsTwo);
                break;
            
            case MiniGameTypes.AffectionBattle:
                AffectionBattleBase affectionBattle = FindAnyObjectByType<AffectionBattleBase>();
                if (affectionBattle == null)
                    Debug.LogError($"AffectionBattleBase not found: {miniGameResultContainer.Type}");
                else
                    delta = AffectionBattleQuitMiniGame(affectionBattle);
                break;
            
            case MiniGameTypes.Cooperative:
                CooperativeBase cooperative = FindAnyObjectByType<CooperativeBase>();
                if (cooperative == null)
                    Debug.LogError($"CooperativeBase not found: {miniGameResultContainer.Type}");
                else
                    delta = CooperativeQuitMiniGame(cooperative);
                break;
        }

        return delta;
    }
    
    private Dictionary<StateTypes, int> SoloBattleQuitMiniGame(SoloBattleBase soloBattleBase)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { (StateTypes)1, miniGameResultContainer.GetDeltaByRankSoloBattle(soloBattleBase.RankPlayer1) },
            { (StateTypes)2, miniGameResultContainer.GetDeltaByRankSoloBattle(soloBattleBase.RankPlayer2) },
            { (StateTypes)3, miniGameResultContainer.GetDeltaByRankSoloBattle(soloBattleBase.RankPlayer3) },
            { (StateTypes)4, miniGameResultContainer.GetDeltaByRankSoloBattle(soloBattleBase.RankPlayer4) },
            { StateTypes.Nightmare, soloBattleBase.NightmareDelta }
        };

        return delta;
    }

    private Dictionary<StateTypes, int> OneVsThreeQuitMiniGame(OneVsThreeBase oneVsThreeBase)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { StateTypes.Nightmare, oneVsThreeBase.NightmareDelta }
        };
        
        if (oneVsThreeBase.IsOneWin)
            delta.Add((StateTypes)oneVsThreeBase.OnePlayerId, miniGameResultContainer.OneVsThreeDelta.oneWin);
        else
        {
            List<int> threePlayers = new List<int>() { 1, 2, 3, 4 };
            threePlayers.Remove(oneVsThreeBase.OnePlayerId);
            threePlayers.ForEach(playerId => delta.Add((StateTypes)playerId, miniGameResultContainer.OneVsThreeDelta.threeWin));
        }

        return delta;
    }

    private Dictionary<StateTypes, int> TwoVsTwoQuitMiniGame(TwoVsTwoBase twoVsTwoBase)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { StateTypes.Nightmare, twoVsTwoBase.NightmareDelta }
        };

        switch (twoVsTwoBase.Winner)
        {
            case TwoVsTwoBase.TwoVsTwoWinner.Draw:
                twoVsTwoBase.PlayerIdsTeam1.ForEach(playerId => delta.Add((StateTypes)playerId, miniGameResultContainer.TwoVsTwoDelta));
                twoVsTwoBase.PlayerIdsTeam2.ForEach(playerId => delta.Add((StateTypes)playerId, miniGameResultContainer.TwoVsTwoDelta));
                break;
            
            case TwoVsTwoBase.TwoVsTwoWinner.Team1:
                twoVsTwoBase.PlayerIdsTeam1.ForEach(playerId => delta.Add((StateTypes)playerId, miniGameResultContainer.TwoVsTwoDelta));
                break;
            
            case TwoVsTwoBase.TwoVsTwoWinner.Team2:
                twoVsTwoBase.PlayerIdsTeam2.ForEach(playerId => delta.Add((StateTypes)playerId, miniGameResultContainer.TwoVsTwoDelta));
                break;
        }

        return delta;
    }

    private Dictionary<StateTypes, int> AffectionBattleQuitMiniGame(AffectionBattleBase affectionBattleBase)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { (StateTypes)1, affectionBattleBase.AffectionDeltaPlayer1 },
            { (StateTypes)2,  affectionBattleBase.AffectionDeltaPlayer2 },
            { (StateTypes)3,  affectionBattleBase.AffectionDeltaPlayer3 },
            { (StateTypes)4, affectionBattleBase.AffectionDeltaPlayer4 },
            { StateTypes.Nightmare, affectionBattleBase.NightmareDelta }
        };

        return delta;
    }

    private Dictionary<StateTypes, int> CooperativeQuitMiniGame(CooperativeBase cooperativeBase)
    {
        Dictionary<StateTypes, int> delta = new Dictionary<StateTypes, int>()
        {
            { StateTypes.Nightmare, cooperativeBase.NightmareDelta },
        };

        if (cooperativeBase.IsSuccess)
        {
            delta.Add((StateTypes)1, miniGameResultContainer.CooperativeDelta);
            delta.Add((StateTypes)2, miniGameResultContainer.CooperativeDelta);
            delta.Add((StateTypes)3, miniGameResultContainer.CooperativeDelta);
            delta.Add((StateTypes)4, miniGameResultContainer.CooperativeDelta);
        }

        return delta;
    }
}
