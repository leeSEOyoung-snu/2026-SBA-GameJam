using System.Collections;
using UnityEngine;

public class MiniGameEvent : IBoardEvent
{
    public IEnumerator Execute()
    {
        Debug.Log("[MiniGameEvent] 미니 게임 시작");
        GameManager.Instance.LoadMiniGame();

        // MiniGameBase 씬이 종료될 때까지 Main Loop 대기
        yield return new WaitUntil(() => !GameManager.Instance.IsMiniGameRunning);

        Debug.Log("[MiniGameEvent] 미니 게임 종료, Main 재개");
    }
}
