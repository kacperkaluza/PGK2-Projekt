using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Klasa zarządzająca paskiem zdrowia (UI Slider) przypisanym do przeciwnika.
/// Odpowiada za aktualizację wyświetlanej wartości oraz sprawia, by pasek zawsze był skierowany przodem do kamery gracza (efekt billboardingu).
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    /// <summary> Referencja do komponentu Slider z UI, który wizualnie reprezentuje poziom zdrowia. </summary>
    public Slider slider;
    
    /// <summary> Referencja do transformacji głównej kamery, używana do poprawnego obracania paska w stronę widza. </summary>
    private Transform cam;

    /// <summary>
    /// Metoda inicjalizacyjna wywoływana przed pierwszą klatką.
    /// Przypisuje referencję do głównej kamery na scenie.
    /// </summary>
    void Start()
    {
        cam = Camera.main.transform;
    }

    /// <summary>
    /// Metoda wywoływana po wszystkich Update() w klatce.
    /// Orientuje pasek zdrowia tak, aby zawsze był skierowany dokładnie przodem do kamery.
    /// </summary>
    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }

    /// <summary>
    /// Aktualizuje wypełnienie paska zdrowia na podstawie aktualnego i maksymalnego stanu punktów życia przeciwnika.
    /// </summary>
    /// <param name="current">Aktualna ilość punktów zdrowia przeciwnika.</param>
    /// <param name="max">Maksymalna ilość punktów zdrowia przeciwnika.</param>
    public void UpdateBar(int current, int max)
    {
        if (slider != null)
            slider.value = (float)current / max;
    }
}