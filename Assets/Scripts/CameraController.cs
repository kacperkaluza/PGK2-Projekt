using UnityEngine;

/// <summary>
/// Klasa zarządzająca zachowaniem kamery w widoku trzecioosobowym (TPP).
/// Odpowiada za podążanie kamery za wskazanym obiektem (graczem), obsługę ruchu myszą
/// oraz zapobieganie przenikaniu kamery przez elementy otoczenia (kolizje kamery).
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    /// <summary> Referencja do obiektu (transformacji), za którym ma podążać kamera (np. gracz). </summary>
    public Transform target;

    [Header("Camera Settings")]
    /// <summary> Domyślna odległość kamery od śledzonego obiektu. </summary>
    public float distance = 10f;
    
    /// <summary> Czułość myszy, określająca szybkość obrotu kamery. </summary>
    public float sensitivity = 2f;

    [Header("Vertical Limits")]
    /// <summary> Minimalny kąt nachylenia kamery w osi X (patrzenie w dół). </summary>
    public float minPitch = -20f;
    
    /// <summary> Maksymalny kąt nachylenia kamery w osi X (patrzenie w górę). </summary>
    public float maxPitch = 80f;

    [Header("Collision")]
    /// <summary> Minimalna odległość, na jaką kamera może zbliżyć się do śledzonego obiektu podczas kolizji ze środowiskiem. </summary>
    public float minDistance = 1.5f;
    
    /// <summary> Dodatkowy margines bezpieczeństwa odległości od przeszkody zapobiegający ucinaniu geometrii otoczenia przez clipping plane kamery. </summary>
    public float clipPadding = 0.2f;

    /// <summary> Zmienna przechowująca aktualny kąt obrotu kamery w poziomie (odchylenie na boki). </summary>
    private float yaw = 0f;
    
    /// <summary> Zmienna przechowująca aktualny kąt nachylenia kamery w pionie (pochylenie w dół/górę). </summary>
    private float pitch = 20f;

    /// <summary>
    /// Metoda inicjalizacyjna wywoływana przed pierwszą klatką.
    /// Konfiguruje ustawienia kursora, blokując go na środku ekranu i ukrywając jego widoczność.
    /// </summary>
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Metoda wywoływana po wykonaniu wszystkich funkcji Update().
    /// Zapewnia płynne śledzenie celu bez drgań, odczytując wejście myszy, przeliczając rotację,
    /// a następnie pozycjonując kamerę. Obejmuje również logikę Linecast zapobiegającą wchodzeniu kamery w ściany.
    /// </summary>
    void LateUpdate()
    {
        if (target == null || Time.timeScale == 0f) return;

        // Odczyt danych z myszy
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        
        // Ograniczenie kąta nachylenia
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Obliczenie docelowej rotacji i przesunięcia
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredOffset = rotation * new Vector3(0f, 0f, -distance);
        Vector3 desiredPos = target.position + desiredOffset;

        // Określenie punktu początkowego promienia (odrobinę nad centrum gracza)
        Vector3 origin = target.position + Vector3.up * 1f;
        Vector3 direction = desiredPos - origin;

        // Weryfikacja, czy linia między obiektem a kamerą jest blokowana przez przeszkody
        if (Physics.Linecast(origin, desiredPos, out RaycastHit hit))
        {
            // Zbliżenie kamery z uwzględnieniem minimalnego dystansu
            float safeDist = Mathf.Max(hit.distance - clipPadding, minDistance);
            transform.position = origin + direction.normalized * safeDist;
        }
        else
        {
            transform.position = desiredPos;
        }

        // Ustawienie kamery tak, by zawsze była skierowana nieco nad obiektem (np. na głowę postaci)
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}