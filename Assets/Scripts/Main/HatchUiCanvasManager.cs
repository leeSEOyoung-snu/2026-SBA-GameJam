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

    [SerializeField] private float evolutionShrinkDuration = 0.3f;
    [SerializeField] private float evolutionGrowDuration = 0.5f;
    [SerializeField] private Ease evolutionShrinkEase = Ease.InBack;
    [SerializeField] private Ease evolutionGrowEase = Ease.OutBack;

    public IEnumerator EvolutionCoroutine(Sprite hechiSprite)
    {
        yield return hechiProfile.transform.DOScale(Vector3.zero, evolutionShrinkDuration)
            .SetEase(evolutionShrinkEase)
            .WaitForCompletion();

        hechiProfile.sprite = hechiSprite;

        yield return hechiProfile.transform.DOScale(Vector3.one, evolutionGrowDuration)
            .SetEase(evolutionGrowEase)
            .WaitForCompletion();
    }
}
