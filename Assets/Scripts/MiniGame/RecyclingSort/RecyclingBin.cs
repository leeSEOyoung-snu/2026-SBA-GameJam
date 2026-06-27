using System;
using UnityEngine;

/// <summary>
/// 바닥의 쓰레기 통.
/// 쓰레기가 물리적으로 충돌하면 판별 후 처리.
/// (Trigger 방식 대신 Collision 방식 사용 — 스크립트가 부모에 있어도 정상 동작)
/// </summary>
public class RecyclingBin : MonoBehaviour
{
    [SerializeField] private RecyclingTrashType acceptedType;

    public RecyclingTrashType AcceptedType => acceptedType;

    // true = 정확한 분류, false = 오분류
    private Action<bool, RecyclingTrash> _onTrashEntered;

    public void Init(Action<bool, RecyclingTrash> onTrashEntered)
    {
        _onTrashEntered = onTrashEntered;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.TryGetComponent<RecyclingTrash>(out var trash)) return;

        bool correct = trash.TrashType == acceptedType;
        _onTrashEntered?.Invoke(correct, trash);
        Destroy(col.gameObject); // 맞든 틀리든 항상 제거
    }
}
