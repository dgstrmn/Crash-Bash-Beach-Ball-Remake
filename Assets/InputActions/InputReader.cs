using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, GameInput.IVehicleActions, GameInput.IUIActions
{

    private GameInput _gameInput;

    private void OnEnable()
    {
        if(_gameInput == null)
        {
            _gameInput = new GameInput();
            _gameInput.Vehicle.SetCallbacks(this);

            SetVehicle();
        }
    }

    private void SetVehicle()
    {
        _gameInput.Vehicle.Enable();
        _gameInput.UI.Disable();
    }

    private void SetUI()
    {
        _gameInput.UI.Enable();
        _gameInput.Vehicle.Disable();
    }

    public event Action<float> SlideEvent;

    public event Action MagnetEvent;
    public event Action MagnetCancelledEvent;

    public event Action PauseEvent;
    public event Action ResumeEvent;

    public void OnSlide(InputAction.CallbackContext context)
    {
        SlideEvent?.Invoke(context.ReadValue<float>());
    }

    public void OnMagnet(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            MagnetEvent?.Invoke();
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            MagnetCancelledEvent?.Invoke();
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            PauseEvent?.Invoke();
            SetUI();
        }
    }

    public void OnResume(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            ResumeEvent?.Invoke();
            SetVehicle();
        }
    }
}
