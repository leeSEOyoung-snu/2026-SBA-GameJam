using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerInputReader
{
    // 이번 프레임에 눌림
    public bool Up { get; }
    public bool Down { get; }
    public bool Left { get; }
    public bool Right { get; }
    public bool SR { get; }
    public bool SL { get; }
    public bool Swing { get; }

    // 현재 누르고 있는 상태
    public bool UpHeld { get; }
    public bool DownHeld { get; }
    public bool LeftHeld { get; }
    public bool RightHeld { get; }
    public bool SRHeld { get; }
    public bool SLHeld { get; }

    public Vector2 Stick { get; }

    public void Reset();
    
    // Activeness
    public void SetActiveInput(bool active);
}

public class PlayerInputProcessor : MonoBehaviour, IPlayerInputReader
{
    #region Input Action Callbacks
    
    [Serializable]
    private struct InputInfo
    {
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;
        public bool SR;
        public bool SL;
        public Vector2 Stick;
        public bool Swing;

        public bool UpHeld;
        public bool DownHeld;
        public bool LeftHeld;
        public bool RightHeld;
        public bool SRHeld;
        public bool SLHeld;
    }

    [SerializeField, ReadOnly] private InputInfo info;

    public bool Up        { get => info.Up;        set => info.Up = value; }
    public bool Down      { get => info.Down;      set => info.Down = value; }
    public bool Left      { get => info.Left;      set => info.Left = value; }
    public bool Right     { get => info.Right;     set => info.Right = value; }
    public bool SR        { get => info.SR;        set => info.SR = value; }
    public bool SL        { get => info.SL;        set => info.SL = value; }
    public bool Swing     { get => info.Swing;     set => info.Swing = value; }
    public Vector2 Stick  { get => info.Stick;     set => info.Stick = value; }

    public bool UpHeld    { get => info.UpHeld;    set => info.UpHeld = value; }
    public bool DownHeld  { get => info.DownHeld;  set => info.DownHeld = value; }
    public bool LeftHeld  { get => info.LeftHeld;  set => info.LeftHeld = value; }
    public bool RightHeld { get => info.RightHeld; set => info.RightHeld = value; }
    public bool SRHeld    { get => info.SRHeld;    set => info.SRHeld = value; }
    public bool SLHeld    { get => info.SLHeld;    set => info.SLHeld = value; }

    #endregion

    [SerializeField] private bool printDebug;
    [Space(10)]
    [SerializeField] private Key upKey;
    [SerializeField] private Key downKey;
    [SerializeField] private Key leftKey;
    [SerializeField] private Key rightKey;
    [SerializeField] private Key slKey;
    [SerializeField] private Key srKey;
    
    private JoyConButtonDetector _buttonDetector;
    private bool _activeInput = true;
    
    public void Init()
    {
        _buttonDetector = GetComponent<JoyConButtonDetector>();
        _buttonDetector.Init();
        
        _buttonDetector.OnUp    += () => { Up    = true; UpHeld    = true; };
        _buttonDetector.OnDown  += () => { Down  = true; DownHeld  = true; };
        _buttonDetector.OnLeft  += () => { Left  = true; LeftHeld  = true; };
        _buttonDetector.OnRight += () => { Right = true; RightHeld = true; };
        _buttonDetector.OnSL    += () => { SL    = true; SLHeld    = true; };
        _buttonDetector.OnSR    += () => { SR    = true; SRHeld    = true; };

        _buttonDetector.OnUpReleased    += () => UpHeld    = false;
        _buttonDetector.OnDownReleased  += () => DownHeld  = false;
        _buttonDetector.OnLeftReleased  += () => LeftHeld  = false;
        _buttonDetector.OnRightReleased += () => RightHeld = false;
        _buttonDetector.OnSLReleased    += () => SLHeld    = false;
        _buttonDetector.OnSRReleased    += () => SRHeld    = false;
    }

    private void Update()
    {
        if (!_activeInput)
        {
            Reset();
            Stick = Vector2.zero;
        }
        
        Stick = _buttonDetector.StickValue;
        GetKeyDown();
        
        if (printDebug)
            PrintInputDebug();
    }

    private void LateUpdate()
    {
        Reset();
    }

    public void GetKeyDown()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[upKey].wasPressedThisFrame)    { Up    = true; UpHeld    = true; }
        if (kb[downKey].wasPressedThisFrame)  { Down  = true; DownHeld  = true; }
        if (kb[leftKey].wasPressedThisFrame)  { Left  = true; LeftHeld  = true; }
        if (kb[rightKey].wasPressedThisFrame) { Right = true; RightHeld = true; }
        if (kb[slKey].wasPressedThisFrame)    { SL    = true; SLHeld    = true; }
        if (kb[srKey].wasPressedThisFrame)    { SR    = true; SRHeld    = true; }

        if (kb[upKey].wasReleasedThisFrame)    UpHeld    = false;
        if (kb[downKey].wasReleasedThisFrame)  DownHeld  = false;
        if (kb[leftKey].wasReleasedThisFrame)  LeftHeld  = false;
        if (kb[rightKey].wasReleasedThisFrame) RightHeld = false;
        if (kb[slKey].wasReleasedThisFrame)    SLHeld    = false;
        if (kb[srKey].wasReleasedThisFrame)    SRHeld    = false;
    }

    private void PrintInputDebug()
    {
        string result = "";
        if (Up) result += "Up  ";
        if (Down) result += "Down  ";
        if (Left) result += "Left ";
        if (Right) result += "Right ";
        if (SR) result += "SR ";
        if (SL) result += "SL ";
        if (Swing) result += "Swing ";
        result += Stick;
        Debug.Log(result);
    }

    public void Reset()
    {
        Up = false;
        Down = false;
        Left = false;
        Right = false;
        SR = false;
        SL = false;
        Swing = false;
        // Stick은 매 Update에서 직접 갱신되므로 Reset 대상에서 제외
    }

    public void SwingDetected()
    {
        Swing = true;
    }
    
    public void SetActiveInput(bool active)
    {
        _activeInput = active;
    }
}
