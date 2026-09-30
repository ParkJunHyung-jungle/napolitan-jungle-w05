
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
    public bool InteractPressed => _inputMode == InputMode.Player && PlayerMap.Interact.WasPressedThisFrame();
    public bool JumpPressed => _inputMode == InputMode.Player && PlayerMap.Jump.WasPressedThisFrame();
    public bool JumpHeld => _inputMode == InputMode.Player && PlayerMap.Jump.IsPressed();
    public bool SprintHeld => _inputMode == InputMode.Player && PlayerMap.Sprint.IsPressed();

    [Header("UI Mode")]

    public bool GamePadConnected { get; private set; }

    public void Init()
    {
        _inputActions = new InputSystem_Actions();

        PlayerMap = _inputActions.Player;
        UIMap = _inputActions.UI;

        InputSystem.onActionChange += CheckDeviceType;

        SetInputMode(InputMode.Player);
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

        if (mode == InputMode.Player)
        {
            PlayerMap.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            UIMap.Enable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void CheckDeviceType(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed)
            return;
        InputAction action = obj as InputAction;
        GamePadConnected = action?.activeControl?.device is Gamepad;
    }
}
