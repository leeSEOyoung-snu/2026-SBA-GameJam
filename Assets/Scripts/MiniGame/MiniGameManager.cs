using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.QuitMiniGame(new Dictionary<StateTypes, int>()
        {
            { StateTypes.Nightmare , 10 },
            { StateTypes.Courage, 5 },
            { StateTypes.Wisdom, 3 },
            { StateTypes.Recovery, 7 },
            { StateTypes.Love, 2 }
        });
    }
}
