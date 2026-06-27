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
    
    public void SetStackAsString(int playerId, string stackStr)
    {
        TextMeshProUGUI text = stackCntTexts[playerId - 1];
        // 텍스트가 속한 Stacking 오브젝트가 비활성일 수 있으므로 활성화
        if (text.transform.parent != null)
            text.transform.parent.gameObject.SetActive(true);
        text.gameObject.SetActive(true);
        text.text = stackStr;
    }

    // 특정 플레이어의 스택 말풍선 크기/위치/폰트 조정 (씬별 커스텀용)
    // bubbleYOffset: Stacking 로컬 y(캐릭터와 안 겹치게 위로 올림), boxSize: 말풍선/텍스트 크기, fontSize: 글자 크기
    public void StyleStackBubble(int playerId, float bubbleYOffset, Vector2 boxSize, float fontSize)
    {
        TextMeshProUGUI text = stackCntTexts[playerId - 1];

        Transform stacking = text.transform.parent;
        if (stacking is RectTransform stackingRt)
        {
            Vector2 pos = stackingRt.anchoredPosition;
            pos.y = bubbleYOffset;
            stackingRt.anchoredPosition = pos;

            Transform box = stacking.Find("StackBox");
            if (box is RectTransform boxRt)
                boxRt.sizeDelta = boxSize;
        }

        text.rectTransform.sizeDelta = boxSize;
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.enableWordWrapping = true;
        text.alignment = TextAlignmentOptions.Center;
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
