using UnityEngine;

// 크로스헤어 공통 베이스 — 입력 초기화, 이동, 맵 경계 클램프
public abstract class CrosshairBase : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 6f;
    [SerializeField] protected Vector2 mapMin = new(-8f, -4.5f);
    [SerializeField] protected Vector2 mapMax = new(8f, 4.5f);

    protected IPlayerInputReader Input { get; private set; }
    protected bool Initialized { get; private set; }

    public virtual void Init(int playerId)
    {
        Input = GameManager.Instance.GetPlayerInputReader(playerId);
        Initialized = true;
    }

    private void Update()
    {
        if (!Initialized) return;
        MoveCrosshair();
        OnUpdate();
    }

    private void MoveCrosshair()
    {
        var dir = new Vector2(Input.Stick.x, Input.Stick.y);

        var pos = (Vector2)transform.position + dir * (moveSpeed * Time.deltaTime);
        pos.x = Mathf.Clamp(pos.x, mapMin.x, mapMax.x);
        pos.y = Mathf.Clamp(pos.y, mapMin.y, mapMax.y);
        transform.position = pos;
    }

    // 파생 클래스에서 게임별 로직 추가 (발사, 상호작용 등)
    protected virtual void OnUpdate() { }
}
