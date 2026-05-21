using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private RectTransform _healthBar;
    [SerializeField] private Health _health;

    private float _startWidth;
    private float _startHealthPoints;

    private void Awake()
    {
        _health.OnHealthChange.AddListener(OnHealthChange);
        _health.OnHealthSpawn.AddListener(OnHealthStart);
    }

    private void Start()
    {
        _startWidth = _healthBar.rect.width;
    }

    private void OnHealthStart(float healthPoints)
    {
        _startHealthPoints = healthPoints;
    }

    private void OnHealthChange(float healthPoints)
    {
        float newWidth = healthPoints / _startHealthPoints * _startWidth;
        _healthBar.sizeDelta = new(newWidth, _healthBar.sizeDelta.y);
    }
}
