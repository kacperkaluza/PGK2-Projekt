using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;

/// <summary>
/// Menedżer pauzy (Pause Menu).
/// Wykorzystuje technologię UI Toolkit do wstrzymywania i wznawiania działania gry za pomocą klawiatury (Escape).
/// Zarządza blokowaniem/odblokowywaniem kursora myszy oraz animowanym odliczaniem do powrotu do gry.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    /// <summary> Referencja do głównego dokumentu interfejsu (UI Toolkit) definiującego strukturę widoku pauzy. </summary>
    public UIDocument uiDocument;

    /// <summary> Główny kontener menu wyświetlający opcje (np. Resume, Exit). </summary>
    private VisualElement menuView;
    
    /// <summary> Kontener wykorzystywany w trakcie animacji odliczania wyświetlanej przy powrocie do gry. </summary>
    private VisualElement countdownView;
    
    /// <summary> Referencja do przycisku wznawiającego grę. </summary>
    private Button resumeButton;
    
    /// <summary> Referencja do przycisku ładującego menu główne. </summary>
    private Button exitButton;
    
    /// <summary> Referencja do etykiety tekstowej wyświetlającej pozostały czas do wznowienia gry. </summary>
    private Label countdownLabel;

    /// <summary> Flaga określająca, czy gra jest aktualnie zapauzowana. </summary>
    private bool isPaused = false;
    
    /// <summary> Flaga określająca, czy aktualnie trwa proces odliczania do wznowienia gry. </summary>
    private bool isCountingDown = false;

    /// <summary>
    /// Metoda wywoływana podczas włączania/aktywacji obiektu.
    /// Pobiera zależności UI Toolkit oraz subskrybuje obsługę kliknięć poszczególnych przycisków w menu.
    /// Na końcu ukrywa główne okna interfejsu.
    /// </summary>
    void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            var root = uiDocument.rootVisualElement;
            menuView = root.Q<VisualElement>("MenuView");
            countdownView = root.Q<VisualElement>("CountdownView");
            resumeButton = root.Q<Button>("ResumeButton");
            exitButton = root.Q<Button>("ExitButton");
            countdownLabel = root.Q<Label>("CountdownLabel");

            if (resumeButton != null)
                resumeButton.clicked += StartResumeCountdown;
            
            if (exitButton != null)
                exitButton.clicked += LoadMainMenu;

            // Initially hide both
            if (menuView != null) menuView.style.display = DisplayStyle.None;
            if (countdownView != null) countdownView.style.display = DisplayStyle.None;
        }
    }

    /// <summary>
    /// Metoda wywoływana co klatkę. Oczekuje na naciśnięcie klawisza Escape w celu naprzemiennego wywoływania pauzy i jej zamykania.
    /// Blokuje możliwość reakcji gracza na input, jeśli proces odliczania powrotu już wystartował.
    /// </summary>
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isCountingDown)
            {
                return; // Ignore input while counting down
            }

            if (isPaused)
            {
                StartResumeCountdown();
            }
            else
            {
                Pause();
            }
        }
    }

    /// <summary>
    /// Uruchamia proces przygotowania do powrotu do gry poprzez korutynę odliczania.
    /// Chroni przed wielokrotnym uruchomieniem procedury na raz.
    /// </summary>
    private void StartResumeCountdown()
    {
        if (isCountingDown) return;
        StartCoroutine(ResumeCountdownRoutine());
    }

    /// <summary>
    /// Korutyna zarządzająca sekwencją powrotu do gry. Wyłącza widok menu, wyświetla element odliczający,
    /// a następnie po 3 sekundach rzeczywistego czasu powraca do pętli rozgrywki.
    /// </summary>
    /// <returns>Obiekt do wstrzymania działania funkcji IEnumerator zależny od czasu w systemie.</returns>
    private IEnumerator ResumeCountdownRoutine()
    {
        isCountingDown = true;
        
        // Hide menu, show countdown
        if (menuView != null) menuView.style.display = DisplayStyle.None;
        if (countdownView != null) countdownView.style.display = DisplayStyle.Flex;

        int count = 3;
        while (count > 0)
        {
            if (countdownLabel != null) countdownLabel.text = count.ToString();
            // Wait for 1 second in real time (since Time.timeScale is 0)
            yield return new WaitForSecondsRealtime(1f);
            count--;
        }

        // Hide countdown
        if (countdownView != null) countdownView.style.display = DisplayStyle.None;

        // Actually resume game
        Time.timeScale = 1f;
        isPaused = false;
        isCountingDown = false;
        
        // Hide and lock cursor
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    /// <summary>
    /// Aktywuje tryb pauzy: wyświetla menu powitalne, zatrzymuje czas systemowy w silniku (Time.timeScale = 0),
    /// uwalnia kursor gracza i flaguje ten stan w systemie.
    /// </summary>
    void Pause()
    {
        if (menuView != null) menuView.style.display = DisplayStyle.Flex;
        if (countdownView != null) countdownView.style.display = DisplayStyle.None;
        
        Time.timeScale = 0f;
        isPaused = true;
        
        // Show and unlock cursor
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    /// <summary>
    /// Zdarzenie powrotu do menu uruchamiane po kliknięciu klawisza 'Exit'.
    /// Przywraca naturalny bieg czasu przed ładowaniem nowej sceny.
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
