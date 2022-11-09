using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // edit on Inspector
    public float maxSpeed;
    public float acceleration;
    public float lerpTime;
    public float maxFireSpeed;

    // player
    public float _speed = 0.0f;

    // Time
    public float _timeElapsed = 0;

    // objects
    CharacterController _characterController;
    PlayerInputActions _playerControls;
    InputAction _move;
    InputAction _fire;
    InputAction _dash;
    InputAction _grab;

    private void Awake()
    {
        _playerControls = new PlayerInputActions();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _move = _playerControls.Player.Move;
        _fire = _playerControls.Player.Fire;
        _dash = _playerControls.Player.Dash;
        _grab = _playerControls.Player.Grab;

        _move.Enable();
        _fire.Enable();
        _dash.Enable();
        _grab.Enable();

        _move.performed += Move;
        _fire.performed += Fire;
        _dash.performed += Dash;
        _grab.performed += Grab;

    }

    private void Move(InputAction.CallbackContext obj)
    {

    }

    private void Grab(InputAction.CallbackContext obj)
    {
    }

    private void Dash(InputAction.CallbackContext obj)
    {
    }

    private void Fire(InputAction.CallbackContext obj)
    {
    }

    private void OnDisable()
    {
        _move.Disable();
        _fire.Disable();
        _dash.Disable();
        _grab.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Only main player actions placed directly on Update()
        // Other actions must be placed as a coroutine.

        MovePlayer();

    }

    private void MovePlayer()
    {
        Vector2 movementInput = _move.ReadValue<Vector2>();
        if (movementInput.normalized.magnitude > 0.1f)
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed > lerpTime) _timeElapsed = lerpTime;
        }
        else
        {
            _timeElapsed = 0;
            //if (_timeElapsed > 0.0f)
            //    _timeElapsed -= Time.deltaTime;
            //if (_timeElapsed < 0.0f) _timeElapsed = 0.0f;
        }

        _speed = Mathf.Lerp(0.0f, maxSpeed,
            _timeElapsed / lerpTime);
        _characterController.Move(movementInput.normalized * _speed * Time.deltaTime);
    }
}
