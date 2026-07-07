using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Klasa przejściowa (bufor) odpowiedzialna za pobieranie akcji wejścia z Nowego Systemu Inputu Unity.
/// Komunikuje się z modułem InputSystem w celu zebrania wciśniętych przycisków przez gracza i zlecenia ruchu.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    /// <summary> Referencja do komponentu fizycznego powiązanego z poruszanym obiektem. </summary>
    public Rigidbody rb;

    /// <summary> Opcjonalny mnożnik bazowej prędkości poruszania się dla tej implementacji inputu. </summary>
    public float moveSpeed = 5.0f;

    /// <summary> Wskazanie na przypisaną w kontrolerze akcję ruchu dwuwymiarowego (WSAD/Analog). </summary>
    InputAction moveAction;
    
    /// <summary> Wskazanie na przypisaną w kontrolerze akcję skoku (Spacja/A). </summary>
    InputAction jumpAction;
    
    /// <summary> Wskazanie na przypisaną w kontrolerze akcję obrotu kamery/celowania. </summary>
    InputAction lookAction;

    /// <summary> Zmienna zapisująca stan bieżących nacisków klawiszy odpowiedzialnych za kierunek poziomy i pionowy. </summary>
    private Vector2 moveValue;

    /// <summary>
    /// Metoda wywoływana przed pierwszą klatką. Używana do podłączenia wewnętrznych 
    /// InputAction z predefiniowanym systemem poleceń `InputSystem.actions`.
    /// </summary>
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");
    }

    /// <summary>
    /// Metoda wywoływana co klatkę. Sprawdza akcje wyzwalane ręcznie z poziomu kontrolera takie jak skok,
    /// a dodatkowo deleguje czytanie logiki do stałego cyklu fizycznego.
    /// </summary>
    void Update()
    {
        FixedUpdate();

        if (jumpAction.IsPressed())
        {
            // Logika skoku
        }
    }

    /// <summary>
    /// Metoda wywoływana w stałych odstępach czasu. 
    /// Przetwarza (czyta na bieżąco z bufora) wartość wejścia ruchu zapisując je do pamięci na rzecz kontrolera sterującego obiektem.
    /// </summary>
    private void FixedUpdate()
    { 
        moveValue = moveAction.ReadValue<Vector2>();
    } 
}