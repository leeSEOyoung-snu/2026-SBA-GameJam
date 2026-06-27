using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DontWorryPlayerCanvasManager : MonoBehaviour
{
    [SerializeField] private RectTransform playersContainer;
    [SerializeField] private List<RectTransform> playerSlots;   // index 0 = 1P, 1 = 2P, ...
    [SerializeField] private List<Image> playerPortraits;       // ChrPortrait Image per slot

    private static readonly Color DeadColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    // onePlayerId: 1~4, 이 플레이어가 1인팀 슈터
    public void Init(int onePlayerId)
    {
        // playersContainer 미할당 시 자동으로 찾기
        if (playersContainer == null)
            playersContainer = GetComponentInChildren<HorizontalLayoutGroup>()?.GetComponent<RectTransform>();
        if (playersContainer == null) return;

        // playerSlots 미할당 시 자동으로 찾기 (Players의 직계 자식들)
        if (playerSlots == null || playerSlots.Count == 0)
        {
            playerSlots = new List<RectTransform>();
            for (int i = 0; i < playersContainer.childCount; i++)
                playerSlots.Add(playersContainer.GetChild(i).GetComponent<RectTransform>());
        }

        int oneIndex = onePlayerId - 1;
        if (oneIndex < 0 || oneIndex >= playerSlots.Count) return;

        // HorizontalLayoutGroup 비활성화 → 직접 좌표 배치
        var hlg = playersContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null) hlg.enabled = false;

        // 기존 슬롯 위치를 그대로 재활용
        // 1인 플레이어 → 슬롯 0번(1P) 위치, 3인팀 → 슬롯 1,2,3번(2P,3P,4P) 위치
        var positions = new Vector2[playerSlots.Count];
        for (int i = 0; i < playerSlots.Count; i++)
            positions[i] = playerSlots[i].anchoredPosition;

        playerSlots[oneIndex].anchoredPosition = positions[0];

        int teamIdx = 1;
        for (int i = 0; i < playerSlots.Count; i++)
        {
            if (i == oneIndex) continue;
            playerSlots[i].anchoredPosition = positions[teamIdx++];
        }
    }

    // playerId: 1~4, 사망한 3인팀 플레이어 초상화를 회색으로
    public void EliminatePlayer(int playerId)
    {
        int idx = playerId - 1;
        if (idx < 0 || idx >= playerPortraits.Count) return;
        playerPortraits[idx].color = DeadColor;
    }
}
