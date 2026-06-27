using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DontWorryPlayerCanvasManager : MonoBehaviour
{
    [SerializeField] private RectTransform playersContainer;
    [SerializeField] private List<Image> playerPortraits;       // ChrPortrait Image per slot

    private static readonly Color DeadColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    // onePlayerId: 1~4, 이 플레이어가 1인팀 슈터
    public void Init(int onePlayerId)
    {
        MiniGameManager.Instance?.ArrangeOneVsThreePlayerLayout(onePlayerId);

        // playerPortraits 미할당 시 자동으로 찾기
        if (playersContainer == null)
            playersContainer = GetComponentInChildren<HorizontalLayoutGroup>()?.GetComponent<RectTransform>();
        if (playerPortraits == null)
            playerPortraits = new List<Image>();
        if (playersContainer == null || playerPortraits.Count > 0) return;

        foreach (Image portrait in playersContainer.GetComponentsInChildren<Image>(true))
        {
            if (portrait.gameObject.name == "ChrPortrait")
                playerPortraits.Add(portrait);
        }
    }

    // playerId: 1~4, 사망한 3인팀 플레이어 초상화를 회색으로
    public void EliminatePlayer(int playerId)
    {
        if (playerPortraits == null) return;

        int idx = playerId - 1;
        if (idx < 0 || idx >= playerPortraits.Count) return;
        playerPortraits[idx].color = DeadColor;
    }
}
