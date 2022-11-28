using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
    // edit on Inspector
    [Header("Running properties")]
    public float maxSpeed;
    public float lerpTime;

    [Header("Grab properties")]
    public float grabLength = 5f;
    public Vector2 enemyThrowForce = new Vector2(5f, 0f); 

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
    private bool _isGrabbing = false;
    private int _layerMask;  //User layer 2

    // Time
    private float _timeElapsed = 0;
    private float _dashingTimeElapsed = 0;

    // objects
    //Gamepad gamepad;
    GameObject _grabbedEnemy;
    Camera _cam;
    PlayerInputActions _playerControls;
    GameObject _grabPoint;
    InputAction _move;
    InputAction _aim;
    InputAction _fire;
    InputAction _dash;
    InputAction _grab;

    private void Awake()
    {
        _playerControls = new PlayerInputActions();
        _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _grabPoint = GameObject.Find("GrabPoint");
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
        // I don't know why, move 1 more bit to left is needed for correct layer mask.
        _layerMask = LayerMask.NameToLayer("Ignore Raycast") << 1;
        _layerMask = ~_layerMask;
    }

    // Update is called once per frame
    void Update()
    {
        // Only main player actions placed directly on Update()
        // Other actions must be placed as a coroutine.
        MovePlayer();
        RotatePlayer();     
        GrabEnemyManagement();
    }

    private void GrabEnemyManagement()
    {
        Debug.DrawRay(transform.position, transform.right * grabLength, Color.red);
        _isGrabbing = _grab.IsPressed();
        if (_isGrabbing)
        {
            if (_grabbedEnemy == null)
            {
                // Raycast config
                RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, transform.right, grabLength, _layerMask);
                if ((hitInfo) && (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy")))
                {
                    _grabbedEnemy = hitInfo.transform.gameObject;
                    EnemyBehavior enemy = _grabbedEnemy.GetComponent<EnemyBehavior>();
                    if (enemy != null)
                    {
                        enemy.DisableHabilities(transform, _grabPoint);
                        _grabbedEnemy.GetComponent<SpriteRenderer>().color = Color.red;
                        Debug.Log(hitInfo.transform.tag + " was hit!");
                    }
                }
            }

        }
        else if (_grabbedEnemy != null)
        {
            EnemyBehavior enemy = _grabbedEnemy.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.EnableHabilities();
                _grabbedEnemy.GetComponent<SpriteRenderer>().color = Color.blue;
                _grabbedEnemy = null;
            }
        }

    }

    private void Aim(InputAction.CallbackContext obj){}
    private void Move(InputAction.CallbackContext obj){}
    private void Grab(InputAction.CallbackContext obj){}
    private void Fire(InputAction.CallbackContext obj)
    {
        Instantiate(_bullet, transform.position, transform.rotation);
    }
    private void Dash(InputAction.CallbackContext obj)
    {
        if (!_isDashing)
        {
            _dashingTimeElapsed = 0;
            if (_movementInput.magnitude > 0.1f)
                StartDash();
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

    private void GrabManagement(RaycastHit2D hitInfo)
    {

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
    private void StartDash()
    {
        _isDashing = true;
        transform.Translate(_movementInput.normalized * maxDashSpeed * Time.deltaTime, Space.World);
    }

    private void StopDash()
    {
        _isDashing = false;
        _dashingTimeElapsed = 0;
    }
}
