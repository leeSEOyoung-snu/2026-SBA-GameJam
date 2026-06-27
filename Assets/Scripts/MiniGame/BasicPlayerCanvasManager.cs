using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BasicPlayerCanvasManager : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> stackCntTexts;

    private void Awake()
    {
        stackCntTexts.ForEach(t => t.text = "0");
    }
    
    public void UpdateStackCnt(int playerId, int stackCnt)
    {
        stackCntTexts[playerId - 1].text = stackCnt.ToString();
    }
}
