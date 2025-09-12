using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Health System")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("UI Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Gradient healthBarGradient;
    
    [Header("Movement Stats")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged; // current, max
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnPlayerHealed;
    
    void Start()
    {
        // Initialize health to max
        currentHealth = maxHealth;
        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    void Update()
    {
        // Test damage with Q key
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TestTakeDamage();
        }
    }
    
    // Simple test function for damaging player
    private void TestTakeDamage()
    {
        TakeDamage(10f); // Take 10 damage each time Q is pressed
        Debug.Log($"Test damage! Current health: {currentHealth}/{maxHealth}");
    }
    
    #region Health System
    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    public void Heal(float healAmount)
    {
        if (healAmount <= 0f) return;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        if (currentHealth > previousHealth)
        {
            UpdateHealthBar();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnPlayerHealed?.Invoke();
        }
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void Die()
    {
        OnPlayerDeath?.Invoke();
        Debug.Log("Player has died!");
        // Add death logic here (disable movement, play animation, etc.)
    }
    
    public void Revive()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("Player revived!");
    }
    
    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // Update fill amount based on health percentage
            healthBarFill.fillAmount = GetHealthPercentage();
            
            // Update color based on health percentage using gradient
            if (healthBarGradient != null)
            {
                healthBarFill.color = healthBarGradient.Evaluate(GetHealthPercentage());
            }
        }
    }
    #endregion
    
    #region Getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0f;
    public float GetMoveSpeed() => moveSpeed;
    #endregion
    
    #region Setters
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }
    #endregion
}
