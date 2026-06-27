using System;
using UnityEngine;

/// <summary>
/// 바닥의 쓰레기 통.
/// Trigger Collider2D를 입구에 배치해 쓰레기 감지.
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<RecyclingTrash>(out var trash)) return;

        bool correct = trash.TrashType == acceptedType;
        _onTrashEntered?.Invoke(correct, trash);
        Destroy(other.gameObject);
    }
}
