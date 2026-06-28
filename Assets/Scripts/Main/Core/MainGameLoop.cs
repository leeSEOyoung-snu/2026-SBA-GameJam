using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainGameLoop : MonoBehaviour
{
    private const int PlayerCount = 4;

    public event Action OnGameEnd;

    private IPlayerInputReader[] _players;
    private Board _board;
    private GamePiece _piece;
    private bool _gameEnded;
    private EvolutionCellBehaviour[] _evolutionCells;
    private MainSceneManager _mainSceneManager;
    private YutThrowCanvasController _yutThrowCanvas;
    private EventCanvas _eventCanvas;

    public void Init(Board board, GamePiece piece)
    {
        _board = board;
        _piece = piece;
        
        _mainSceneManager = GetComponent<MainSceneManager>();
        YutThrowCanvasController[] yutThrowCanvases =
            FindObjectsByType<YutThrowCanvasController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _yutThrowCanvas = yutThrowCanvases.Length > 0 ? yutThrowCanvases[0] : null;
        
        EventCanvas[] eventCanvases =
            FindObjectsByType<EventCanvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _eventCanvas = eventCanvases.Length > 0 ? eventCanvases[0] : null;

        _players = new IPlayerInputReader[PlayerCount];
        for (int i = 0; i < PlayerCount; i++)
            _players[i] = GameManager.Instance.GetPlayerInputReader(i + 1);

        _evolutionCells = FindObjectsByType<EvolutionCellBehaviour>(FindObjectsSortMode.None);

        // 미니게임 중에도 이 GameObject는 활성 유지 (코루틴 보호)
        GameManager.Instance.RegisterKeepActive(gameObject);

        _piece.PlaceAt(_board.CurrentCell.transform.position);
        UpdateAllProximityUIs(_board.CurrentCell);

        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        yield return new WaitForSeconds(1f);
        
        while (!_gameEnded)
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
        //if (_yutThrowCanvas != null && _yutThrowCanvas.isActiveAndEnabled)
        if (_yutThrowCanvas != null)
        {
            yield return _yutThrowCanvas.Open().WaitForCompletion();
            yield return StartCoroutine(_yutThrowCanvas.PlayThrowRoutine(_players, onComplete));
            yield return new WaitForSeconds(2f);
            yield return _yutThrowCanvas.Close().WaitForCompletion();
            yield break;
        }

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
        
        //yield return _yutThrowCanvas.Close().WaitForCompletion();
    }

    private IEnumerator MovePiecePhase(int steps)
    {
        // 이동할 칸 목록을 미리 수집
        var cellsToVisit = new System.Collections.Generic.List<CellInfo>();
        CellInfo cursor = _board.CurrentCell;
        for (int s = 0; s < steps; s++)
        {
            if (cursor.nextCells.Count == 0) { Debug.Log("[MovePhase] 마지막 칸 도달"); break; }
            cursor = cursor.nextCells[0];
            cellsToVisit.Add(cursor);
        }
        if (cellsToVisit.Count == 0) yield break;

        // Evolution Cell 기준으로 구간을 나눠 이동
        int segStart = 0;
        while (segStart < cellsToVisit.Count)
        {
            // 다음 Evolution Cell까지(포함) 구간 수집
            var segmentWaypoints = new System.Collections.Generic.List<Vector3>();
            CellInfo segmentEnd = null;
            bool hitEvo = false;

            for (int i = segStart; i < cellsToVisit.Count; i++)
            {
                CellInfo cell = cellsToVisit[i];
                segmentWaypoints.Add(cell.transform.position);
                segmentEnd = cell;

                if (cell.TryGetComponent<EvolutionCellBehaviour>(out _))
                {
                    hitEvo = true;
                    segStart = i + 1;
                    break;
                }

                if (i == cellsToVisit.Count - 1)
                    segStart = cellsToVisit.Count;
            }

            // 구간 연속 이동
            yield return StartCoroutine(_piece.MoveTo(segmentWaypoints.ToArray(), _mainSceneManager.PlayerIdByRanking));

            _board.SetCurrentCell(segmentEnd);
            UpdateAllProximityUIs(segmentEnd);

            if (hitEvo)
                yield return StartCoroutine(_mainSceneManager.GoGoEvolution());

            if (segmentEnd == _board.StartCell)
            {
                Debug.Log("[MovePhase] 시작 칸 도달 → 게임 종료");
                _gameEnded = true;
                OnGameEnd?.Invoke();
                yield break;
            }
        }
    }

    private void UpdateAllProximityUIs(CellInfo from)
    {
        foreach (var evo in _evolutionCells)
            evo.UpdateProximityUI(from);
    }

    private IEnumerator TriggerEvent(CellInfo cell)
    {
        IBoardEvent boardEvent = BoardEventFactory.Create(cell.type, _mainSceneManager, _players);
        bool useEventCanvas = UsesEventCanvas(cell.type) && _eventCanvas != null;

        if (useEventCanvas)
            yield return _eventCanvas.Open(cell.type, _mainSceneManager.StateContainer, boardEvent).WaitForCompletion();

        if (boardEvent != null)
            yield return StartCoroutine(boardEvent.Execute());

        if (useEventCanvas)
            yield return new WaitForSeconds(_eventCanvas.DisplayDuration);

        if (useEventCanvas)
            yield return _eventCanvas.Close().WaitForCompletion();

        // 이벤트 종료 후 순위 비주얼 갱신 (sorting order, alpha, offset)
        _piece.ApplyRanking(_board.CurrentCell.transform.position, _mainSceneManager.PlayerIdByRanking);
    }

    private static bool UsesEventCanvas(CellType cellType)
    {
        switch (cellType)
        {
            case CellType.AffectionSteal:
            case CellType.StateChange:
            case CellType.NightmarePit:
                return true;
            default:
                return false;
        }
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
