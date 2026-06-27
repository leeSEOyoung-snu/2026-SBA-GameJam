using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicPlayerCanvasManager : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> stackCntTexts;
    [SerializeField] private List<Image> characterPortraits;

    private static readonly Color DeadColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private void Awake()
    {
        stackCntTexts.ForEach(t => t.text = "0");

        // 인스펙터 미할당 시 각 플레이어 슬롯(1P~4P) 하위의 ChrPortrait 이미지 자동 수집
        if (characterPortraits == null || characterPortraits.Count == 0)
        {
            characterPortraits = new List<Image>();
            Transform playersContainer = transform.Find("Players");
            if (playersContainer != null)
            {
                foreach (Transform slot in playersContainer)
                {
                    Transform portrait = slot.Find("ChrPortrait");
                    Image img = portrait != null ? portrait.GetComponent<Image>() : null;
                    if (img != null)
                        characterPortraits.Add(img);
                }
            }
        }
    }

    public void UpdateStackCnt(int playerId, int stackCnt)
    {
        stackCntTexts[playerId - 1].text = stackCnt.ToString();
    }

    // playerId: 1~4, 게임오버된 캐릭터의 초상화를 회색으로 처리
    public void GreyOutCharacter(int playerId)
    {
        if (characterPortraits == null) return;
        int idx = playerId - 1;
        if (idx < 0 || idx >= characterPortraits.Count) return;
        characterPortraits[idx].color = DeadColor;
    }
}
