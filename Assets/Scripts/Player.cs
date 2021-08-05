using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Go to project settings > Physics2D > Enable 'auto sync transformations'
[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
//region-variables
    //region-public
    [Header("Gravity Modifications")]
    [Range (-100, 0)] public float gravity = -97.5f;
    [Range (-100, 0)] public float terminalVelocity = -20.5f;
    [Range(-1,1)] public float hittingCeilingFallSpeed = 0f;
    public bool allowFastFalling = true;
    [Space(10)]

    [Header("Walk Speed")]
    [Range(0.0f, 1.0f)] public float timeScale = 0.8f;
    [Range (0, 25)] public float walkSpeed = 11f;
    [Range (100, 1000)] public float airAccelerationX = 175;
    [Range (100, 1000)] public float groundAccelerationX = 800;
    [Space(10)]

    [Header("Jump Modifications")]
    [Range (1, 10)] public int maxJumps = 1;
    [Range (0, 100)] public float jumpSpeed = 27f;
    [Range (0.0f, 1.0f)] public float secondJumpModifier = 0.65f;
    [Range(0, 10)] public float fallOffJumpSpeed = 4f;
    [Range(0, 10)] public float fallOffWallJumpSpeed = 9f;
    public float jumpGracePeriod = 0.1f;
    [Space(10)]

    [Header("Wall Jump Modifications")]
    public bool wallJumpUnlocked = true;
    public bool canWallSlideDown = true;
    public bool canWallSlideUp = false;
    public bool canCancelDashWithWallJump = true;
    public Vector2 strongWallJumpForce = new Vector2(35.8f, 26f);
    public Vector2 weakWallJumpForce = new Vector2(25f, 27f);
    public float wallSlideSpeedMax = 3f;
    [Space(10)]

    [Header("Dash Modifications")]
    public bool dashUnlocked = true;
    [Range (0,1)] public int maxDashes = 1;
    public Vector2 dashForce = new Vector2(42f, 43f);
    public float dashCoolDownTimerMax = 0.09f;
    [Range(0,1)] public float dashPressThreshold = 0.2f;
    public bool resetDashOnWall = true;
    [Space(10)]

    [Header("Supa Jump Modifications")]
    public bool supaJumpUnlocked = false;
    public bool unlimitedSupaJump = false;
    public float supaJumpSpeed = 30f;
    public int supaJumpCharges = 3;

    [Header("Particle Effects")]
    public GameObject dashParticlePrefab;
    public int dashParticlePoolSize = 3;
    public GameObject collectableParticlePrefab;
    public int collectableParticlePoolSize = 1;
    [Space(10)]

    [Header("Analog Input Modifications")]
    [Range (0,1)] public float horizontalInputMinimumThreshold = 0.25f;
    [Range (0,1)] public float verticalInputMinimumThreshold = 0.25f;
    [Space(10)]

    [Header("Layers")]
    public int killzoneLayer;
    public int abilityLayer;
    [Space(10)]

    [Header("Tags")]
    public string wallJumpTag = "wall_jump";
    public string dashTag = "dash";
    [Space(10)]

    [Header("Unlocked Signs")]
    public GameObject wallJumpUnlockedSign;
    public GameObject dashUnlockedSign;
    public GameObject dashInstructionSign;
    [Space(10)]

    [Header("Audio Clips & Settings")]
    public AudioSource m_audioSource;
    [Range(0,1)]public float effectsVolume = 1.0f;
    public AudioClip collectItem;
    public AudioClip playerJump;
    [Space(10)]

    [HideInInspector]
    public Vector3 respawnPoint;
    //endregion

    //region-private
    [Header("Particle Pool System")]
    private GameObject[] _dashParticlePool;
    private GameObject[] _collectableParticlePool;
    private int _dashParticlePoolIndex;
    private int _collectableParticlePoolIndex;

    [Header("Vectors")]
    private Vector2 _playerInput = Vector2.zero;
    private Vector2 _velocity = Vector2.zero;

    [Header("Timer Counters")]
    private float _dashCooldownTimer = 0;
    private float _jumpGraceTimer = 0;

    [Header("Movement Counters")]
    private int _jumpCount = 0;
    private int _dashCount = 0;

    [Header("Wall Sliding")]
    private int _lastWallDirection = 0;

    [Header("Wall Jumping")]
    private int _wallDirectionX = 0;

    [Header("Booleans")]
    private bool _canMove = true;
    private bool _canDashThisFrame = true;
    private bool _isGrounded = false;
    private bool _isFloating = false;
    private bool _isDashing = false;
    private bool _isSupaJumping = false;
    private bool _playerWasJumping = false;
    private bool _allowFallOffJump = true;
    private bool _isWallJumping = false;
    private bool _isStrongWallJump = false;
    private bool _isWallSliding = false;
    private bool _isCrouching = false;
    private bool _enteredKillzone = false;
    private bool _enableVerticalDashGravity = false;
    private bool _toggleDashParticles = false;

    [Header("Components")]
    private Controller2D m_controller;
    private SpriteRenderer m_sprend;
    private Animator m_animator;
    private InterpolatedTransform m_interpolatedTransform;

    [Header("Scripts")]
    private CharacterInput m_rawInput;

    [HideInInspector]
    public bool freezePlayer = false;
    public List<ParticleSystem> dust;
    Inventory inventory;
    //endregion

//endregion

//region-awake_start_update_fixedUpdate
    private void Awake() {
        Time.timeScale = 0.2f;
        respawnPoint = transform.position;
        // InstantiateParticlePool();
        CacheComponents();
        CacheScripts();
    }

    private void Start() {
        InitializeGameObjects();
    }

    private void Update() {
        Time.timeScale = timeScale;
        m_rawInput.OnUpdate(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Dash"), Input.GetButton("Jump"), Input.GetButton("Fire"), Input.GetButton("Restart"), Input.GetButton("SupaJump"));
    }

    private void FixedUpdate() {
        if (!freezePlayer) {
            StorePlayerInputs();
            GroundCheck();
            CeilingCheck();
            StoreWallDirection();
            if (dashUnlocked) { ManageDash(); }
            ManageWallSlide();
            ManageJump();
            ApplyGravity();
            MovePlayer();

            ManageCooldowns();
            ManageOrientation();
            ManageParticleEffects();
            ManageAnimation();
            ResetPosition();
        }
        m_rawInput.ResetAfterFixedUpdate();
    }
//endregion

//region-awake_functions
    private void InitializeGameObjects() {
        if (wallJumpUnlockedSign)
            wallJumpUnlockedSign.SetActive(false);
        if (dashUnlockedSign)
            dashUnlockedSign.SetActive(false);
        if (dashInstructionSign)
            dashInstructionSign.SetActive(false);
    }

    private void InstantiateParticlePool() {
        _dashParticlePool = new GameObject[dashParticlePoolSize];
        for (int i = 0; i < dashParticlePoolSize; i++) {
            _dashParticlePool[i] = Instantiate(dashParticlePrefab, Vector3.zero, Quaternion.identity);
            _dashParticlePool[i].SetActive(false);
        }
        _dashParticlePoolIndex = 0;

        _collectableParticlePool = new GameObject[collectableParticlePoolSize];
        for (int i = 0; i < collectableParticlePoolSize; i++) {
            _collectableParticlePool[i] = Instantiate(collectableParticlePrefab, Vector3.zero, Quaternion.identity);
            _collectableParticlePool[i].SetActive(false);
        }
        _collectableParticlePoolIndex = 0;
    }

    private void CacheComponents() {
        m_controller = GetComponent<Controller2D>();
        m_animator = GetComponent<Animator>();
        m_sprend = GetComponent<SpriteRenderer>();
        m_interpolatedTransform = GetComponent<InterpolatedTransform>();
    }

    private void CacheScripts() => m_rawInput = new CharacterInput();
//endregion

//region-camera_blend_funcs
    public void FreezePlayer() {
        freezePlayer = true;
        _playerWasJumping = (_velocity.y > 0) ? true : false;
        endDash();
        _enableVerticalDashGravity = false;
    }
    public void UnfreezePlayer() => freezePlayer = false;
    public void FlingPlayerUp() {
        if (_playerWasJumping) {
            _velocity.y = jumpSpeed;
            _allowFallOffJump = false;
            m_controller.Move(_velocity * Time.deltaTime, _isDashing, false, false);
        }
    }
//endregion

//region-fixed_update_functions
    private void StorePlayerInputs() {
        // Store player input as 1.0, -1.0, 0
        _playerInput = Vector2.zero;
        if (Mathf.Abs(m_rawInput.Horizontal) > horizontalInputMinimumThreshold)
            _playerInput.x = 1.0f * Mathf.Sign(m_rawInput.Horizontal);

        if (Mathf.Abs(m_rawInput.Vertical) > verticalInputMinimumThreshold)
            _playerInput.y = 1.0f * Mathf.Sign(m_rawInput.Vertical);
    }

    private void GroundCheck() {
        if (m_controller.collisions.below) { resetOnGround(); }
        else
            _isGrounded = false;
    }

    private void resetOnGround() {
        // Check if we weren't grounded in previous frame
        // If _isGrounded == false, then this means we JUST landed on the ground
        if (_isGrounded == false) {
            // Create dust particle when player lands on the ground
            if (m_controller.collisions.facingRight) {
                CreateDust(0);
            }
            else {
                CreateDust(1);
            }
        }

        _isGrounded = true;
        _isFloating = false;
        _jumpCount = 0;
        _dashCount = 0;
        _velocity.y = 0;
        _jumpGraceTimer = 0;
        _allowFallOffJump = true;
        _isSupaJumping = false;
    }

    private void CeilingCheck() {
        if (!_isGrounded && m_controller.collisions.above)
            _velocity.y = hittingCeilingFallSpeed;
    }

    private void StoreWallDirection() {
        _wallDirectionX = (m_controller.collisions.wallIsLeft ? -1 : (m_controller.collisions.wallIsRight ? 1 : 0) );
    }

    private void ManageDash() {
        if (_wallDirectionX != 0 && resetDashOnWall) { _dashCount = 0; }
        if (_canDashThisFrame && _dashCount < maxDashes && m_rawInput.DashButtonDown) {
            if (_isWallJumping) { endWallJump(); }
            setDashVelocity();
            _toggleDashParticles = true;
            _canDashThisFrame = false;
            _isDashing = true;
            _dashCount++;
        }
    }

    private void setDashVelocity() {
        // New code - horizontal, vertical, and cardinal direction dashing
        _enableVerticalDashGravity = false;

        if (_playerInput.x == 0 && _playerInput.y == 0) {
            int dirX = (m_controller.collisions.facingRight) ? 1 : -1;
            _velocity.x = dashForce.x * dirX;
            _velocity.y = 0;
            return;
        }
        else {
            _velocity.x = _playerInput.x * dashForce.x;
            _velocity.y = _playerInput.y * dashForce.y;
            if (_isGrounded && _playerInput.y == -1)
                _velocity.y = 0f;
            else if (_playerInput.y != 0)
                _enableVerticalDashGravity = true;
        }
    }

    private void ManageWallSlide() {
        checkIfPlayerCanWallSlide();
        if (_isWallSliding) { performWallSlide(); }
    }

    private void checkIfPlayerCanWallSlide() {
        if (!_isDashing && !_isWallJumping && _velocity.y < 0 &&
            _wallDirectionX == _playerInput.x && _playerInput.x != 0 && _playerInput.y == 0) {

            _isWallSliding = true;
            _lastWallDirection = _wallDirectionX;
        } else { _isWallSliding = false; }
    }

    private void performWallSlide() {
        if (canWallSlideDown)
            if (_velocity.y < -wallSlideSpeedMax)
                _velocity.y = -wallSlideSpeedMax;
        if (canWallSlideUp)
            if (_velocity.y > wallSlideSpeedMax)
                _velocity.y += jumpSpeed * Time.deltaTime;
        if (_velocity.x != 0)
            _velocity.x = 0;
    }

    private void ManageJump() {
        if (_jumpGraceTimer < jumpGracePeriod && !_isGrounded)
            _jumpGraceTimer += Time.deltaTime;
        if (m_rawInput.SupaJumpDown && supaJumpUnlocked) {
            onSupaJumpButtonDown();
        }
        if (m_rawInput.JumpDown)
            onJumpButtonDown();
        if (m_rawInput.JumpUp && !_isGrounded &&
            _velocity.y > fallOffJumpSpeed && !_isDashing)
            onJumpButtonUp();
    }

    private void onSupaJumpButtonDown() {
        if (!_isDashing) {
            if (_isGrounded && (supaJumpCharges > 0 || unlimitedSupaJump == true)) {
                supaJump();
                supaJumpCharges--;
                _isSupaJumping = true;
            }
        }
    }

    private void onJumpButtonDown() {
        if (_isSupaJumping)
            return;
        if (wallJumpUnlocked && _wallDirectionX != 0 && !_isGrounded) {
            if (canCancelDashWithWallJump) {
                endDash();
                _enableVerticalDashGravity = false;
                endDashCooldown();
                wallJump();
            }
        }
        else if (!_isDashing) {
            if (_isGrounded) groundJump();
            else if (_jumpCount < maxJumps && _jumpGraceTimer < jumpGracePeriod) midAirJump();
        }
    }

    private void onJumpButtonUp()  {
        if (_isSupaJumping)
            return;
        if (_isWallJumping)
            _velocity.y = fallOffWallJumpSpeed;
        else if (_velocity.y > fallOffJumpSpeed && _allowFallOffJump)
            _velocity.y = fallOffJumpSpeed;
    }

    private void wallJump() {
        if (_isWallJumping)
            endWallJump();
        if (_jumpCount == 0)
            ++_jumpCount;
        if (_playerInput.x == _wallDirectionX) {
            _velocity = weakWallJumpForce;
            _isStrongWallJump = false;
        }
        else {
            _velocity = strongWallJumpForce;
            _isStrongWallJump = true;
        }

        _velocity.x *= -_wallDirectionX;
        _isWallJumping = true;
        _allowFallOffJump = true;
    }

    private void groundJump() {
        _velocity.y = jumpSpeed;
        ++_jumpCount;
        _isGrounded = false;

        // Create Dust Particle
        if (m_controller.collisions.facingRight) {
            CreateDust(0);
        }
        else {
            CreateDust(1);
        }

        // Play Jump sound effect
        if (m_audioSource && playerJump) {
            m_audioSource.PlayOneShot(playerJump, effectsVolume);
        }
    }

    private void midAirJump() {
        _velocity.y = jumpSpeed; //(jumpSpeed - (_jumpCount / jumpSpeed)) * secondJumpModifier; //* 0.75f;
        _jumpCount++;
    }

    private void supaJump() {
        _velocity.y = supaJumpSpeed;
        _jumpCount++;
    }

    private void ApplyGravity() {
        if (_enableVerticalDashGravity) {
            if (_velocity.y > fallOffJumpSpeed) {
                _velocity.y -= airAccelerationX * Time.deltaTime;
            }
            else if (_velocity.y < terminalVelocity) {
                _velocity.y += airAccelerationX * Time.deltaTime;
            }
            else {
                _velocity.y = Mathf.Clamp(_velocity.y, terminalVelocity, fallOffJumpSpeed);
                _enableVerticalDashGravity = false;
                endDash();
            }
        }
        else {
            float lowerTerminalVelocity = 0f;
            if (allowFastFalling) {
                if (!_isGrounded && _playerInput.y < 0 && _playerInput.x == 0) {
                    lowerTerminalVelocity = 10f;
                }
            }
            if (!_isFloating && !_isDashing && _velocity.y > terminalVelocity - lowerTerminalVelocity) {
                _velocity.y += gravity * Time.deltaTime;
            }
            _velocity.y = Mathf.Clamp(_velocity.y, terminalVelocity - lowerTerminalVelocity, 100f);
        }
    }

    //region-timers
    private void endDash() {
        _isDashing = false;
        if (_playerInput.x == 0)
            _velocity.x = 0f;
        else ;
            //_velocity.x = _playerInput.x * walkSpeed;
    }

    private void endWallJump() {
        _isWallJumping = false;
    }

    private void dashCooldownTimer() {
        _dashCooldownTimer += Time.deltaTime;
        if (_dashCooldownTimer > dashCoolDownTimerMax) {
            endDashCooldown();
        }
    }
    private void endDashCooldown() {
        _canDashThisFrame = true;
        _dashCooldownTimer = 0f;
    }

    private void disableMovement() => _canMove = false;
    private void enableMovement() => _canMove = true;
    //endregion

    private void MovePlayer() {
        if (_canMove) {
            if (_isWallJumping || (_isDashing && _velocity.x != 0)) {
                // So basically this is me trying to implement "air friction"
                // Maybe try changing walkSpeed here to something else as an upper bound
                if (_velocity.x > walkSpeed) {
                    _velocity.x -= airAccelerationX * Time.deltaTime;
                }
                else if (_velocity.x < -walkSpeed) {
                    _velocity.x += airAccelerationX * Time.deltaTime;
                }
                else {
                    _velocity.x = Mathf.Clamp(_velocity.x, -walkSpeed, walkSpeed);
                    _isWallJumping = false;
                    if (!_enableVerticalDashGravity)
                        endDash();
                }
            }
            else if (_playerInput.x == 0) {
                // Slow player down to 0
                if (_velocity.x > 0) {
                    if (_isGrounded) {
                        _velocity.x -= groundAccelerationX * Time.deltaTime;
                    }
                    else {
                        _velocity.x -= airAccelerationX * Time.deltaTime;
                    }
                    if (_velocity.x < 0) {
                        _velocity.x = 0;
                    }
                }
                if (_velocity.x < 0) {
                    if (_isGrounded)
                        _velocity.x += groundAccelerationX * Time.deltaTime;
                    else
                        _velocity.x += airAccelerationX * Time.deltaTime;
                    if (_velocity.x > 0)
                        _velocity.x = 0;
                }
            }
            else if (!_isWallSliding && !_enableVerticalDashGravity) {
                if (_velocity.x < walkSpeed || _velocity.x > -walkSpeed) {
                    if (_isGrounded)
                        _velocity.x += _playerInput.x * groundAccelerationX * Time.deltaTime;
                    else
                        _velocity.x += _playerInput.x * airAccelerationX * Time.deltaTime;
                    _velocity.x = Mathf.Clamp(_velocity.x, -walkSpeed, walkSpeed);
                }
            }
        }

        bool ignoreLeftWallDetectionRays = (_jumpGraceTimer < jumpGracePeriod && _velocity.x > 0);
        bool ignoreRightWallDetectionRays = (_jumpGraceTimer < jumpGracePeriod && _velocity.x < 0);

        m_controller.Move(_velocity * Time.deltaTime, _isDashing, _playerInput.y < 0, ignoreLeftWallDetectionRays, ignoreRightWallDetectionRays);
    }

    private void ManageCooldowns() {
        if (!_isDashing && !_canDashThisFrame) {
            dashCooldownTimer();
        }
    }

    private void ManageOrientation() {
        // flipX = false => sprite is facing right
        // flipX = true => sprite is facing left
        bool spriteFacingRight = (m_sprend.flipX == false);
        if (!_isWallJumping || (_isWallJumping && _isStrongWallJump)) {
            if ((spriteFacingRight && (_velocity.x < 0f || (_isWallSliding && _playerInput.x < 0))) ||
                (!spriteFacingRight && (_velocity.x > 0f || (_isWallSliding && _playerInput.x > 0)))) {
                flipPlayerSprite();
                flipParticleSystem();
                m_controller.SetFacingRight(!m_sprend.flipX);
                if (m_controller.collisions.facingRight) {
                    if (_isGrounded) CreateDust(0);
                }
                else {
                    if (_isGrounded) CreateDust(1);
                }
            }
        }

    }

    void CreateDust(int index)
    {
        if (index < dust.Count && dust[index] != null)
        {
            dust[index].Play();
        }
    }

    private void flipPlayerSprite() {
        m_sprend.flipX = !m_sprend.flipX;
    }

    private void flipParticleSystem() {
        GameObject dashParticleSpawner = transform.GetChild(0).gameObject;
        if (dashParticleSpawner) {
            dashParticleSpawner.transform.localRotation = Quaternion.Euler(0, 0, (dashParticleSpawner.transform.localEulerAngles.z + 180) % 360);
            dashParticleSpawner.transform.localPosition = new Vector3(dashParticleSpawner.transform.localPosition.x * -1f, dashParticleSpawner.transform.localPosition.y, dashParticleSpawner. transform.localPosition.z);
        }
    }

    private void ManageParticleEffects() {
        if (_toggleDashParticles) {
            // activateDashParticleEffect();

            if (m_controller.collisions.facingRight) {
                CreateDust(0);
            }
            else {
                CreateDust(1);
            }
            _toggleDashParticles = false;
        }
    }

    private void activateDashParticleEffect() {
        GameObject dashParticleSpawner = transform.GetChild(0).gameObject;
        if (dashParticleSpawner) {
            _dashParticlePool[_dashParticlePoolIndex].SetActive(false);
            _dashParticlePool[_dashParticlePoolIndex].transform.position = dashParticleSpawner.transform.position;
            _dashParticlePool[_dashParticlePoolIndex].transform.rotation = dashParticleSpawner.transform.rotation;
            _dashParticlePool[_dashParticlePoolIndex].SetActive(true);
            _dashParticlePoolIndex = (_dashParticlePoolIndex + 1) % dashParticlePoolSize;
        }
    }

    private void ManageAnimation() {
        m_animator.SetFloat("velocityX", Mathf.Abs (_velocity.x));
        m_animator.SetFloat("velocityY", _velocity.y);
        m_animator.SetFloat("inputX", Mathf.Abs (_playerInput.x));
        m_animator.SetBool("isGrounded", m_controller.collisions.below);
        m_animator.SetBool("crouching", _isGrounded && _playerInput.y < -0.5);
        m_animator.SetBool("dashing", _isDashing);
    }

    private void ResetPosition() {
        if (m_rawInput.RestartDown)
            Respawn();
        else if (_enteredKillzone) {
            m_interpolatedTransform.enabled = false;
            Respawn();
            m_interpolatedTransform.enabled = true;
            _enteredKillzone = false;
        }
    }

    public void Respawn() => transform.position = respawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If we collide with door trigger we call its open function and pass in player orientation
        if (collision.CompareTag("door"))
        {
            bool unlockedDoor = collision.gameObject.GetComponent<Door>()
                .OpenDoor(m_controller.collisions.facingRight, (inventory.GetKeyCount() > 0));
            if (unlockedDoor) {
                inventory.DecrementKeyCount();
            }
        }
        if (collision.CompareTag("key"))
        {
            inventory.IncrementKeyCount();
            collision.gameObject.SetActive(false);
        }
        if (collision.CompareTag("collectable"))
        {
            if (m_audioSource != null && collectItem != null) {
                m_audioSource.PlayOneShot(collectItem, effectsVolume);
            }
            collision.gameObject.SetActive(false);
        }
        // This should happen before checking for hazards
        if (collision.CompareTag("checkpoint")) {
            respawnPoint = collision.gameObject.transform.position;
        }
        if (collision.CompareTag("hazard")) {
            _enteredKillzone = true;
        }
        if (collision.CompareTag("button")) {
            // Only activate button if player is dashing downwards at it
            if (collision.gameObject.GetComponent<Button>().isActivated == false && _isDashing && _velocity.y < 0) {
                collision.gameObject.GetComponent<Button>().ToggleButtonDown(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // If we leave door trigger we call its close function
        if (collision.CompareTag("door"))
        {
            collision.gameObject.GetComponent<Door>()
                .CloseDoor();
        }
    }

    // TODO: Move this to its own serializable class
    struct Inventory {
        public int numKeys;

        // Constructor
        public void Initialize() {
            numKeys = 0;
        }

        // Methods
        public void IncrementKeyCount()
        {
            numKeys++;
        }

        public void DecrementKeyCount() {
            if (numKeys > 0)
                numKeys--;
        }

        public int GetKeyCount() {
            return numKeys;
        }
    }
}
