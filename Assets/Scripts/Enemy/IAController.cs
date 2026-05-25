using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class IAController : MonoBehaviour
{
    [Header("Target Detection")]
    [SerializeField] private bool _useTargetDetection = true;
    [SerializeField] private float _targetDetectionRange = 5f;
    [SerializeField] private float _targetLeaveRange = 1f;
    [SerializeField] private float _attackRange = 2.5f;
    [SerializeField] private float _attackDelay = 0.8f;

    [Header("Patrol")]
    [SerializeField] private bool _enablePatrol;

    [Header("Jump")]
    [SerializeField] private bool _enableJumpTimer;
    [SerializeField] private float _jumpCooldown = 2f;
    [SerializeField] private bool _enableJumpOnDetect;

    [Header("Dash")]
    [SerializeField] private bool _enableDashTimer;
    [SerializeField] private float _dashCooldown = 2f;
    [SerializeField] private bool _enableDashOnDetect;
    
    private GameObject _target;
    private bool _canAttack = true;
    private CharacterController2D _characterController2d;

    private Coroutine _enableJumpRoutine;
    private Coroutine _enableDashRoutine;

    private void Awake()
    {
        _characterController2d = GetComponent<CharacterController2D>();
        _characterController2d.OnCharacterDead.AddListener(OnCharacterDead);
    }

    private void Start()
    {
        if (_enablePatrol) SetRandomMoveDirection();

        if (_enableJumpTimer) _enableJumpRoutine = StartCoroutine(EnableJump());

        if (_enableDashTimer) _enableDashRoutine = StartCoroutine(EnableDash());
    }

    private void Update()
    {
        if (_enablePatrol && !_target) Patrol();

        if (_target && _canAttack) ChaseTarget();

        if (_useTargetDetection && _target == null) UpdateTargetOnEnter();
    }

    private void OnDisable()
    {
        if (_enableJumpRoutine != null) StopCoroutine(_enableJumpRoutine);

        if (_enableDashRoutine != null) StopCoroutine(_enableDashRoutine);
    }

    private void SetRandomMoveDirection()
    {
        int randomXDirection = Random.Range(0.1f, 1f) > 0.5f ? 1 : -1;
        _characterController2d.SetMoveDirection(new(randomXDirection, 0));
    }

    private void UpdateTargetOnEnter()
    {
        // I don't know why but when I use a layer like player, it doesn't detect the player...
        // Also, if I use only OverlapCircle, sometimes it doesn't detect the player...
        Collider2D[] raycastHits = Physics2D.OverlapCircleAll(transform.position, _targetDetectionRange);
        
        foreach(Collider2D hit in raycastHits)
        {
            if (!hit.CompareTag("Player")) continue;

            _target = hit.gameObject;
            _characterController2d.SetMoveDirection(GetMoveDirectionFromTarget());

            StartCoroutine(OnDetect());

            break;
        }

    }

    private IEnumerator OnDetect()
    {
        yield return new WaitForEndOfFrame();


        if (_enableDashOnDetect) _characterController2d.Dash();

        if (_enableJumpOnDetect) _characterController2d.Jump();
    }

    private void ChaseTarget()
    {
        Vector2 direction = GetMoveDirectionFromTarget();
        float distance = Vector2.Distance(_target.transform.position, transform.position);
        float horizontalDistance = Mathf.Abs(_target.transform.position.x - transform.position.x);

        if (distance >= _targetDetectionRange + _targetLeaveRange)
        {
            _target = null;

            if (_enablePatrol)
            {
                SetRandomMoveDirection();
                return;
            }

            _characterController2d.SetMoveDirection(Vector2.zero);
            return;
        }

        if (distance <= _attackRange)
        {
            _characterController2d.SetMoveDirection(Vector2.zero);
            _characterController2d.Attack();

            _canAttack = false;    
            StartCoroutine(EnableAttack());

            return;
        }

        if (horizontalDistance < 0.1f)
        {
            _characterController2d.SetMoveDirection(Vector2.zero);
            return;
        }

        _characterController2d.SetMoveDirection(direction);
    }

    private Vector2 GetMoveDirectionFromTarget()
    {
        return _target.transform.position.x > transform.position.x ? Vector2.right : Vector2.left;
    }

    private void Patrol()
    {
        if(_characterController2d.IsCollidingWithWall())
        {
            Vector2 moveDirection = _characterController2d.GetMoveDirection();
            moveDirection.x *= -1;

            _characterController2d.SetMoveDirection(moveDirection);
        }
    }

    private void OnCharacterDead()
    {
        enabled = false;
    }

    private IEnumerator EnableAttack()
    {
        yield return new WaitForSeconds(_attackDelay);

        _canAttack = true;
    }

    private IEnumerator EnableDash()
    {
        while (true)
        {
            yield return new WaitForSeconds(_dashCooldown);

            _characterController2d.Dash();
        }
    }

    private IEnumerator EnableJump()
    {
        while (true)
        {
            yield return new WaitForSeconds(_jumpCooldown);

            _characterController2d.Jump();
        }
    }
}
