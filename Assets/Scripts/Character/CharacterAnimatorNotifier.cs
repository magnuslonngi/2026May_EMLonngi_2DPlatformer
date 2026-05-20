using UnityEngine;

public class CharacterAnimatorNotifier : MonoBehaviour
{
    private CharacterController2D _characterController2d;

    private void Awake()
    {
        _characterController2d = GetComponentInParent<CharacterController2D>();
    }

    public void DisableAttackAnimation()
    {
        _characterController2d.DisableAttackAnimation();
    }

    public void DisableAttackCollider()
    {
        _characterController2d.DisableAttackCollider();
    }

    public void EnableAttackCollider()
    {
        _characterController2d.EnableAttackCollider();
    }
}
