using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _moveThreshold = 0.1f;
    [SerializeField] private float _jumpDistance = 5f;

    [Header("Attack")]
    [SerializeField] private GameObject _attackCollider;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.5f;
    [SerializeField] private float _dashCooldown = 1f;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private float _jumpAnimationDuration = 0.2f;

    private Health _health;
    private bool _isDead;

    private Vector2 _moveDirection = Vector2.zero;
    private Rigidbody2D _rigidBody2d;
    private BoxCollider2D _boxCollider2d;
    private float _startGravityScale;

    private Vector3 _startAttackPosition;
    private bool _isHeavyAttacking;
    private bool _isChargingComplete;

    private bool _isGrounded;
    private bool _isJumping;
    private bool _isCollidingWall;

    private bool _canDash = true;
    private bool _isDashing = false;

    private int _isMovingHash;
    private int _isGroundedHash;
    private int _isJumpingHash;
    private int _isAttackingHash;
    private int _isChargingHash;
    private int _isHeavyAttackingHash;
    private int _isDeadHash;

    private void Awake()
    {
        _rigidBody2d = GetComponent<Rigidbody2D>();
        _boxCollider2d = GetComponent<BoxCollider2D>();

        _health = GetComponent<Health>();
        _health.OnHealthDeplete.AddListener(OnPlayerDead);
    }

    private void Start()
    {
        _startGravityScale = _rigidBody2d.gravityScale;
        _startAttackPosition = _attackCollider.transform.localPosition;

        _isMovingHash = Animator.StringToHash("IsMoving");
        _isGroundedHash = Animator.StringToHash("IsGrounded");
        _isJumpingHash = Animator.StringToHash("IsJumping");
        _isAttackingHash = Animator.StringToHash("IsAttacking");
        _isChargingHash = Animator.StringToHash("IsCharging");
        _isHeavyAttackingHash = Animator.StringToHash("IsHeavyAttacking");
        _isDeadHash = Animator.StringToHash("IsDead");
    }

    private void Update()
    {
        if (_isDead) return; 

        if (!_isDashing)
        {
            _rigidBody2d.linearVelocityX = _speed * _moveDirection.x;
        }

        CheckForGround();
        CheckForWall();

        UpdateAnimation();
    }

    private void OnPlayerDead()
    {
        _isDead = true;
        _animator.SetBool(_isDeadHash, true);

        _rigidBody2d.bodyType = RigidbodyType2D.Static;
        _rigidBody2d.simulated = false;
        _boxCollider2d.enabled = false;

        if (TryGetComponent(out PlayerInput playerInput))
        {
            playerInput.enabled = false;
        }
    }

    private void UpdateAnimation()
    {
        bool isMoving = Math.Abs(_moveDirection.x) > _moveThreshold;
        _animator.SetBool(_isMovingHash, isMoving);

        if (!isMoving) return;

        bool shouldFlipOnX = _moveDirection.x < 0;
        _spriteRenderer.flipX = shouldFlipOnX;
        _attackCollider.transform.localPosition = new Vector3(FlipAttackCollider(shouldFlipOnX), _startAttackPosition.y, _startAttackPosition.z);
    }

    private float FlipAttackCollider(bool shouldFlipOnX)
    {
        if (shouldFlipOnX) return -_startAttackPosition.x;
        else return _startAttackPosition.x;
    }

    private void CheckForGround()
    {
        Vector2 colliderSize = new(_boxCollider2d.size.x, 0.5f);

        RaycastHit2D raycastHit2D = Physics2D.BoxCast(transform.position, colliderSize, 0f, Vector2.down, 1f);

        _isGrounded = raycastHit2D && raycastHit2D.collider.CompareTag("Enviroment");
        _animator.SetBool(_isGroundedHash, _isGrounded);
    }

    private void CheckForWall()
    {
        Vector2 colliderSize = new(0.5f, _boxCollider2d.size.y);
        Vector2 rayDirection = new(_moveDirection.x, 0);

        RaycastHit2D raycastHit2D = Physics2D.BoxCast(transform.position, colliderSize, 0f, rayDirection, 1f);
        _isCollidingWall = raycastHit2D && raycastHit2D.collider.CompareTag("Enviroment");
        if (_isDashing && _isCollidingWall) CancelDash();
    }

    public void Attack()
    {
        if (_isHeavyAttacking) return;

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

    public void DisableAttackCollider()
    {
        _attackCollider.SetActive(false);
    }

    public void EnableAttackCollider()
    {
        _attackCollider.SetActive(true);
    }

    public void Jump()
    {
        if (!_isGrounded) return;

        _isJumping = true;
        _rigidBody2d.linearVelocityY = _jumpDistance;
        _animator.SetBool(_isJumpingHash, _isJumping);
        _trailRenderer.emitting = true;

        StartCoroutine(UpdateJumpState());
    }

    private IEnumerator UpdateJumpState()
    {
        yield return new WaitForSeconds(_jumpAnimationDuration);

        _isJumping = false;
        _animator.SetBool(_isJumpingHash, _isJumping);

        if (!_isDashing) _trailRenderer.emitting = false;
    }

    public void Dash()
    {
        if (!_canDash || _moveDirection.x == 0) return;

        _canDash = false;
        _isDashing = true;

        _rigidBody2d.gravityScale = 0;
        _rigidBody2d.linearVelocity = new Vector2(_moveDirection.x * _dashSpeed, 0);

        _trailRenderer.emitting = true;

        StartCoroutine(UpdateDashState());
    }

    private void CancelDash()
    {
        _isDashing = false;
        _rigidBody2d.gravityScale = _startGravityScale;
        
        if (!_isJumping) _trailRenderer.emitting = false;
    }

    private IEnumerator UpdateDashState()
    {
        yield return new WaitForSeconds(_dashDuration);

        CancelDash();

        yield return new WaitForSeconds(_dashCooldown);

        _canDash = true;
    }

    public void SetMoveDirection(Vector2 direction)
    {
        _moveDirection = direction;
    }
}
