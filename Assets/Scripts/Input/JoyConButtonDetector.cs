using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// vJoy 버튼 번호 → 가로 잡기 기준 논리 방향으로 변환해 이벤트를 발행한다.
// 조이스틱 값도 Vector2로 제공한다.
// multi_joycon.py의 Controls 딕셔너리 기준.
public class JoyConButtonDetector : MonoBehaviour
{
    public enum Side { Left, Right }

    [Header("대상 조이스틱")]
    [Tooltip("Joystick.all 인덱스 (0 = 첫 번째 vJoy 디바이스)")]
    public int joystickIndex = 0;
    public Side side = Side.Right;

    // 눌림 이벤트
    public event Action OnUp;
    public event Action OnDown;
    public event Action OnLeft;
    public event Action OnRight;
    public event Action OnSL;
    public event Action OnSR;

    // 떼기 이벤트
    public event Action OnUpReleased;
    public event Action OnDownReleased;
    public event Action OnLeftReleased;
    public event Action OnRightReleased;
    public event Action OnSLReleased;
    public event Action OnSRReleased;

    // 가로 잡기 기준 스틱 입력 (-1 ~ 1)
    public Vector2 StickValue { get; private set; }

    // vJoy 버튼 번호 (multi_joycon.py Controls 기준, 가로 잡기 매핑)
    // Right: Up=Y(8) Down=A(5) Left=X(7) Right=B(6) SL=SLR(16) SR=SRR(15)
    // Left:  Up=Down(10) Down=Up(11) Left=Right(9) Right=Left(12) SL=SLL(17) SR=SRL(18)
    private int _btnUp, _btnDown, _btnLeft, _btnRight, _btnSL, _btnSR;

    private bool _prevUp, _prevDown, _prevLeft, _prevRight, _prevSL, _prevSR;

    public void Init()
    {
        if (side == Side.Right)
        {
            _btnUp = 8; _btnDown = 5; _btnLeft = 6; _btnRight = 7;
            _btnSL = 16; _btnSR = 15;
        }
        else
        {
            _btnUp = 9; _btnDown = 12; _btnLeft = 11; _btnRight = 10;
            _btnSL = 17; _btnSR = 18;
            // _btnUp = 10; _btnDown = 11; _btnLeft = 9; _btnRight = 12;
            // _btnSL = 17; _btnSR = 18;
        }
    }

    void Update()
    {
        if (Joystick.all.Count <= joystickIndex) return;
        var joystick = Joystick.all[joystickIndex];

        UpdateStick(joystick);
        UpdateButtons(joystick);
    }

    void UpdateStick(Joystick joystick)
    {
        float rawX, rawY;

        if (side == Side.Left)
        {
            // Left → HID_USAGE_X / Y (stick 하위에 위치)
            rawX = ReadAxis(joystick, "stick/x");
            rawY = ReadAxis(joystick, "stick/y");
            // 가로 잡기(90° 시계): 물리 X→게임 Y(반전), 물리 Y→게임 X
            StickValue = new Vector2(-rawY, rawX);
        }
        else
        {
            // Right → HID_USAGE_RX / RY
            rawX = ReadAxis(joystick, "rx");
            rawY = ReadAxis(joystick, "ry");
            // 가로 잡기(90° 반시계): 물리 X→게임 Y, 물리 Y→게임 X(반전)
            StickValue = new Vector2(rawY, -rawX);
        }
    }

    float ReadAxis(Joystick joystick, string axisName)
    {
        var control = joystick.TryGetChildControl(axisName) as AxisControl;
        return control?.ReadValue() ?? 0f;
    }

    void UpdateButtons(Joystick joystick)
    {
        bool up    = GetButton(joystick, _btnUp);
        bool down  = GetButton(joystick, _btnDown);
        bool left  = GetButton(joystick, _btnLeft);
        bool right = GetButton(joystick, _btnRight);
        bool sl    = GetButton(joystick, _btnSL);
        bool sr    = GetButton(joystick, _btnSR);

        FireEvents(up,    _prevUp,    OnUp,    OnUpReleased);
        FireEvents(down,  _prevDown,  OnDown,  OnDownReleased);
        FireEvents(left,  _prevLeft,  OnLeft,  OnLeftReleased);
        FireEvents(right, _prevRight, OnRight, OnRightReleased);
        FireEvents(sl,    _prevSL,    OnSL,    OnSLReleased);
        FireEvents(sr,    _prevSR,    OnSR,    OnSRReleased);

        _prevUp = up; _prevDown = down; _prevLeft = left;
        _prevRight = right; _prevSL = sl; _prevSR = sr;
    }

    bool GetButton(Joystick joystick, int vjoyButtonNumber)
    {
        var control = joystick.TryGetChildControl($"button{vjoyButtonNumber}") as ButtonControl;
        return control != null && control.isPressed;
    }

    void FireEvents(bool current, bool previous, Action onPress, Action onRelease)
    {
        if (current && !previous) onPress?.Invoke();
        else if (!current && previous) onRelease?.Invoke();
    }
}
