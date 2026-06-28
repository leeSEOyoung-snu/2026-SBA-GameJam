using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AffectionStealEvent : IBoardEvent
{
    private const int StealMin = 5;
    private const int StealMax = 10;

    private readonly MainSceneManager _sceneManager;
    private readonly IPlayerInputReader[] _players;

    public int ThiefId { get; }
    public int TargetId { get; private set; }
    public int StealAmount { get; private set; }

    public AffectionStealEvent(MainSceneManager sceneManager, IPlayerInputReader[] players)
    {
        _sceneManager = sceneManager;
        _players = players;

        var affectionById = _sceneManager.StateContainer.AffectionById;
        int lowestAffection = affectionById.Min(kv => kv.Value);
        int[] lowestPlayerIds = affectionById
            .Where(kv => kv.Value == lowestAffection)
            .Select(kv => kv.Key)
            .ToArray();

        // ThiefId = lowestPlayerIds[Random.Range(0, lowestPlayerIds.Length)];
        // TargetId = PickHighestAffectionTarget(affectionById);
        
        // 꼴지가 랜덤 가져감
        ThiefId = lowestPlayerIds[Random.Range(0, lowestPlayerIds.Length)];
        while (true)
        {
            TargetId = Random.Range(1, lowestPlayerIds.Length+1);
            if (TargetId != ThiefId)
                break;
        }
    }

    public IEnumerator Execute()
    {
        Debug.Log($"[AffectionSteal] Player {ThiefId}가 강탈자. Player {TargetId}에게서 강탈 예정");
        Debug.Log("[AffectionSteal] 강탈자가 조이콘을 흔들면 강탈 실행");

        IPlayerInputReader thiefInput = _players[ThiefId - 1];
        while (!thiefInput.Swing)
            yield return null;

        StealAmount = Random.Range(StealMin, StealMax + 1);

        // CommonStats와 무관한 순수 호감도 이전
        _sceneManager.StateContainer.TransferAffection(TargetId, ThiefId, StealAmount);

        Debug.Log($"[AffectionSteal] Player {ThiefId}가 Player {TargetId}에게서 {StealAmount} 강탈");

        yield return _sceneManager.RefreshAffectionUI();
    }

    private int PickHighestAffectionTarget(System.Collections.Generic.Dictionary<int, int> affectionById)
    {
        int highestAffection = affectionById
            .Where(kv => kv.Key != ThiefId)
            .Max(kv => kv.Value);

        int[] highestPlayerIds = affectionById
            .Where(kv => kv.Key != ThiefId && kv.Value == highestAffection)
            .Select(kv => kv.Key)
            .ToArray();

        return highestPlayerIds[Random.Range(0, highestPlayerIds.Length)];
    }
}
