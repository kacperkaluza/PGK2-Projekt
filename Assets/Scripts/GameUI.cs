using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Kontroler głównego interfejsu użytkownika (HUD) działający w trakcie rozgrywki (UI Toolkit).
/// Obsługuje wyświetlanie najważniejszych informacji: paska zdrowia z dynamiczną kolorystyką, 
/// paska postępu poziomu, liczników doświadczenia oraz czasu przetrwania w grze.
/// </summary>
public class GameUI : MonoBehaviour
{
    /// <summary> Referencja do korzenia dokumentu UI Toolkit opisującego HUD gracza. </summary>
    private UIDocument uiDocument;
    
    /// <summary> Etykieta wyświetlająca czas rozegrany od początku gry. </summary>
    private Label timerText;
    
    /// <summary> Pasek postępu reprezentujący bieżący stan punktów zdrowia gracza (HP). </summary>
    private ProgressBar healthBar;
    
    /// <summary> Tekstowa reprezentacja punktów zdrowia w formacie (Aktualne / Maksymalne). </summary>
    private Label healthText;
    
    /// <summary> Pasek postępu reprezentujący zdobyte przez gracza punkty doświadczenia. </summary>
    private ProgressBar expBar;
    
    /// <summary> Etykieta wyświetlająca obecny poziom gracza. </summary>
    private Label levelText;
    
    /// <summary> Tekstowa reprezentacja punktów doświadczenia zebranych w ramach obecnego poziomu gracza. </summary>
    private Label expText;

    /// <summary> Osobny komponent wizualny używany do manipulowania barwą wypełnienia paska zdrowia. </summary>
    private VisualElement healthProgressElement;

    /// <summary> Buforowana referencja do komponentu ze statystykami gracza. </summary>
    private PlayerStats playerStats;
    
    /// <summary> Wewnętrzny licznik przechowujący czas sesji (w sekundach). </summary>
    private float elapsedTime;

    /// <summary>
    /// Metoda wywoływana przed pierwszą klatką. Inicjalizuje referencje do komponentu `PlayerStats` 
    /// oraz parsuje drzewo elementów UI Toolkit wyszukując obiekty składowe interfejsu (etykiety i paski).
    /// </summary>
    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        uiDocument = GetComponent<UIDocument>();
        
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            timerText = root.Q<Label>("TimerLabel");
            healthBar = root.Q<ProgressBar>("HealthBar");
            healthText = root.Q<Label>("HealthText");
            expBar = root.Q<ProgressBar>("ExpBar");
            levelText = root.Q<Label>("LevelText");
            expText = root.Q<Label>("ExpText");

            if (healthBar != null)
            {
                // Cache the inner progress element to change its color dynamically
                healthProgressElement = healthBar.Q(className: "unity-progress-bar__progress");
            }
        }
    }

    /// <summary>
    /// Metoda wywoływana cyklicznie podczas gry. Odpowiada za akumulację odmierzanego czasu 
    /// oraz rutynową aktualizację wyświetlanych wartości na interfejsie.
    /// </summary>
    void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateTimer();
        UpdatePlayerUI();
    }

    /// <summary>
    /// Formatuje zgromadzony czas (w sekundach) do postaci "minuty:sekundy" i aplikuje go na odpowiednią etykietę UI.
    /// </summary>
    void UpdateTimer()
    {
        if (timerText == null) return;
        
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Aktualizuje wszystkie wskaźniki zdrowia oraz progresu doświadczenia bezpośrednio z modelu `PlayerStats`.
    /// Dostosowuje również kolor paska zdrowia zależnie od progu pozostałych punktów życia.
    /// </summary>
    void UpdatePlayerUI()
    {
        if (playerStats == null || uiDocument == null) return;

        // Update Health
        if (healthBar != null)
        {
            healthBar.highValue = playerStats.maxHealth;
            healthBar.value = playerStats.currentHealth;
            
            // Dynamic Health Color
            float healthPct = playerStats.currentHealth / playerStats.maxHealth;
            if (healthProgressElement != null)
            {
                if (healthPct > 0.5f)
                    healthProgressElement.style.backgroundColor = new StyleColor(new Color(0f, 1f, 0.26f)); // Green
                else if (healthPct > 0.2f)
                    healthProgressElement.style.backgroundColor = new StyleColor(new Color(1f, 0.8f, 0f)); // Yellow
                else
                    healthProgressElement.style.backgroundColor = new StyleColor(new Color(1f, 0.2f, 0.2f)); // Red
            }
        }
        
        if (healthText != null)
        {
            healthText.text = Mathf.CeilToInt(playerStats.currentHealth) + " / " + playerStats.maxHealth;
        }

        // Update EXP
        if (expBar != null)
        {
            expBar.highValue = playerStats.expToNextLevel;
            expBar.value = playerStats.currentExp;
        }
        
        if (levelText != null)
        {
            levelText.text = "LVL " + playerStats.currentLevel;
        }
        
        if (expText != null)
        {
            expText.text = Mathf.FloorToInt(playerStats.currentExp) + " / " + playerStats.expToNextLevel;
        }
    }
}