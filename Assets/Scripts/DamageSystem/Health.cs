using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private float _healthPoints;

    public UnityEvent<float> OnHealthSpawn;
    public UnityEvent<float> OnHealthChange;
    public UnityEvent OnHealthDeplete;

    private HurtCollider _hurtCollider;

    private void Awake()
    {
        _hurtCollider = GetComponent<HurtCollider>();
        _hurtCollider.OnHitRecieved.AddListener(OnHitRecieved);
    }

    private void Start()
    {
        OnHealthSpawn?.Invoke(_healthPoints);
    }

    private void OnHitRecieved(float damage)
    {
        if (!(_healthPoints > 0)) return;

        _healthPoints -= damage;
        OnHealthChange?.Invoke(_healthPoints);

        if (_healthPoints <= 0)
        {
            _healthPoints = 0;
            OnHealthDeplete?.Invoke();
        }
    }
}
