using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{

#region PublicAndEditorVariables

    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _moveThreshold = 0.1f;
    [SerializeField] private float _jumpDistance = 5f;

    [Header("Attack")]
    [SerializeField] private BoxCollider2D _attackCollider;
    [SerializeField] private float _baseDamage = 5f;
    [SerializeField] private float _heavyDamage = 8f;
    private HitCollider _hitCollider;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.5f;
    [SerializeField] private float _dashCooldown = 1f;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private float _jumpAnimationDuration = 0.2f;

    [Header("Events")]
    public UnityEvent OnCharacterDead;

#endregion PublicAndEditorVariables

#region HealthVariables

    private Health _health;
    private bool _isDead;

#endregion HealthVariables

#region PhysicsVariables

    private Vector2 _moveDirection = Vector2.zero;
    private Rigidbody2D _rigidBody2d;
    private BoxCollider2D _boxCollider2d;
    private int _enviromentLayerMask;
    private float _startGravityScale;

#endregion PhysicsVariables

#region AttackVariables

    private Vector3 _startAttackPosition;
    private bool _isHeavyAttacking;
    private bool _isChargingComplete;

#endregion AttackVariables

#region JumpVariables

    private bool _isJumping;
    private bool _isWallJumping;
    private bool _isGrounded;
    private bool _isCollidingCealing;
    private bool _isCollidingWall;
    private Coroutine _updateJumpStateRoutine;

#endregion Jumpvariables

#region DashVariables

    private bool _canDash = true;
    private bool _isDashing = false;
    private Coroutine _updateDashStateRoutine;

#endregion DashVariables

#region AnimatorHashVariables

    private int _isMovingHash;
    private int _isGroundedHash;
    private int _isJumpingHash;
    private int _isAttackingHash;
    private int _isChargingHash;
    private int _isHeavyAttackingHash;
    private int _isDeadHash;

#endregion AnimationHashVariables

#region UnityLifeCycle

    private void Awake()
    {
        _rigidBody2d = GetComponent<Rigidbody2D>();
        _boxCollider2d = GetComponent<BoxCollider2D>();
        _hitCollider = GetComponentInChildren<HitCollider>();

        _health = GetComponent<Health>();
        _health.OnHealthDeplete.AddListener(OnPlayerDead);
    }

    private void Start()
    {
        _startGravityScale = _rigidBody2d.gravityScale;
        _startAttackPosition = _attackCollider.transform.localPosition;

        _enviromentLayerMask = LayerMask.NameToLayer("Enviroment");

        AssignAnimatorHashes();
    }

    private void Update()
    {
        if (_isDead) return; 

        Move();

        // Checks for character state
        CheckForGround();
        CheckForCealing();
        CheckForWall();

        // Checks for ability states
        CheckForJump();
        CheckForDash();

        UpdateAnimation();
    }

#endregion UnityLifeCycle

#region HealthManagement

    private void OnPlayerDead()
    {
        _isDead = true;
        _animator.SetBool(_isDeadHash, true);

        _rigidBody2d.bodyType = RigidbodyType2D.Static;
        _rigidBody2d.simulated = false;
        _boxCollider2d.enabled = false;

        OnCharacterDead?.Invoke();
    }

#endregion HealthManagement

#region Movement

    private void Move()
    {
        if (_isDashing || _isWallJumping) return;
        
        _rigidBody2d.linearVelocityX = _speed * _moveDirection.x;
    }

    public void SetMoveDirection(Vector2 direction)
    {
        _moveDirection = direction;
    }

#endregion Movement

#region Animation

    private void AssignAnimatorHashes()
    {
        
        _isMovingHash = Animator.StringToHash("IsMoving");
        _isGroundedHash = Animator.StringToHash("IsGrounded");
        _isJumpingHash = Animator.StringToHash("IsJumping");
        _isAttackingHash = Animator.StringToHash("IsAttacking");
        _isChargingHash = Animator.StringToHash("IsCharging");
        _isHeavyAttackingHash = Animator.StringToHash("IsHeavyAttacking");
        _isDeadHash = Animator.StringToHash("IsDead");
    }

    private void UpdateAnimation()
    {
        bool isMoving = Math.Abs(_moveDirection.x) > _moveThreshold;
        _animator.SetBool(_isMovingHash, isMoving);

        if (!isMoving || _isDashing || _isWallJumping) return;

        FlipCharacter();
    }

    private void FlipCharacter()
    {
        bool shouldFlipOnX = _moveDirection.x < 0;
        _spriteRenderer.flipX = shouldFlipOnX;

        FlipAttackCollider(shouldFlipOnX);
    }

    private void FlipAttackCollider(bool shouldFlipOnX)
    {
        float colliderPositionX = FlipAttackPosition(shouldFlipOnX);
        _attackCollider.transform.localPosition = new(colliderPositionX, _startAttackPosition.y, _startAttackPosition.z);
    }

    private float FlipAttackPosition(bool shouldFlipOnX)
    {
        if (shouldFlipOnX) return -_startAttackPosition.x;
        else return _startAttackPosition.x;
    }

#endregion Animation

#region CharacterState

    private void CheckForGround()
    {
        Vector2 colliderSize = new(_boxCollider2d.size.x, 0.5f);
        var raycastHit2D = Physics2D.BoxCast(transform.position, colliderSize, 0f, Vector2.down, 1f, _enviromentLayerMask);

        _isGrounded = raycastHit2D;
        _animator.SetBool(_isGroundedHash, _isGrounded);
    }

    private void CheckForWall()
    {
        Vector2 colliderSize = new(0.5f, _boxCollider2d.size.y);
        Vector2 rayDirection = new(_moveDirection.x, 0);
        var raycastHit2D = Physics2D.BoxCast(transform.position, colliderSize, 0f, rayDirection, 1f, _enviromentLayerMask);

        _isCollidingWall = raycastHit2D;
    }

    private void CheckForCealing()
    {
        Vector2 colliderSize = new(_boxCollider2d.size.x, 0.5f);
        var raycastHit2D = Physics2D.BoxCast(transform.position, colliderSize, 0f, Vector2.up, 1f, _enviromentLayerMask);

        _isCollidingCealing = raycastHit2D;
    }

#endregion CharacterState

#region Attack

    public void Attack()
    {
        if (_isHeavyAttacking) return;

        _hitCollider.SetDamage(_baseDamage);
        _animator.SetBool(_isAttackingHash, true);
    }

    public void HeavyCharge()
    {
        _isChargingComplete = true;
        _animator.SetBool(_isChargingHash, true);
    }

    public void HeavyAttack()
    {
        if (!_isChargingComplete) return;

        _isHeavyAttacking = true;
        _hitCollider.SetDamage(_heavyDamage);
        _animator.SetBool(_isHeavyAttackingHash, true);
    }

    public void DisableAttackAnimation()
    {
        _isHeavyAttacking = false;
        _isChargingComplete = false;

        _animator.SetBool(_isAttackingHash, false); 
        _animator.SetBool(_isHeavyAttackingHash, false);
        _animator.SetBool(_isChargingHash, false);
    }

    public void EnableAttackCollider()
    {
        _attackCollider.enabled = true;
    }

    public void DisableAttackCollider()
    {
        _attackCollider.enabled = false;
    }

#endregion Attack

#region Jump

    public void Jump()
    {
        if (_isCollidingWall && !_isGrounded && _moveDirection != Vector2.zero)
        {
            WallJump();
            return;
        }

        if (!_isGrounded) return;

        _isJumping = true;

        _rigidBody2d.linearVelocityY = _jumpDistance;

        _animator.SetBool(_isJumpingHash, true);
        _trailRenderer.emitting = true;

        _updateJumpStateRoutine = StartCoroutine(UpdateJumpState());
    }

    private void WallJump()
    {
        if (_isCollidingWall && !_isGrounded && _moveDirection != Vector2.zero)
        {
            CancelJump();
            StopCoroutine(_updateJumpStateRoutine);

            _isWallJumping = true;
            _isJumping = true;

            _rigidBody2d.linearVelocityY = _jumpDistance;
            _rigidBody2d.linearVelocityX = _jumpDistance * -_moveDirection.x;

            _spriteRenderer.flipX = !_spriteRenderer.flipX;
            FlipAttackCollider(!_spriteRenderer.flipX);

            _animator.SetBool(_isJumpingHash, true);
            _trailRenderer.emitting = true;

            _updateJumpStateRoutine = StartCoroutine(UpdateJumpState());
        }
    }

    private IEnumerator UpdateJumpState()
    {
        yield return new WaitForSeconds(_jumpAnimationDuration);

        CancelJump();

        if (!_isDashing) _trailRenderer.emitting = false;
    }

    private void CancelJump()
    {
        _isJumping = false;
        _isWallJumping = false;
        _animator.SetBool(_isJumpingHash, false);
    }

    private void CheckForJump()
    {
        if (_isJumping && _isCollidingCealing)
        {
            CancelJump();
            StopCoroutine(_updateJumpStateRoutine);
        }
    }

#endregion Jump

#region Dash

    public void Dash()
    {
        if (!_canDash || _moveDirection.x == 0) return;

        _canDash = false;
        _isDashing = true;

        _rigidBody2d.gravityScale = 0;
        _rigidBody2d.linearVelocity = new Vector2(_moveDirection.x * _dashSpeed, 0);

        _trailRenderer.emitting = true;

        _updateDashStateRoutine = StartCoroutine(UpdateDashState());
    }

    private IEnumerator UpdateDashState()
    {
        yield return new WaitForSeconds(_dashDuration);

        CancelDash();

        yield return new WaitForSeconds(_dashCooldown);

        _canDash = true;
    }

    private void CancelDash()
    {
        _isDashing = false;
        _rigidBody2d.gravityScale = _startGravityScale;
        
        if (!_isJumping) _trailRenderer.emitting = false;
    }

    private void CheckForDash()
    {
        if (_isDashing && _isCollidingWall)
        {
            CancelDash();  
            StopCoroutine(_updateDashStateRoutine);
            _canDash = true;
        }
    }

#endregion Dash

}
