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
    
    private GameObject _target;
    private bool _canAttack = true;
    private CharacterController2D _characterController2d;

    private void Awake()
    {
        _characterController2d = GetComponent<CharacterController2D>();
        _characterController2d.OnCharacterDead.AddListener(OnCharacterDead);
    }

    private void Update()
    {
        if (_target && _canAttack) ChaseTarget();

        if (_useTargetDetection && _target == null) UpdateTargetOnEnter();
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
            break;
        }

    }

    private void ChaseTarget()
    {
        Vector2 direction = _target.transform.position.x > transform.position.x ? Vector2.right : Vector2.left;
        float distance = Vector2.Distance(_target.transform.position, transform.position);
        float horizontalDistance = Mathf.Abs(_target.transform.position.x - transform.position.x);

        if (distance >= _targetDetectionRange + _targetLeaveRange)
        {
            _characterController2d.SetMoveDirection(Vector2.zero);
            _target = null;

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

    private void OnCharacterDead()
    {
        enabled = false;
    }

    private IEnumerator EnableAttack()
    {
        yield return new WaitForSeconds(_attackDelay);

        _canAttack = true;
    }
}
