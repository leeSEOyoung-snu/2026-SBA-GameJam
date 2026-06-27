using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PlayerUiCanvasManager : MonoBehaviour
{
    #region Player
    
    [SerializeField] private List<PlayerAffection> playerAffectionRaw;

    [Serializable]
    private struct PlayerAffection
    {
        public int playerId;
        public TextMeshProUGUI affection;
    }
    
    #endregion
    
    private Dictionary<int, TextMeshProUGUI> _playerAffectionDict = new Dictionary<int, TextMeshProUGUI>();

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        playerAffectionRaw.ForEach(r =>
        {
            _playerAffectionDict.Add(r.playerId, r.affection);
            r.affection.text = "0";
        });
    }

    public IEnumerator UpdateAffection(Dictionary<int, int> currAffectionById)
    {
        Sequence seq = DOTween.Sequence();

        foreach (var curr in currAffectionById)
        {
            // TODO: 어쩌구저쩌구 애니메이션 추가?
            _playerAffectionDict[curr.Key].text = curr.Value.ToString();
        }
        
        yield return seq.WaitForCompletion();
    }
}
