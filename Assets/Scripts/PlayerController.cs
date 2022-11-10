using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PlayerController : MonoBehaviour
{
    // edit on Inspector
    [Header("Running properties")]
    public float maxSpeed;
    public float lerpTime;

    [Header("Dash properties")]
    public float maxDashTime;
    public float maxDashSpeed;

    [Header("Bullet properties")]
    public GameObject _bullet;
    public float maxBulletSpeed;



    // player
    private float _speed = 0.0f;
    private Vector2 _movementInput = Vector2.zero;
    private bool _isDashing = false;
    private Ray _ray;
    private RaycastHit _hit;

    // Time
    private float _timeElapsed = 0;
    private float _dashingTimeElapsed = 0;

    // objects
    //Gamepad gamepad;
    Camera _cam;
    PlayerInputActions _playerControls;
    InputAction _move;
    InputAction _aim;
    InputAction _fire;
    InputAction _dash;
    InputAction _grab;

    private void Awake()
    {
        _playerControls = new PlayerInputActions();
        _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void OnEnable()
    {
        _move = _playerControls.Player.Move;
        _aim = _playerControls.Player.Aim;
        _fire = _playerControls.Player.Fire;
        _dash = _playerControls.Player.Dash;
        _grab = _playerControls.Player.Grab;

        _move.Enable();
        _aim.Enable();
        _fire.Enable();
        _dash.Enable();
        _grab.Enable();

        _move.performed += Move;
        _aim.performed += Aim;
        _fire.performed += Fire;
        _dash.performed += Dash;
        _grab.performed += Grab;

    }

    private void Aim(InputAction.CallbackContext obj) {
    
    }
    private void Move(InputAction.CallbackContext obj) {
    }
    private void Fire(InputAction.CallbackContext obj) 
    {
        Instantiate(_bullet, transform.position, transform.rotation);
    }
    private void Grab(InputAction.CallbackContext obj) {}
    private void Dash(InputAction.CallbackContext obj)
    {
        if (!_isDashing)
        {
            _dashingTimeElapsed = 0;
            if (_movementInput.magnitude > 0.1f)
                StartDash();
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        transform.Translate(_movementInput.normalized * maxDashSpeed * Time.deltaTime, Space.World);
    }

    private void StopDash()
    {
        _isDashing= false;
        _dashingTimeElapsed = 0;
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
        LineRenderer line = new LineRenderer();
        Vector3[] positions = new Vector3[2];
   }

    // Update is called once per frame
    void Update()
    {
        // Only main player actions placed directly on Update()
        // Other actions must be placed as a coroutine.

        MovePlayer();
        RotatePlayer();

        // Raycast test
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, transform.right);
        Debug.DrawRay(transform.position, 
            transform.right * 20, Color.red);

        if (hitInfo != null)
        {
            Debug.Log(hitInfo.collider.gameObject + " was hit!");
        }
        
    }
    private void RotatePlayer()
    {

       /*** for mouse cursor rotation ***/
       Vector3 position = _cam.ScreenToWorldPoint(new Vector3(_aim.ReadValue<Vector2>().x, _aim.ReadValue<Vector2>().y, 0.0f));
        transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(position.y - transform.position.y,
             position.x - transform.position.x) * Mathf.Rad2Deg);

        /*** for gamepad rotation ***/
        //Vector3 position = _aim.ReadValue<Vector2>().normalized;
        //if (position != Vector3.zero)
        //    transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg);
    }

    private void MovePlayer()
    {
        _movementInput = _move.ReadValue<Vector2>();
        _speed = _isDashing ? ManageDashSpeed() : ManageRunSpeed();
        transform.Translate(_movementInput.normalized * _speed * Time.deltaTime, Space.World);
    }

    private float ManageRunSpeed()
    {
        if (_movementInput.normalized.magnitude > 0.1f)
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed > lerpTime) _timeElapsed = lerpTime;
        }
        else _timeElapsed = 0;

        return Mathf.Lerp(0.0f, maxSpeed,_timeElapsed / lerpTime);
    }

    private float ManageDashSpeed()
    {
        _dashingTimeElapsed += Time.deltaTime;
        if (_dashingTimeElapsed > maxDashTime)
            StopDash();

        return maxDashSpeed;
    }

    public float GetBulletSpeed()
    {
        return maxBulletSpeed;
    }
}
