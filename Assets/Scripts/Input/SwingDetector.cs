using System;
using UnityEngine;

// JoyConMotion에서 가속도를 읽어 "휘두르기"를 감지한다.
// 같은 GameObject에 붙이거나, 독립 GameObject에 붙여도 됨.
public class SwingDetector : MonoBehaviour
{
    [Header("대상 조이콘")]
    public int playerIndex = 1;
    public char side = 'R'; // 'L' or 'R'

    [Header("감지 설정")]
    [Tooltip("이 가속도(g) 이상이면 스윙으로 판정")]
    public float threshold = 3.0f;

    [Tooltip("스윙 판정 후 재판정까지 대기 시간(초)")]
    public float cooldown = 0.4f;

    // 스윙 발생 시 호출됨. float = 감지 시점의 가속도 크기
    public event Action<int, char, float> OnSwing;

    private float _cooldownTimer;


    void Update()
    {
        if (JoyConMotion.Instance == null) return;

        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
            return;
        }

        Vector3 accel = JoyConMotion.Instance.GetAccel(playerIndex, side);
        float magnitude = accel.magnitude;

        if (magnitude >= threshold)
        {
            _cooldownTimer = cooldown;
            OnSwing?.Invoke(playerIndex, side, magnitude);
            // Debug.Log($"[SwingDetector] P{playerIndex}{side} 스윙! 가속도={magnitude:F2}g");
        }
    }

    // 외부에서 쿨다운 중인지 확인
    public bool IsOnCooldown => _cooldownTimer > 0f;
}
