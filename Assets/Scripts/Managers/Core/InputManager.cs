
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputMode
{
    Player,
    UI
}

public class InputManager
{
    [Header("Input System")]
    private InputSystem_Actions _inputActions;
    public InputSystem_Actions.PlayerActions PlayerMap;
    public InputSystem_Actions.UIActions UIMap;
    private InputMode _inputMode = InputMode.Player;

    [Header("Player Mode")]
    public Vector2 MoveInput => _inputMode == InputMode.Player ? PlayerMap.Move.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 LookInput => _inputMode == InputMode.Player ? PlayerMap.Look.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 PointerPosition => _inputMode == InputMode.Player
        ? PlayerMap.Point.ReadValue<Vector2>()
        : UIMap.Point.ReadValue<Vector2>();
    public bool InteractPressed => _inputMode == InputMode.Player
        && (PlayerMap.Interact.WasPressedThisFrame() || PlayerMap.Click.WasPressedThisFrame());
    public bool InteractHeld => _inputMode == InputMode.Player
        && (PlayerMap.Interact.IsPressed() || PlayerMap.Click.IsPressed());
    public bool EscapePressed => _inputMode == InputMode.Player
        ? PlayerMap.Escape.WasPressedThisFrame()
        : UIMap.Cancel.WasPressedThisFrame();
    public bool ClickPressed => _inputMode == InputMode.Player
        ? PlayerMap.Click.WasPressedThisFrame()
        : UIMap.Click.WasPressedThisFrame();
    public bool ConfirmPressed => _inputMode == InputMode.UI && UIMap.Submit.WasPressedThisFrame();

    public void Init()
    {
        _inputActions = new InputSystem_Actions();

        PlayerMap = _inputActions.Player;
        UIMap = _inputActions.UI;

        SetInputMode(InputMode.UI);
    }

    public void Clear()
    {

    }

    public void SetInputMode(InputMode mode)
    {
        _inputMode = mode;

        if (_inputActions == null)
            return;

        PlayerMap.Disable();
        UIMap.Disable();

        Debug.Log(mode.ToString());

        if (mode == InputMode.Player)
        {
            PlayerMap.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
        else
        {
            UIMap.Enable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
    }
}
