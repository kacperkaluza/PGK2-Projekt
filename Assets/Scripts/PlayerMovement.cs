using UnityEngine;

/// <summary>
/// Klasa odpowiedzialna za poruszanie się gracza oraz zachowanie jego fizyki w świecie gry.
/// Obejmuje obsługę wejścia, przemieszczanie, obrót oraz kolizje ze ścianami i przeciwnikami.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    /// <summary> Prędkość poruszania się gracza. </summary>
    public float moveSpeed = 5f;
    
    /// <summary> Prędkość obrotu gracza w kierunku ruchu. </summary>
    public float rotateSpeed = 10f;

    /// <summary> Referencja do transformacji głównej kamery, używana do obliczania relatywnego wektora ruchu. </summary>
    public Transform cameraTransform;

    /// <summary> Komponent odpowiedzialny za zachowanie fizyczne gracza. </summary>
    private Rigidbody rb;
    
    /// <summary> Wektor przechowujący aktualny, znormalizowany kierunek poruszania się gracza. </summary>
    private Vector3 moveDirection;

    /// <summary>
    /// Metoda wywoływana przed pierwszą klatką. Inicjalizuje komponent Rigidbody przypisany do obiektu.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Metoda wywoływana w każdej klatce. Służy do pobierania danych wejściowych od gracza
    /// i na ich podstawie obliczania kierunku ruchu (relatywnie do rotacji kamery).
    /// </summary>
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        
        // Ignorujemy kąt nachylenia kamery, poruszamy się w poziomie
        camForward.y = 0f;
        camRight.y = 0f;

        moveDirection = (camForward * vertical + camRight * horizontal).normalized;
    }

    /// <summary>
    /// Metoda wywoływana w stałych odstępach czasu (zgodnie z silnikiem fizyki).
    /// Odpowiada za aktualizację pozycji i rotacji obiektu poprzez modyfikację Rigidbody,
    /// a także za zapobieganie przepychaniu gracza przez przeciwników.
    /// </summary>
    void FixedUpdate()
    {
        // Obliczanie rotacji do kierunku ruchu
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        }

        // Aktualizacja pozycji
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        // Ograniczenie pozycji gracza do granic areny
        Vector3 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, -75f, 75.1f);
        pos.z = Mathf.Clamp(pos.z, -75f, 75.1f);
        pos.y = Mathf.Max(pos.y, 8.58f);
        rb.position = pos;

        // Blokowanie przepychania gracza przez przeciwników (Enemy)
        Collider[] hits = Physics.OverlapSphere(rb.position, 0.8f);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 pushDir = rb.position - hit.transform.position;
                pushDir.y = 0f;
                if (pushDir != Vector3.zero)
                {
                    Vector3 safePos = rb.position + pushDir.normalized * 0.05f;
                    
                    // Bezpieczna pozycja uwzględnia zmniejszone marginesy zapobiegające błędnym kolizjom
                    safePos.x = Mathf.Clamp(safePos.x, -24.5f, 24.5f);
                    safePos.z = Mathf.Clamp(safePos.z, -24.5f, 24.5f);
                    rb.MovePosition(safePos);
                }
            }
        }
    }
}