using System;
using UnityEngine;

/// <summary>
/// 떨어지는 쓰레기 아이템.
/// Rigidbody2D + Collider2D 필요 (gravityScale > 0).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class RecyclingTrash : MonoBehaviour
{
    public RecyclingTrashType TrashType { get; private set; }

    // 화면 밖으로 벗어났을 때 호출 (실수 처리)
    private Action<RecyclingTrash> _onFellOff;

    [SerializeField] private float killY = -6f; // 이 Y 아래로 내려가면 화면 밖

    public void Init(RecyclingTrashType type, Action<RecyclingTrash> onFellOff)
    {
        TrashType = type;
        _onFellOff = onFellOff;
    }

    private void Update()
    {
        if (transform.position.y < killY)
        {
            _onFellOff?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
