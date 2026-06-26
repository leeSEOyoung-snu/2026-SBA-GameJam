using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainGameLoop : MonoBehaviour
{
    private const int PlayerCount = 4;

    private IPlayerInputReader[] _players;
    private Board _board;
    private GamePiece _piece;

    public void Init(Board board, GamePiece piece)
    {
        _board = board;
        _piece = piece;

        _players = new IPlayerInputReader[PlayerCount];
        for (int i = 0; i < PlayerCount; i++)
            _players[i] = GameManager.Instance.GetPlayerInputReader(i + 1);

        _piece.PlaceAt(_board.CurrentCell.transform.position);

        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            // 1단계: 윷 던지기
            YutType yutResult = default;
            yield return StartCoroutine(ThrowYutPhase(r => yutResult = r));

            // 2단계: 말 이동
            yield return StartCoroutine(MovePiecePhase(YutCalculator.ToMoveCount(yutResult)));

            // 3단계: 이벤트 실행
            yield return StartCoroutine(TriggerEvent(_board.CurrentCell));
        }
    }

    private IEnumerator ThrowYutPhase(Action<YutType> onComplete)
    {
        bool[] results   = new bool[PlayerCount];
        bool[] hasThrown = new bool[PlayerCount];
        int thrownCount  = 0;

        Debug.Log("[YutPhase] 모든 플레이어가 Up 버튼으로 윷을 던지세요! (디버그: 숫자 1~5)");

        while (thrownCount < PlayerCount)
        {
            var debugResult = GetDebugYutInput();
            if (debugResult.HasValue)
            {
                Debug.Log($"[YutPhase] 디버그 입력: {debugResult.Value}");
                onComplete?.Invoke(debugResult.Value);
                yield break;
            }

            for (int i = 0; i < PlayerCount; i++)
            {
                if (!hasThrown[i] && _players[i].Up)
                {
                    results[i]   = UnityEngine.Random.value > 0.5f;
                    hasThrown[i] = true;
                    thrownCount++;
                    Debug.Log($"[YutPhase] Player {i + 1} → {(results[i] ? "앞" : "뒤")}");
                }
            }
            yield return null;
        }

        YutType yut = YutCalculator.Calculate(results);
        Debug.Log($"[YutPhase] 결과: {yut} ({YutCalculator.ToMoveCount(yut)}칸 이동)");
        onComplete?.Invoke(yut);
    }

    private IEnumerator MovePiecePhase(int steps)
    {
        for (int s = 0; s < steps; s++)
        {
            CellInfo current = _board.CurrentCell;

            if (current.nextCells.Count == 0)
            {
                Debug.Log("[MovePhase] 마지막 칸 도달");
                yield break;
            }

            CellInfo next = current.nextCells[0];
            _board.SetCurrentCell(next);
            yield return StartCoroutine(_piece.MoveTo(next.transform.position));
        }
    }

    private IEnumerator TriggerEvent(CellInfo cell)
    {
        IBoardEvent boardEvent = BoardEventFactory.Create(cell.type);
        if (boardEvent != null)
            yield return StartCoroutine(boardEvent.Execute());
    }

    private static YutType? GetDebugYutInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return null;
        if (kb.digit1Key.wasPressedThisFrame) return YutType.Do;
        if (kb.digit2Key.wasPressedThisFrame) return YutType.Gae;
        if (kb.digit3Key.wasPressedThisFrame) return YutType.Geol;
        if (kb.digit4Key.wasPressedThisFrame) return YutType.Yut;
        if (kb.digit5Key.wasPressedThisFrame) return YutType.Mo;
        return null;
    }
}
