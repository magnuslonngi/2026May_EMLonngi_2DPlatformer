using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private InputActionReference _moveInput;
    [SerializeField] private InputActionReference _jumpInput;
    [SerializeField] private InputActionReference _attackInput;
    [SerializeField] private InputActionReference _dashInput;

    private CharacterController2D _characterController2d;

    private void Awake()
    {
        _characterController2d = GetComponent<CharacterController2D>();
    }

    private void OnEnable()
    {
        _moveInput.action.performed += OnMove;
        _moveInput.action.canceled += OnMove;

        _jumpInput.action.performed += OnJump;

        _attackInput.action.performed += OnAttack;

        _dashInput.action.performed += OnDash;
    }

    private void OnDisable()
    {
        _moveInput.action.performed -= OnMove;
        _moveInput.action.canceled -= OnMove;

        _jumpInput.action.performed -= OnJump;

        _attackInput.action.performed -= OnAttack;

        _dashInput.action.performed -= OnDash;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _characterController2d.SetMoveDirection(context.ReadValue<Vector2>());
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        _characterController2d.Jump();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        _characterController2d.Attack();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        _characterController2d.Dash();
    }
}
