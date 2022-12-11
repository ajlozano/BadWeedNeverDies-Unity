using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private const float MAX_MUZZLE_TIME = 0.1f;

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

    // Player
    private float _speed = 0.0f;
    private Vector2 _movementInput = Vector2.zero;
    private Vector2 _aimInput = Vector2.zero;
    private bool _isDashing = false;
    private bool _isGrabbing = false;
    private bool _isDying = false   ;
    private int _layerMask;  //User layer 2

    // Delta time
    private float _timeElapsed = 0;
    private float _dashingTimeElapsed = 0;
    private float _muzzleCountdown = 0;
    private float _dieTimeElapsed = 0;
    private float _maxDieTime = 1f;

    // Objects
    //Gamepad gamepad;
    GameObject _grabbedEnemy;
    Camera _cam;
    PlayerInputActions _playerControls;
    GameObject _grabPoint;
    GameObject _weapon;
    GameObject _body;
    GameObject _grabAimParticle;
    GameObject _hitEffect;

    Transform _muzzle;
    Animator _anim;
    InputAction _move;
    InputAction _aim;
    InputAction _fire;
    InputAction _dash;
    InputAction _grab;
    HealthManager _healthManager;
    AnimationManager _animManager;
    private void Awake()
    {
        _playerControls = new PlayerInputActions();
        _cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _weapon = this.transform.Find("Weapon").gameObject;
        _body = this.transform.Find("Body").gameObject;
        _muzzle = _weapon.transform.Find("Muzzle");
        _muzzle.gameObject.SetActive(false);
        _grabPoint = _weapon.transform.Find("Crosshair").gameObject;
        _animManager = this.GetComponent<AnimationManager>();
        _grabAimParticle = _weapon.transform.Find("GrabParticle").gameObject;
        _hitEffect = this.transform.Find("HitEffect").gameObject;
        _healthManager = this.GetComponent<HealthManager>();
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

        _fire.performed += Fire;
        _dash.performed += Dash;
    }
    private void OnDisable()
    {
        _move.Disable();
        _fire.Disable();
        _dash.Disable();
        _grab.Disable();
    }
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
        if (!_isDashing)
        {
            _dashingTimeElapsed = 0;
            if (_movementInput.magnitude > 0.1f)
                StartDash();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (_grabAimParticle != null)   _grabAimParticle.SetActive(false);
        if (_hitEffect != null) _hitEffect.SetActive(false);
        //_grabEnemyParticle = this.transform.Find("ElectricGrab").gameObject;
        // I don't know why, move 1 more bit to left is needed for correct layer mask.
        _layerMask = LayerMask.NameToLayer("Ignore Raycast") << 1;
        _layerMask = ~_layerMask;


    }
    // Update is called once per frame
    void Update()
    {
        // Only main player actions placed directly on Update()
        // Other actions must be placed as a coroutine.
        _movementInput = _move.ReadValue<Vector2>();
        _aimInput = _aim.ReadValue<Vector2>();
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
        _speed = _isDashing ? ManageDashSpeed() : ManageRunSpeed();
        GetComponent<Rigidbody2D>().velocity = _movementInput.normalized * _speed;
    }
    private void RotatePlayer()
    {
        /*** for mouse cursor rotation ***/
        Vector3 position = _cam.ScreenToWorldPoint(new Vector3(_aimInput.x, _aimInput.y, 0.0f));
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
        _isDashing = true;
        transform.Translate(_movementInput.normalized * maxDashSpeed * Time.deltaTime, Space.World);
    }
    private void StopDash()
    {
        _isDashing = false;
        _dashingTimeElapsed = 0;
    }
    public float GetBulletSpeed()
    {
        return maxBulletSpeed;
    }

    public void SetDamage(float damage)
    {
        if (!_isDashing)
        {
            _animManager.SetAnimationId("isHit", true, 0.1f);
            _hitEffect.SetActive(true);
            Invoke("ClearHitEffect", 0.5f);
            _isDying = _healthManager.SetDamage(damage);
        }
    }

    void ClearHitEffect()
    {
        _hitEffect.SetActive(false);
    }
    //void ClearHitAnimationID()
    //{
    //    _anim.SetBool("isHit", false);
    //}
    private void DeathCycle()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
        OnDisable();

        _dieTimeElapsed += Time.deltaTime;
        if (_dieTimeElapsed > _maxDieTime)
        {
            SceneManager.LoadScene("Level1");
        }
    }
}
