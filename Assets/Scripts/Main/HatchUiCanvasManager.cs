using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HatchUiCanvasManager : MonoBehaviour
{
    #region Gage
    
    [SerializeField] private List<StateGage> gagesRaw;
    [SerializeField] private float gageUpdateDuration;
    [SerializeField] private Ease gageUpdateEase;
    [SerializeField] private Image hechiProfile;
    
    [Serializable]
    private struct StateGage
    {
        public StateTypes type;
        public Image gageImg;
    }
    
    #endregion
    
    private Dictionary<StateTypes, Image> _gageImgDict = new();

    private const int MAX_STATE = 50;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        gagesRaw.ForEach(r =>
        {
            _gageImgDict.Add(r.type, r.gageImg);
            r.gageImg.fillAmount = 0f;
        });

        hechiProfile.sprite = GameManager.Instance.GetHechiSpriteOnMain();
    }

    public IEnumerator UpdateStates(Dictionary<StateTypes, int> currStates)
    {
        Sequence seq = DOTween.Sequence();

        foreach (var curr in currStates)
        {
            // TODO: Nightmare Gage 연결
            if (curr.Key == StateTypes.Nightmare)
                continue;
            
            seq.Join(_gageImgDict[curr.Key].DOFillAmount((float)curr.Value / MAX_STATE, gageUpdateDuration)
                .SetEase(gageUpdateEase));
        }
        
        yield return seq.WaitForCompletion();
    }

    public IEnumerator EvolutionCoroutine(Sprite hechiSprite)
    {
        // TODO: 변경 애니메이션
        hechiProfile.sprite = hechiSprite;
        yield break;
    }
}
