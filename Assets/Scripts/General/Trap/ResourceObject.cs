using UnityEngine;

public class ResourceObject : MonoBehaviour
{
    [Header("Resource Stats")]
    [SerializeField] private int _maxHealth = 3;
    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public bool TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
            return true;
        }

        return false;
    }
}