using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HechiSpriteData
{
    public int conditionCnt;
    public StateTypes condition1;
    public StateTypes condition2;
    public Sprite mainSprite;
    public Sprite miniGameSprite;
    public string hechiName;

    public override string ToString()
    {
        return
            $"HechiSpriteData(Name: {hechiName}, conditionCnt: {conditionCnt}, condition1: {condition1}, condition2: {condition2}, mainSprite: {mainSprite.name}, miniGameSprite: {miniGameSprite.name})";
    }
}

public class HechiSpriteContainer : MonoBehaviour
{
    [SerializeField] private List<HechiSpriteData> hechiSpriteData;

    private Dictionary<(StateTypes? cond1, StateTypes? cond2), (Sprite main, Sprite miniGame, string name)> _hechiSpriteDict = new();

    private void Awake()
    {
        foreach (HechiSpriteData data in hechiSpriteData)
        {
            (StateTypes?, StateTypes?) newKey = (data.conditionCnt > 0 ? data.condition1 : null,
                data.conditionCnt > 1 ? data.condition2 : null);
            (Sprite, Sprite, string) newValue = (data.mainSprite, data.miniGameSprite, data.hechiName);

            if(!_hechiSpriteDict.TryAdd(newKey, newValue))
                Debug.LogError($"<color=red>[Hechi Sprite Container] 중복 key 있음 ({data})</color>");
        }
    }

    public Sprite GetHechiSpriteOnMain(List<StateTypes> states)
    {
        if (!_hechiSpriteDict.TryGetValue((states.Count > 0 ? states[0] : null, states.Count > 1 ? states[1] : null),
                out var value))
            return hechiSpriteData[0].mainSprite;
        return value.main;
    }

    public Sprite GetHechiSpriteOnMiniGame(List<StateTypes> states)
    {
        if (!_hechiSpriteDict.TryGetValue((states.Count > 0 ? states[0] : null, states.Count > 1 ? states[1] : null),
                out var value))
            return hechiSpriteData[0].miniGameSprite;
        return value.miniGame;
    }

    public string GetHechiName(List<StateTypes> states)
    {
        if (!_hechiSpriteDict.TryGetValue((states.Count > 0 ? states[0] : null, states.Count > 1 ? states[1] : null),
                out var value))
            return hechiSpriteData[0].hechiName;
        return value.name;
    }
}
