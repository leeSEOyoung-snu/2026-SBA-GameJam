using UnityEngine;

/// <summary>
/// 플레이어 한 명이 조작하는 시소.
/// SL → 반시계(+Z), SR → 시계(-Z) 회전.
/// 자식 오브젝트(판)가 실제 물리 플랫폼 역할.
/// </summary>
public class RecyclingSeesaw : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 80f;  // 도/초
    [SerializeField] private float maxAngle    = 60f;  // 최대 기울기

    private IPlayerInputReader _input;
    private float _angle; // 현재 Z 각도

    public void Init(int playerId)
    {
        _input = GameManager.Instance.GetPlayerInputReader(playerId);
    }

    private void Update()
    {
        if (_input == null) return;

        float dir = 0f;
        if (_input.SLHeld) dir += 1f;  // 반시계 (+Z)
        if (_input.SRHeld) dir -= 1f;  // 시계 (-Z)

        if (dir == 0f) return;

        _angle += dir * rotateSpeed * Time.deltaTime;
        _angle  = Mathf.Clamp(_angle, -maxAngle, maxAngle);
        transform.rotation = Quaternion.Euler(0f, 0f, _angle);
    }
}
