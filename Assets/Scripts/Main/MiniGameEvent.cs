using System.Collections;
using UnityEngine;
// using UnityEngine.SceneManagement; // 추후 Additive 로드 시 활성화

public class MiniGameEvent : IBoardEvent
{
    public IEnumerator Execute()
    {
        Debug.Log("[MiniGameEvent] 미니 게임 시작!");
        // TODO: yield return SceneManager.LoadSceneAsync("MiniGame", LoadSceneMode.Additive);
        yield return null;
        Debug.Log("[MiniGameEvent] 미니 게임 종료!");
    }
}
