using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MiniGameManager : MonoBehaviour
{
    [SerializeField] private MiniGameResultContainer miniGameResultContainer;
    [SerializeField] private float tutorialShowSeconds = 2.5f;
    [SerializeField] private float resultShowSeconds = 3f;
    
    public static MiniGameManager Instance { get; private set; }
    public EffectManager Effects { get; private set; }
    
    public MiniGameResultContainer ResultContainer => miniGameResultContainer;

    private void Awake()
    {
        Instance = this;
        Effects = GetComponent<EffectManager>();
        GameManager.Instance.SetActiveAllInput(false);

        switch (miniGameResultContainer.Type)
        {
            case MiniGameTypes.OneVsThree:
                OneVsThreeBase oneThreeBase = FindAnyObjectByType<OneVsThreeBase>();
                if (oneThreeBase == null)
                    Debug.LogError($"OneVsThreeBase not found: {miniGameResultContainer.Type}");
                else
                    oneThreeBase.SetRandomPlayer(Random.Range(1, 5));
                break;
            
            case MiniGameTypes.TwoVsTwo:
                TwoVsTwoBase twoVsTwoBase = FindAnyObjectByType<TwoVsTwoBase>();
                if (twoVsTwoBase == null)
                    Debug.LogError($"TwoVsTwoBase not found: {miniGameResultContainer.Type}");
                else
                {
                    List<int> team1 = new List<int>();
                    while (team1.Count < 2)
                    {
                        int randomPlayerId = Random.Range(1, 5);
                        if (!team1.Contains(randomPlayerId))
                            team1.Add(randomPlayerId);
                    }
                    twoVsTwoBase.SetRandomPlayer(team1);
                }
                break;
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        yield return MiniGameStartFlow();
        
        yield return null;
        GameManager.Instance.SetActiveAllInput(true);
        StartTimeAttackView();
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

    }

    private void StartTimeAttackView()
    {
        if (miniGameResultContainer == null || !miniGameResultContainer.IsTimeAttack)
            return;

        BasicMiniGameCanvas basicCanvas = FindAnyObjectByType<BasicMiniGameCanvas>();
        if (basicCanvas != null)
            basicCanvas.StartTimeAttack(miniGameResultContainer.TimeAttackSeconds);
    }

    public void ArrangeOneVsThreePlayerLayout(int onePlayerId)
    {
        if (onePlayerId < 1 || onePlayerId > 4)
        {
            Debug.LogWarning($"Invalid OneVsThree player id: {onePlayerId}");
            return;
        }

        BasicPlayerCanvasManager playerCanvas = FindAnyObjectByType<BasicPlayerCanvasManager>(FindObjectsInactive.Include);
        if (playerCanvas == null)
        {
            Debug.LogWarning("BasicPlayerCanvasManager not found.");
            return;
        }

        RectTransform playersContainer = playerCanvas.transform.Find("Players") as RectTransform;
        if (playersContainer == null)
        {
            Debug.LogWarning("Players container not found in BasicPlayerCanvas.");
            return;
        }

        HorizontalLayoutGroup layoutGroup = playersContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null && layoutGroup.enabled)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(playersContainer);
            layoutGroup.enabled = false;
        }

        RectTransform[] playerSlots = GetPlayerSlotsById(playersContainer);
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (playerSlots[i] != null) continue;
            Debug.LogWarning($"Player slot {i + 1}P not found in BasicPlayerCanvas.");
            return;
        }

        Vector2[] positions = new Vector2[playerSlots.Length];
        for (int i = 0; i < playerSlots.Length; i++)
            positions[i] = playerSlots[i].anchoredPosition;

        int onePlayerIndex = onePlayerId - 1;
        playerSlots[onePlayerIndex].anchoredPosition = positions[0];

        int threeTeamPositionIndex = 1;
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i == onePlayerIndex) continue;

            playerSlots[i].anchoredPosition = positions[threeTeamPositionIndex];
            threeTeamPositionIndex++;
        }
    }

    public void ApplyCurrentMiniGameHechiSprite(GameObject target)
    {
        if (target == null)
            return;

        Sprite hechiSprite = GameManager.Instance.GetHechiSpriteOnMiniGame();
        if (hechiSprite == null)
        {
            Debug.LogWarning("Current mini game Hechi sprite not found.");
            return;
        }

        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = target.GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"SpriteRenderer not found: {target.name}");
            return;
        }

        spriteRenderer.sprite = hechiSprite;
    }

    private RectTransform[] GetPlayerSlotsById(RectTransform playersContainer)
    {
        RectTransform[] slotsById = new RectTransform[4];
        List<RectTransform> fallbackSlots = new();

        for (int i = 0; i < playersContainer.childCount; i++)
        {
            RectTransform slot = playersContainer.GetChild(i) as RectTransform;
            if (slot == null) continue;

            fallbackSlots.Add(slot);
            if (TryParsePlayerId(slot.name, out int playerId) && playerId >= 1 && playerId <= slotsById.Length)
                slotsById[playerId - 1] = slot;
        }

        for (int i = 0; i < slotsById.Length && i < fallbackSlots.Count; i++)
        {
            if (slotsById[i] == null)
                slotsById[i] = fallbackSlots[i];
        }

        return slotsById;
    }

    private bool TryParsePlayerId(string playerName, out int playerId)
    {
        playerName = playerName.Trim();
        if (playerName.EndsWith("P") || playerName.EndsWith("p"))
            playerName = playerName.Substring(0, playerName.Length - 1);

        return int.TryParse(playerName, out playerId);
    }

    public void QuitMiniGame()
    {
        GameManager.Instance.SetActiveAllInput(false);
        
        Dictionary<StateTypes, int> delta = CalculateStatesDelta();
        StartCoroutine(QuitMiniGameCoroutine(delta));
    }
    
    private IEnumerator QuitMiniGameCoroutine(Dictionary<StateTypes, int> delta)
    {
        BasicMiniGameCanvas basicCanvas = FindAnyObjectByType<BasicMiniGameCanvas>();

        if (basicCanvas != null)
            basicCanvas.StopTimeAttack();

        if (basicCanvas != null)
            yield return basicCanvas.PlayGameEnd().WaitForCompletion();

        if (basicCanvas != null)
            yield return basicCanvas.ShowCurtain().WaitForCompletion();

        if (basicCanvas != null)
            yield return basicCanvas.OpenResult(delta).WaitForCompletion();

        yield return new WaitForSecondsRealtime(resultShowSeconds);
        
        if (basicCanvas != null)
            yield return basicCanvas.CloseResult().WaitForCompletion();
        
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
