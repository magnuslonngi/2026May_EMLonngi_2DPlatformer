using UnityEngine;

public class HitCollider : MonoBehaviour
{
    [SerializeField] internal float _damage = 5f;
    private void OnTriggerEnter2D(Collider2D other)
    {
        HurtCollider hurtCollider = other.GetComponent<HurtCollider>();
        hurtCollider?.NotifyHit(this);
    }
}
