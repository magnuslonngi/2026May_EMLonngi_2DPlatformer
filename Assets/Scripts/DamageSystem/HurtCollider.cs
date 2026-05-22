using UnityEngine;
using UnityEngine.Events;

public class HurtCollider : MonoBehaviour
{
    public UnityEvent<float> OnHitRecieved;

    public void NotifyHit(HitCollider hitCollider)
    {
        OnHitRecieved?.Invoke(hitCollider.GetDamage());
    }
}
