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
    
    [Header("Armor System")]
    [SerializeField] private float armor = 0f;
    [SerializeField] private float maxArmor = 100f;
    
    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged; // current, max
    public UnityEvent<float, float> OnArmorChanged; // current, max
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnPlayerHealed;
    
    void Start()
    {
        // Initialize health to max
        currentHealth = maxHealth;
        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Initialize armor
        OnArmorChanged?.Invoke(armor, maxArmor);
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
        float originalDamage = 10f;
        float actualDamage = CalculateDamageWithArmor(originalDamage);
        TakeDamage(originalDamage); // Take 10 damage each time Q is pressed
        Debug.Log($"Test damage! Original: {originalDamage}, Actual: {actualDamage:F1}, Health: {currentHealth}/{maxHealth}, Armor: {armor}/{maxArmor}");
    }
    
    #region Health System
    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        
        float actualDamage = CalculateDamageWithArmor(damage);
        
        currentHealth = Mathf.Max(0f, currentHealth - actualDamage);
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
    
    #region Armor System
    private float CalculateDamageWithArmor(float incomingDamage)
    {
        if (armor <= 0f) return incomingDamage;
        
        // Simple armor calculation: armor reduces damage by a percentage
        // Formula: actualDamage = incomingDamage * (1 - armor / (armor + 100))
        // This means 50 armor = 33% damage reduction, 100 armor = 50% damage reduction
        float damageReduction = armor / (armor + 100f);
        float actualDamage = incomingDamage * (1f - damageReduction);
        
        return Mathf.Max(0f, actualDamage);
    }
    
    public void AddArmor(float armorAmount)
    {
        if (armorAmount <= 0f) return;
        
        armor = Mathf.Min(maxArmor, armor + armorAmount);
        OnArmorChanged?.Invoke(armor, maxArmor);
    }
    
    public void RemoveArmor(float armorAmount)
    {
        if (armorAmount <= 0f) return;
        
        armor = Mathf.Max(0f, armor - armorAmount);
        OnArmorChanged?.Invoke(armor, maxArmor);
    }
    
    public void SetArmor(float newArmor)
    {
        armor = Mathf.Clamp(newArmor, 0f, maxArmor);
        OnArmorChanged?.Invoke(armor, maxArmor);
    }
    
    public void SetMaxArmor(float newMaxArmor)
    {
        maxArmor = Mathf.Max(0f, newMaxArmor);
        armor = Mathf.Min(armor, maxArmor);
        OnArmorChanged?.Invoke(armor, maxArmor);
    }
    #endregion
    
    #region Getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0f;
    public float GetMoveSpeed() => moveSpeed;
    public float GetCurrentArmor() => armor;
    public float GetMaxArmor() => maxArmor;
    public float GetArmorPercentage() => armor / maxArmor;
    #endregion
    
    #region Setters
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }
    #endregion
}
