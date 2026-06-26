using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerInputReader
{
    public bool Up { get; }
    public bool Down { get; }
    public bool Left { get; }
    public bool Right { get; }
    public bool SR { get; }
    public bool SL { get; }
    public Vector2 Stick { get; }
    public bool Swing { get; }

    public void Reset();
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
    }
    
    [SerializeField, ReadOnly] private InputInfo info;

    public bool Up
    {
        get => info.Up;
        set => info.Up = value;
    }

    public bool Down
    {
        get => info.Down;
        set => info.Down = value;
    }

    public bool Left
    {
        get => info.Left;
        set => info.Left = value;
    }

    public bool Right
    {
        get => info.Right;
        set => info.Right = value;
    }

    public bool SR
    {
        get => info.SR;
        set => info.SR = value;
    }

    public bool SL
    {
        get => info.SL;
        set => info.SL = value;
    }

    public Vector2 Stick
    {
        get => info.Stick;
        set => info.Stick = value;
    }

    public bool Swing
    {
        get => info.Swing;
        set => info.Swing = value;
    }
    
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
    
    void OnEnable()
    {
        _buttonDetector = GetComponent<JoyConButtonDetector>();
        
        _buttonDetector.OnUp += () => Up = true;
        _buttonDetector.OnDown += () => Down = true;
        _buttonDetector.OnLeft += () => Left = true;
        _buttonDetector.OnRight += () => Right = true;
        _buttonDetector.OnSL += () => SL = true;
        _buttonDetector.OnSR += () => SR = true;
    }

    private void Update()
    {
        Stick = _buttonDetector.StickValue;
        GetKeyDown();
        
        if (printDebug)
            PrintInputDebug();

        Reset();
    }

    public void GetKeyDown()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb[upKey].wasPressedThisFrame)    Up    = true;
        if (kb[downKey].wasPressedThisFrame)  Down  = true;
        if (kb[leftKey].wasPressedThisFrame)  Left  = true;
        if (kb[rightKey].wasPressedThisFrame) Right = true;
        if (kb[slKey].wasPressedThisFrame)    SL    = true;
        if (kb[srKey].wasPressedThisFrame)    SR    = true;
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
        Stick = Vector2.zero;
        Swing = false;
    }

    public void SwingDetected()
    {
        Swing = true;
    }
}
