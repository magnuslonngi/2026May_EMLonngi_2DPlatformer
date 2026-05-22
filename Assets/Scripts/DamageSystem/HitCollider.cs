using UnityEngine;

public class HitCollider : MonoBehaviour
{
    private float _damage = 5f;

    public float GetDamage()
    {
        return _damage;
    }

    public void SetDamage(float damage)
    {
        _damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HurtCollider hurtCollider = other.GetComponent<HurtCollider>();
        hurtCollider?.NotifyHit(this);
    }
}
