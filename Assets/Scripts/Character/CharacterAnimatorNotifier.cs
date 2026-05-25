using UnityEngine;

public class CharacterAnimatorNotifier : MonoBehaviour
{
    private CharacterController2D _characterController2d;
    private AudioSource _audioSource;

    private void Awake()
    {
        _characterController2d = GetComponentInParent<CharacterController2D>();
        _audioSource = GetComponent<AudioSource>();
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

    public void PlayAttackSound()
    {
        _audioSource.Play();
    }
}
