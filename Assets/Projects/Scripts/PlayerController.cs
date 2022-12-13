using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    #region Public Properties
    // Edit on Inspector
    [Header("Running properties")]
    public float maxSpeed;
    public float lerpTime;
    [Space(10)]
    [Header("Grab properties")]
    public float grabLength = 5f;
    public Vector2 enemyThrowForce = new Vector2(5f, 0f);
    [Space(10)]
    [Header("Dash properties")]
    public float maxDashTime;
    public float maxDashSpeed;
    [Space(10)]
    [Header("Bullet properties")]
    public GameObject _bullet;
    public float maxBulletSpeed;
    #endregion

    #region Private Properties
    // Player
    private const float MAX_MUZZLE_TIME = 0.1f;
    private float _speed = 0.0f;
    private Vector2 _movementInput = Vector2.zero;
    private Vector2 _aimInput = Vector2.zero;
    private bool _isDashing = false;
    private bool _isGrabbing = false;
    private bool _isDying = false;
    private int _layerMask;  //User layer 2
    private Vector3 _PreviousPosition = Vector3.zero;

    // UI
    private bool _isPaused = false;

    [Header("Player Audio Clips")]
    // Audio clips
    [SerializeField] private AudioClip _fireClip;
    [SerializeField] private AudioClip _dashClip;
    [SerializeField] private AudioClip _deathClip;
    [SerializeField] private AudioClip _hitClip;

    // Delta time
    private float _timeElapsed = 0;
    private float _muzzleCountdown = 0;
    private float _maxDieTime = 1f;

    // Objects
    private GameObject _grabbedEnemy;
    private Camera _cam;
    private PlayerInputActions _playerControls;
    private GameObject _grabPoint;
    private GameObject _weapon;
    private GameObject _body;
    private GameObject _grabAimParticle;
    private GameObject _hitEffect;
    private Transform _muzzle;
    private HealthManager _healthManager;
    private AnimationManager _animManager;
    private GameManager _gameManager;

    private InputAction _move;
    private InputAction _aim;
    private InputAction _fire;
    private InputAction _dash;
    private InputAction _grab;
    private InputAction _pause;
    private InputAction _exit;
    private bool _isGamepad;
    private InputBinding _aimMouseBinding;
    #endregion

    #region Main Methods
    private void Awake()
    {
        _playerControls = new PlayerInputActions();
        _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _weapon = this.transform.Find("Weapon").gameObject;
        _body = this.transform.Find("Body").gameObject;
        _muzzle = _weapon.transform.Find("Muzzle");
        _grabPoint = _weapon.transform.Find("Crosshair").gameObject;
        _animManager = this.GetComponent<AnimationManager>();
        _grabAimParticle = _weapon.transform.Find("GrabParticle").gameObject;
        _hitEffect = this.transform.Find("HitEffect").gameObject;
        _healthManager = this.GetComponent<HealthManager>();
        _gameManager = GameObject.Find("GameSystem").GetComponent<GameManager>();
    }
    public void OnEnable()
    {
        _move = _playerControls.Player.Move;
        _aim = _playerControls.Player.Aim;
        _fire = _playerControls.Player.Fire;
        _dash = _playerControls.Player.Dash;
        _grab = _playerControls.Player.Grab;
        _pause = _playerControls.UI.Pause;
        _exit = _playerControls.UI.Exit;

        _move.Enable();
        _aim.Enable();
        _fire.Enable();
        _dash.Enable();
        _grab.Enable();
        _pause.Enable();
        _exit.Enable();

        _fire.performed += Fire;
        _dash.performed += Dash;
        _grab.performed += Grab;
        _pause.performed += Pause;
        _exit.performed += Exit;
    }
    public void OnDisable()
    {
        _move.Disable();
        _fire.Disable();
        _dash.Disable();
        _grab.Disable();
       // _pause.Disable();
        _exit.Disable();
    }
    void Start()
    {
        _muzzle.gameObject.SetActive(false);
        if (_grabAimParticle != null) _grabAimParticle.SetActive(false);
        if (_hitEffect != null) _hitEffect.SetActive(false);
        // I don't know why, move 1 more bit to left is needed for correct layer mask.
        _layerMask = LayerMask.NameToLayer("Ignore Raycast") << 1;
        _layerMask = ~_layerMask;


    }
    void Update()
    {
        // Only main player actions placed directly on Update()
        // Other actions must be placed as a coroutine.
        _movementInput = _move.ReadValue<Vector2>();
        _aimInput = _aim.ReadValue<Vector2>();
        print(_aimInput);
        _isGrabbing = _grab.IsPressed();

    }
    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
        ManageEnemyGrab();
        if (_animManager != null)
        {
            _animManager.SetAnimationId("isWalking", _move.IsPressed());
            _animManager.SetAnimationId("isDashing", _isDashing);
            if (_isDying)
                _animManager.SetAnimationId("isDying", true);
        }


        if (_muzzleCountdown > 0)
        {
            _muzzleCountdown -= Time.deltaTime;
        }
        else
        {
            _muzzleCountdown = 0;
            _muzzle.gameObject.SetActive(false);
        }

        if (_isDying)
        {
            DeathCycle();
        }
    }

    #endregion

    #region Public Methods
    public float GetBulletSpeed()
    {
        return maxBulletSpeed;
    }
    public void SetDamage(float damage)
    {
        if (!_isDashing)
        {
            if (_animManager != null)
                _animManager.SetAnimationId("isHit", true, 0.1f);
            _hitEffect.SetActive(true);
            Invoke("ClearHitEffect", 0.5f);
            _isDying = _healthManager.SetDamage(damage);
            if(!_isDying) AudioManager.instance.ExecuteSound(_hitClip);
            else AudioManager.instance.ExecuteSound(_deathClip);
        }
    }

    public void OnDeviceChange(PlayerInput pi)
    {
        _isGamepad = pi.currentControlScheme.Equals("Gamepad") ? true : false;
        if (_isGamepad)
        {
            _aimMouseBinding = _aim.ChangeBinding(1).binding;
            _aim.ChangeBinding(1).Erase();
        }
        else
            _aim.AddBinding(_aimMouseBinding);
    }
    #endregion

    #region Private Methods
    private void Fire(InputAction.CallbackContext obj)
    {
        if (!_isGrabbing)
        {
            Instantiate(_bullet, _muzzle.position, _muzzle.transform.rotation);
            _muzzle.gameObject.SetActive(true);
            _muzzleCountdown = MAX_MUZZLE_TIME;
            AudioManager.instance.ExecuteSound(_fireClip);
        }
    }
    private void Dash(InputAction.CallbackContext obj)
    {
        if (!_isDashing)
        {
            if (_movementInput.magnitude > 0.1f)
            {
                AudioManager.instance.ExecuteSound(_dashClip);
                StartDash();

            }
        }
    }
    private void Grab(InputAction.CallbackContext obj)
    {
        // Play grab sound
        _grabPoint.GetComponent<AudioSource>().Play();   
    }
    private void Pause(InputAction.CallbackContext obj)
    {
        if (_gameManager != null)
        {
            _isPaused = !_isPaused;
            _gameManager.PauseGame(_isPaused);
        }
    }
    private void Exit(InputAction.CallbackContext obj)
    {
        _gameManager.ExitToMainMenu();
    }

    private void ManageEnemyGrab()
    {
        Debug.DrawRay(_grabPoint.transform.position, _muzzle.transform.right * grabLength, Color.red);

        if (_isGrabbing)
        {
            if (_grabAimParticle != null)
            {
                _grabAimParticle.SetActive(true);
            }
            if (_grabbedEnemy == null)
            {
                // Raycast config
                RaycastHit2D hitInfo = Physics2D.Raycast(_grabPoint.transform.position, _muzzle.transform.right, grabLength, _layerMask);
                if ((hitInfo) && (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy")))
                {
                    _grabbedEnemy = hitInfo.transform.gameObject;
                    EnemyBehavior enemy = _grabbedEnemy.GetComponent<EnemyBehavior>();

                    if (enemy != null)
                    {
                        // Check if enemy is transformed
                        if (!enemy.DisableHabilities(_muzzle.transform, _grabPoint))
                        {
                            _grabbedEnemy = null;
                            enemy = null;
                        }
                        else
                        {
                            if (_grabAimParticle != null)
                            {
                                _grabAimParticle.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // Stop Grab sound
            _grabPoint.GetComponent<AudioSource>().Stop();

            if (_grabAimParticle != null)
            {
                _grabAimParticle.SetActive(false);
            }
            if (_grabbedEnemy != null)
            {
                EnemyBehavior enemy = _grabbedEnemy.GetComponent<EnemyBehavior>();
                if (enemy != null)
                {
                    enemy.EnableHabilities();
                    _grabbedEnemy = null;
                }
            }
        }
    }
    private void MovePlayer()
    {
        _speed = _isDashing ? maxDashSpeed : ManageRunSpeed();
        GetComponent<Rigidbody2D>().velocity = _movementInput.normalized * _speed;
    }
    private void RotatePlayer()
    {
        Vector3 currentPosition = Vector3.zero;
        Vector3 aimingEuler = Vector3.zero;

        if (_isGamepad)
        {
            /*** for gamepad rotation ***/
            if (Mathf.Abs(_aimInput.normalized.x) > 0.8f || (Mathf.Abs(_aimInput.normalized.y) > 0.8f))
                currentPosition = _aimInput.normalized;
            else
                currentPosition = _PreviousPosition;
            //print(_aimInput);

            if (currentPosition != Vector3.zero)
                aimingEuler = new Vector3(0, 0, Mathf.Atan2(currentPosition.y, currentPosition.x) * Mathf.Rad2Deg);

            _PreviousPosition = currentPosition;
        }
        else
        {
            /*** for mouse cursor rotation ***/
            currentPosition = _cam.ScreenToWorldPoint(new Vector3(_aimInput.x, _aimInput.y, 0.0f));
            aimingEuler = new Vector3(0, 0, Mathf.Atan2(currentPosition.y - transform.position.y,
                 currentPosition.x - transform.position.x) * Mathf.Rad2Deg);
        }

        if ((aimingEuler.z > 90) || (aimingEuler.z < -90))
        {
            _weapon.transform.eulerAngles = new Vector3(0, 180, 180 - aimingEuler.z);
            _body.transform.eulerAngles = new Vector3(0, 180f, 0);
            _weapon.transform.Find("Shotgun").GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
        else
        {
            _weapon.transform.eulerAngles = aimingEuler;
            _body.transform.eulerAngles = new Vector3(0, 0, 0);
            _weapon.transform.Find("Shotgun").GetComponent<SpriteRenderer>().sortingOrder = 2;
        }
    }

    private float ManageRunSpeed()
    {
        if (_movementInput.normalized.magnitude > 0.1f)
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed > lerpTime) _timeElapsed = lerpTime;
        }
        else _timeElapsed = 0;
        return Mathf.Lerp(0.0f, maxSpeed, _timeElapsed / lerpTime);
    }

    private void StartDash()
    {
        _isDashing = true;
        transform.Translate(_movementInput.normalized * maxDashSpeed * Time.deltaTime, Space.World);
        TimerManager.active.AddTimer(maxDashTime, () =>
        {
            StopDash();
        }, false);
    }
    private void StopDash()
    {
        _isDashing = false;
    }
    private void ClearHitEffect()
    {
        _hitEffect.SetActive(false);
    }
    private void DeathCycle()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Rigidbody2D>().simulated = false;
        OnDisable();
        TimerManager.active.AddTimer(_maxDieTime, () =>
        {
            _gameManager.ExitFromPause();
            //SceneManager.LoadScene("Level1");
        }, false);
    }
    #endregion
}
