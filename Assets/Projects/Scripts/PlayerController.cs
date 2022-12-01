using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
    private const float MAX_MUZZLE_TIME = 0.1f;

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
    private bool _isMakingDash = false;
    private bool _isGrabbing = false;
    private int _layerMask;  //User layer 2

    // Time
    private float _timeElapsed = 0;
    private float _dashingTimeElapsed = 0;
    private float _muzzleCountdown = 0;

    //Animation IDs
    private int isWalking;
    private int _isDying;
    private int isDashing;

    // objects
    //Gamepad gamepad;
    GameObject _grabbedEnemy;
    Camera _cam;
    PlayerInputActions _playerControls;
    GameObject _grabPoint;
    GameObject _weapon;
    GameObject _body;
    Transform _muzzle;
    Animator _anim;
    InputAction _move;
    InputAction _aim;
    InputAction _fire;
    InputAction _dash;
    InputAction _grab;

    private void Awake()
    {
        _playerControls = new PlayerInputActions();
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
    private void Aim(InputAction.CallbackContext obj) {}
    private void Move(InputAction.CallbackContext obj) {}
    private void Grab(InputAction.CallbackContext obj) {}
    private void Fire(InputAction.CallbackContext obj)
    {
        if (!_isGrabbing)
        {
            Instantiate(_bullet, _muzzle.position, _muzzle.transform.rotation);
            _muzzle.gameObject.SetActive(true);
            _muzzleCountdown = MAX_MUZZLE_TIME;
        }
    }
    private void Dash(InputAction.CallbackContext obj)
    {
        print("Dashing!");

        if (!_isMakingDash)
        {
            _dashingTimeElapsed = 0;
            if (_movementInput.magnitude > 0.1f)
                StartDash();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _weapon = transform.Find("Weapon").gameObject;
        _body = transform.Find("Body").gameObject;
        _muzzle = _weapon.transform.Find("Muzzle");
        _muzzle.gameObject.SetActive(false);
        _grabPoint = _weapon.transform.Find("Crosshair").gameObject;
        _anim = transform.Find("Body").GetComponent<Animator>();

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
        ManageEnemyGrab();
        ManageAnimation();
    }
    private void FixedUpdate()
    {
        if (_muzzleCountdown > 0)
        {
            _muzzleCountdown -= Time.deltaTime;
        }
        else
        {
            _muzzleCountdown = 0;
            _muzzle.gameObject.SetActive(false);
        }
    }
    private void ManageAnimation()
    {
        if (_anim != null)
        {
            _anim.SetBool("isWalking", _move.IsPressed());
            _anim.SetBool("isDashing", false);
        }

    }
    private void ManageEnemyGrab()
    {
        Debug.DrawRay(_grabPoint.transform.position, _muzzle.transform.right * grabLength, Color.red);
        _isGrabbing = _grab.IsPressed();

        if (_isGrabbing)
        {
            if (_grabbedEnemy == null)
            {
                print("Is grabbing");
                // Raycast config
                RaycastHit2D hitInfo = Physics2D.Raycast(_grabPoint.transform.position, _muzzle.transform.right, grabLength, _layerMask);
                if ((hitInfo) && (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy")))
                {
                    _grabbedEnemy = hitInfo.transform.gameObject;
                    EnemyBehavior enemy = _grabbedEnemy.GetComponent<EnemyBehavior>();
                    // Check if invencible enemy is enabled

                    if ((enemy != null) && (!enemy._isTransformed))
                    {
                        enemy.DisableHabilities(_muzzle.transform, _grabPoint);
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
                _grabbedEnemy = null;
            }
        }
    }
    private void RotatePlayer()
    {
        /*** for mouse cursor rotation ***/
        Vector3 position = _cam.ScreenToWorldPoint(new Vector3(_aim.ReadValue<Vector2>().x, _aim.ReadValue<Vector2>().y, 0.0f));
        Vector3 aimingEuler = new Vector3(0, 0, Mathf.Atan2(position.y - transform.position.y,
             position.x - transform.position.x) * Mathf.Rad2Deg);

        /*** for gamepad rotation ***/
        //Vector3 position = _aim.ReadValue<Vector2>().normalized;
        //if (position != Vector3.zero)
        //    aimingEuler = new Vector3(0, 0, Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg);

        if ((aimingEuler.z > 90) || (aimingEuler.z < -90))
        {
            _weapon.transform.eulerAngles = new Vector3(0, 180, 180 - aimingEuler.z);
            _body.transform.eulerAngles = new Vector3(0, 180f, 0);
            //_body.GetComponent<SpriteRenderer>().flipX = true;
            print(_body.transform.eulerAngles);
            _weapon.transform.Find("Shotgun").GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
        else
        {
            _weapon.transform.eulerAngles = aimingEuler;
            _body.transform.eulerAngles = new Vector3(0, 0, 0);
            //_body.GetComponent<SpriteRenderer>().flipX = false;

            print(_body.transform.eulerAngles);
            _weapon.transform.Find("Shotgun").GetComponent<SpriteRenderer>().sortingOrder = 2;
        }
    }
    private void MovePlayer()
    {
        _movementInput = _move.ReadValue<Vector2>();
        _speed = _isMakingDash ? ManageDashSpeed() : ManageRunSpeed();
        GetComponent<Rigidbody2D>().velocity = _movementInput.normalized * _speed;
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

    private void StartDash()
    {
        _isMakingDash = true;
        transform.Translate(_movementInput.normalized * maxDashSpeed * Time.deltaTime, Space.World);
    }
    private void StopDash()
    {
        _isMakingDash = false;
        _dashingTimeElapsed = 0;
    }
    public float GetBulletSpeed()
    {
        return maxBulletSpeed;
    }
}
