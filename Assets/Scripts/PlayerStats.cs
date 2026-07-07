using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Klasa przechowująca statystyki gracza oraz zarządzająca jego zdrowiem, doświadczeniem i postępami w grze.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Statystyki Główne (HP)")]
    /// <summary> Maksymalna liczba punktów zdrowia (HP) gracza. </summary>
    public float maxHealth = 100f;
    
    /// <summary> Aktualna liczba punktów zdrowia (HP) gracza. </summary>
    public float currentHealth;
    
    /// <summary> Wartość regeneracji zdrowia na sekundę. </summary>
    public float healthRegen = 0f;
    
    /// <summary> Wartość pancerza zmniejszającego otrzymywane obrażenia. </summary>
    public float armor = 0f;
    
    [Header("Statystyki Mobilności")]
    /// <summary> Mnożnik prędkości poruszania się gracza. </summary>
    public float moveSpeedMultiplier = 1.0f;

    [Header("System Doświadczenia")]
    /// <summary> Aktualny poziom doświadczenia gracza. </summary>
    public int currentLevel = 1;
    
    /// <summary> Aktualnie zgromadzone punkty doświadczenia (EXP). </summary>
    public float currentExp = 0f;
    
    /// <summary> Wymagana liczba punktów doświadczenia do osiągnięcia następnego poziomu. </summary>
    public float expToNextLevel = 100f;
    
    /// <summary> Mnożnik zdobywanych punktów doświadczenia. </summary>
    public float xpGain = 1.0f;
    
    /// <summary> Mnożnik zasięgu podnoszenia przedmiotów. </summary>
    public float pickupRadius = 1.0f;
    
    [Header("Statystyki Ofensywne")]
    /// <summary> Bazowe obrażenia zadawane przez ataki gracza. </summary>
    public float attackDamage = 10f;
    
    /// <summary> Szybkość ataku (liczba ataków na sekundę). </summary>
    public float attackSpeed = 1f;
    
    /// <summary> Zasięg detekcji celów dla ataków gracza. </summary>
    public float attackRange = 5f;
    
    /// <summary> Szansa na trafienie krytyczne, wyrażona w procentach. </summary>
    public float critChance = 5f;
    
    /// <summary> Mnożnik obrażeń krytycznych. </summary>
    public float critDamage = 1.5f;
    
    /// <summary> Procent zadanych obrażeń przywracany jako zdrowie gracza (wartość od 0 do 1). </summary>
    public float lifesteal = 0f;

    [Header("Referencje UI i Menedżerów")]
    /// <summary> Referencja do menedżera ulepszeń. </summary>
    public UIUpgradeManager upgradeManager;

    [Header("Statystyki Gry (Podsumowanie)")]
    /// <summary> Całkowita liczba pokonanych przeciwników w trakcie gry. </summary>
    public int enemiesKilled = 0;
    
    /// <summary> Łączna liczba obrażeń zadanych przeciwnikom. </summary>
    public float totalDamageDealt = 0f;
    
    /// <summary> Łączna liczba przywróconego zdrowia gracza. </summary>
    public float totalHealthHealed = 0f;
    
    /// <summary> Czas spędzony w grze przy życiu, wyrażony w sekundach. </summary>
    public float timeAlive = 0f;

    /// <summary>
    /// Metoda inicjalizacyjna wywoływana przed pierwszą klatką gry.
    /// Ustawia początkowe punkty zdrowia gracza na wartość maksymalną.
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    /// <summary>
    /// Metoda wywoływana w każdej klatce gry.
    /// Odpowiada za aktualizację czasu przetrwania i regenerację punktów zdrowia.
    /// </summary>
    void Update()
    {
        timeAlive += Time.deltaTime;

        if (healthRegen > 0f && currentHealth < maxHealth && currentHealth > 0f)
        {
            currentHealth += healthRegen * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateUI();
        }
    }

    /// <summary>
    /// Przywraca punkty zdrowia graczowi.
    /// </summary>
    /// <param name="amount">Ilość punktów zdrowia do przywrócenia.</param>
    public void Heal(float amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;
        
        float actualHeal = Mathf.Min(amount, maxHealth - currentHealth);
        totalHealthHealed += actualHeal;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    /// <summary>
    /// Zmniejsza punkty zdrowia gracza po otrzymaniu obrażeń, uwzględniając pancerz.
    /// </summary>
    /// <param name="damage">Ilość zadawanych obrażeń.</param>
    public void TakeDamage(float damage)
    {
        float mitigatedDamage = Mathf.Max(1f, damage - armor);

        currentHealth -= mitigatedDamage;
        
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 
        
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Dodaje punkty doświadczenia graczowi, sprawdzając możliwość awansu na kolejny poziom.
    /// </summary>
    /// <param name="amount">Bazowa ilość punktów doświadczenia do dodania.</param>
    public void AddExp(float amount)
    {
        currentExp += (amount * xpGain);

        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        
        UpdateUI();
    }

    /// <summary>
    /// Zwiększa poziom gracza po osiągnięciu odpowiedniego progu doświadczenia.
    /// Zwiększa wymagania na kolejny poziom i wywołuje okno wyboru ulepszeń.
    /// </summary>
    private void LevelUp()
    {
        currentLevel++;
        currentExp -= expToNextLevel; 
        expToNextLevel *= 1.2f; 

        if (upgradeManager != null)
        {
            upgradeManager.TriggerLevelUp();
        }
        else
        {
            Debug.LogWarning("Brak przypisanego UpgradeManager w PlayerStats!");
        }
    }

    /// <summary>
    /// Aktualizuje stan interfejsu użytkownika (obecnie zarządzane zewnętrznie przez GameUI).
    /// </summary>
    public void UpdateUI()
    {
        // HUD is now updated via GameUI.cs and UI Toolkit in Update()
    }

    /// <summary>
    /// Dodaje wartość do łącznej puli zadanych obrażeń przez gracza w aktualnej sesji.
    /// </summary>
    /// <param name="damage">Zadane obrażenia.</param>
    public void AddDamageDealt(float damage)
    {
        totalDamageDealt += damage;
    }

    /// <summary>
    /// Inkrementuje licznik zabitych przeciwników.
    /// </summary>
    public void AddKill()
    {
        enemiesKilled++;
    }

    /// <summary>
    /// Obsługuje logikę śmierci gracza, wyświetlając ekran końcowy lub resetując scenę.
    /// </summary>
    private void Die()
    {
        Debug.Log("Gracz zginął! Koniec gry.");
        
        DeathScreenManager deathScreen = FindObjectOfType<DeathScreenManager>();
        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}