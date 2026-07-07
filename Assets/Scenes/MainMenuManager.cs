using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Menedżer menu głównego (Main Menu).
/// Zarządza ekranem startowym zbudowanym w oparciu o technologię UI Toolkit, obsługuje akcje
/// wszystkich zadeklarowanych w nim przycisków, w tym przechodzenie do sceny gry oraz zamykanie aplikacji.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    /// <summary> Nazwa sceny, która ma zostać załadowana po wciśnięciu przycisku 'Play'. </summary>
    [SerializeField] private string gameSceneName = "ArenaGry";

    /// <summary> Referencja do komponentu UIDocument przechowującego układ elementów wizualnych. </summary>
    private UIDocument _uiDocument;
    
    /// <summary> Przycisk uruchamiający główną rozgrywkę. </summary>
    private Button _playButton;
    
    /// <summary> Przycisk wyświetlający informacje o autorach projektu. </summary>
    private Button _authorsButton;
    
    /// <summary> Przycisk zamykający grę. </summary>
    private Button _exitButton;

    /// <summary>
    /// Metoda wywoływana przy inicjalizacji skryptu (przed Start).
    /// Pobiera elementy graficzne (przyciski) z dokumentu UI Toolkit na podstawie ich etykiet
    /// oraz przypisuje do nich odpowiednie akcje i delegaty (Eventy).
    /// </summary>
    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
        {
            Debug.LogError("UIDocument not found on MainMenuManager!");
            return;
        }

        var root = _uiDocument.rootVisualElement;

        _playButton = root.Q<Button>("PlayButton");
        _authorsButton = root.Q<Button>("AuthorsButton");
        _exitButton = root.Q<Button>("ExitButton");

        if (_playButton != null) _playButton.clicked += OnPlayClicked;
        if (_authorsButton != null) _authorsButton.clicked += OnAuthorsClicked;
        if (_exitButton != null) _exitButton.clicked += OnExitClicked;
    }

    /// <summary>
    /// Obsługa zdarzenia kliknięcia przycisku 'Play'. 
    /// Ładuje asynchronicznie scenę docelową o nazwie określonej w `gameSceneName`.
    /// </summary>
    private void OnPlayClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Obsługa zdarzenia kliknięcia przycisku 'Authors'.
    /// Wyświetla komunikat w konsoli deweloperskiej z listą autorów gry.
    /// </summary>
    private void OnAuthorsClicked()
    {
        Debug.Log("Authors: Kacper Kaluza & Jakub Krysinski");
    }

    /// <summary>
    /// Obsługa zdarzenia kliknięcia przycisku 'Exit'.
    /// Wychodzi z aplikacji lub – w przypadku działania z poziomu edytora – zatrzymuje odtwarzanie sceny.
    /// </summary>
    private void OnExitClicked()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
