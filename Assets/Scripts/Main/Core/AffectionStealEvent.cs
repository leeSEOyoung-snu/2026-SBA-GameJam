using System.Collections;
using System.Linq;
using UnityEngine;

public class AffectionStealEvent : IBoardEvent
{
    private const int StealMin = 5;
    private const int StealMax = 15;

    private readonly MainSceneManager _sceneManager;
    private readonly IPlayerInputReader[] _players;

    public int ThiefId { get; }

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

        ThiefId = lowestPlayerIds[Random.Range(0, lowestPlayerIds.Length)];
    }

    public IEnumerator Execute()
    {
        var affectionById = _sceneManager.StateContainer.AffectionById;

        // 나머지 3명을 Left / Up / Right 에 배정
        int[] others = affectionById.Keys.Where(id => id != ThiefId).ToArray();
        // others는 순서 보장 안 되므로 정렬
        System.Array.Sort(others);

        // Left=others[0], Up=others[1], Right=others[2]
        Debug.Log($"[AffectionSteal] Player {ThiefId}가 강탈자. Left=P{others[0]}, Up=P{others[1]}, Right=P{others[2]}");
        Debug.Log("[AffectionSteal] Left/Up/Right 버튼으로 강탈 대상 선택");

        IPlayerInputReader thiefInput = _players[ThiefId - 1];
        int targetId = -1;

        while (targetId == -1)
        {
            if (thiefInput.Left)  targetId = others[0];
            if (thiefInput.Up)    targetId = others[1];
            if (thiefInput.Right) targetId = others[2];
            yield return null;
        }

        int stealAmount = Random.Range(StealMin, StealMax + 1);

        // CommonStats와 무관한 순수 호감도 이전
        _sceneManager.StateContainer.TransferAffection(targetId, ThiefId, stealAmount);

        Debug.Log($"[AffectionSteal] Player {ThiefId}가 Player {targetId}에게서 {stealAmount} 강탈");

        yield return _sceneManager.RefreshAffectionUI();
    }
}
