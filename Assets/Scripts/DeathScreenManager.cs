using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Menedżer zarządzający ekranem końcowym gry (ekranem śmierci).
/// Wykorzystuje technologię UI Toolkit do zaprezentowania graczowi jego podsumowania statystyk 
/// oraz daje możliwość restartu rozgrywki lub powrotu do menu głównego.
/// </summary>
public class DeathScreenManager : MonoBehaviour
{
    /// <summary> Referencja do głównego dokumentu UI Toolkit na scenie. </summary>
    private UIDocument uiDocument;
    
    /// <summary> Główny kontener (VisualElement) zawierający cały ekran śmierci, domyślnie ukryty. </summary>
    private VisualElement deathOverlay;
    
    /// <summary> Przycisk umożliwiający ponowne uruchomienie aktualnej sceny gry. </summary>
    private Button restartButton;
    
    /// <summary> Przycisk służący do powrotu do menu głównego. </summary>
    private Button exitMenuButton;

    /// <summary> Etykieta wyświetlająca całkowity czas przetrwania w grze. </summary>
    private Label lifetimeLabel;
    
    /// <summary> Etykieta wyświetlająca liczbę pokonanych przeciwników. </summary>
    private Label killsLabel;
    
    /// <summary> Etykieta wyświetlająca ostateczny poziom doświadczenia gracza. </summary>
    private Label levelLabel;
    
    /// <summary> Etykieta wyświetlająca sumę zadanych przez gracza obrażeń. </summary>
    private Label damageLabel;
    
    /// <summary> Etykieta wyświetlająca wartość statystyki obrażeń ataku gracza. </summary>
    private Label attackDamageLabel;
    
    /// <summary> Etykieta wyświetlająca wartość statystyki szybkości ataku gracza. </summary>
    private Label attackSpeedLabel;
    
    /// <summary> Etykieta wyświetlająca maksymalne zdrowie gracza. </summary>
    private Label maxHealthLabel;
    
    /// <summary> Etykieta wyświetlająca wartość pancerza gracza. </summary>
    private Label armorLabel;
    
    /// <summary> Etykieta wyświetlająca szansę na trafienie krytyczne gracza (w %). </summary>
    private Label critChanceLabel;
    
    /// <summary> Etykieta wyświetlająca mnożnik trafień krytycznych gracza. </summary>
    private Label critDamageLabel;
    
    /// <summary> Etykieta wyświetlająca mnożnik prędkości poruszania się gracza. </summary>
    private Label moveSpeedLabel;

    /// <summary>
    /// Metoda inicjalizacyjna wywoływana przed pierwszą klatką.
    /// Wiąże zmienne z elementami drzewa DOM (UI Toolkit), ukrywa ekran i podpina zdarzenia do przycisków.
    /// </summary>
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;

            deathOverlay = root.Q<VisualElement>("DeathOverlay");
            
            // Elements are hidden by default
            if (deathOverlay != null)
            {
                deathOverlay.style.display = DisplayStyle.None;
            }

            lifetimeLabel = root.Q<Label>("LifetimeLabel");
            killsLabel = root.Q<Label>("KillsLabel");
            levelLabel = root.Q<Label>("LevelLabel");
            damageLabel = root.Q<Label>("DamageLabel");
            
            attackDamageLabel = root.Q<Label>("AttackDamageLabel");
            attackSpeedLabel = root.Q<Label>("AttackSpeedLabel");
            maxHealthLabel = root.Q<Label>("MaxHealthLabel");
            armorLabel = root.Q<Label>("ArmorLabel");
            critChanceLabel = root.Q<Label>("CritChanceLabel");
            critDamageLabel = root.Q<Label>("CritDamageLabel");
            moveSpeedLabel = root.Q<Label>("MoveSpeedLabel");

            restartButton = root.Q<Button>("RestartButton");
            exitMenuButton = root.Q<Button>("ExitMenuButton");

            if (restartButton != null)
            {
                restartButton.clicked += OnRestartClicked;
            }
            if (exitMenuButton != null)
            {
                exitMenuButton.clicked += OnExitMenuClicked;
            }
        }
    }

    /// <summary>
    /// Metoda wywoływana w momencie niszczenia obiektu.
    /// Odpina zdarzenia od przycisków, zapobiegając wyciekom pamięci.
    /// </summary>
    private void OnDestroy()
    {
        if (restartButton != null) restartButton.clicked -= OnRestartClicked;
        if (exitMenuButton != null) exitMenuButton.clicked -= OnExitMenuClicked;
    }

    /// <summary>
    /// Wyświetla główny overlay ekranu śmierci, wstrzymuje czas gry (Time.timeScale = 0)
    /// oraz odblokowuje widoczność kursora dla gracza.
    /// </summary>
    public void ShowDeathScreen()
    {
        if (deathOverlay != null)
        {
            UpdateStatsUI();
            deathOverlay.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f; // Pause game logic
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }

    /// <summary>
    /// Obsługa zdarzenia kliknięcia przycisku Restart. Przywraca upływ czasu i przeładowuje aktualną scenę.
    /// </summary>
    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Obsługa zdarzenia kliknięcia przycisku powrotu do Menu. Przywraca upływ czasu i ładuje scenę MainMenu.
    /// </summary>
    private void OnExitMenuClicked()
    {
        Time.timeScale = 1f;
        // Assuming "MainMenu" is the name of the menu scene. Fallback to index 0 if needed.
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Aktualizuje treść etykiet UI w oparciu o obiekt klasy PlayerStats pobrany ze sceny.
    /// Formatuje wartości czasowe oraz liczbowe.
    /// </summary>
    private void UpdateStatsUI()
    {
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
        {
            if (lifetimeLabel != null) 
            {
                int minutes = Mathf.FloorToInt(stats.timeAlive / 60f);
                int seconds = Mathf.FloorToInt(stats.timeAlive % 60f);
                lifetimeLabel.text = $"Lifetime: {minutes:00}:{seconds:00}";
            }
            if (killsLabel != null) killsLabel.text = $"Enemies Killed: {stats.enemiesKilled}";
            if (levelLabel != null) levelLabel.text = $"Level: {stats.currentLevel}";
            if (damageLabel != null) damageLabel.text = $"Damage Dealt: {Mathf.RoundToInt(stats.totalDamageDealt)}";

            if (attackDamageLabel != null) attackDamageLabel.text = $"Attack Dmg: {stats.attackDamage}";
            if (attackSpeedLabel != null) attackSpeedLabel.text = $"Attack Speed: {stats.attackSpeed}";
            if (maxHealthLabel != null) maxHealthLabel.text = $"Max Health: {stats.maxHealth}";
            if (armorLabel != null) armorLabel.text = $"Armor: {stats.armor}";
            if (critChanceLabel != null) critChanceLabel.text = $"Crit Chance: {stats.critChance}%";
            if (critDamageLabel != null) critDamageLabel.text = $"Crit Dmg: {stats.critDamage * 100f}%";
            if (moveSpeedLabel != null) moveSpeedLabel.text = $"Move Speed: {stats.moveSpeedMultiplier}";
        }
    }
}
