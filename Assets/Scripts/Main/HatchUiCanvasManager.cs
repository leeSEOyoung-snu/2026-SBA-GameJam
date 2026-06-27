using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
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

    [SerializeField] private float gageHeadroom = 20f;
    [SerializeField] private TextMeshProUGUI hechiNameText;

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
        hechiNameText.text = $"<wiggle>{GameManager.Instance.GetHechiName()}</wiggle>";
    }

    public IEnumerator UpdateStates(Dictionary<StateTypes, int> currStates)
    {
        int maxValue = 0;
        foreach (var curr in currStates)
        {
            if (_gageImgDict.ContainsKey(curr.Key) && curr.Value > maxValue)
                maxValue = curr.Value;
        }

        float denominator = maxValue + gageHeadroom;

        Sequence seq = DOTween.Sequence();

        foreach (var curr in currStates)
        {
            if (!_gageImgDict.ContainsKey(curr.Key))
                continue;

            float fill = (float)curr.Value / denominator;
            seq.Join(_gageImgDict[curr.Key].DOFillAmount(fill, gageUpdateDuration).SetEase(gageUpdateEase));
        }

        yield return seq.WaitForCompletion();
    }

    [SerializeField] private float evolutionShrinkDuration = 0.3f;
    [SerializeField] private float evolutionGrowDuration = 0.5f;
    [SerializeField] private Ease evolutionShrinkEase = Ease.InBack;
    [SerializeField] private Ease evolutionGrowEase = Ease.OutBack;

    public IEnumerator EvolutionCoroutine(Sprite hechiSprite, string hechiName)
    {
        yield return hechiProfile.transform.DOScale(Vector3.zero, evolutionShrinkDuration)
            .SetEase(evolutionShrinkEase)
            .WaitForCompletion();

        hechiProfile.sprite = hechiSprite;
        hechiNameText.text = $"<wiggle>{hechiName}</wiggle>";

        yield return hechiProfile.transform.DOScale(Vector3.one, evolutionGrowDuration)
            .SetEase(evolutionGrowEase)
            .WaitForCompletion();
    }
}
