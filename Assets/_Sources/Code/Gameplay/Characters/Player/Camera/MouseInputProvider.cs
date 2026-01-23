using Game.Interfaces;
using Sources.Code.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputProvider : ICameraInputProvider
{
    private IInputManager _input;

    public MouseInputProvider(IInputManager input)
    {
        _input = input;
    }

    public Vector2 GetLookDelta()
    {
        if (Mouse.current == null)
            return Vector2.zero;

        if (Cursor.lockState != CursorLockMode.Locked)
            return Vector2.zero;

        return Mouse.current.delta.ReadValue();
    }

    public Vector2 GetMoveInput()
    {
        if (_input == null)
            return Vector2.zero;

        return new Vector2(_input.Horizontal, _input.Vertical);
    }
}
