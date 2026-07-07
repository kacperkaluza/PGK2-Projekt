using UnityEngine;

/// <summary>
/// Klasa reprezentująca punkt doświadczenia (kulę doświadczenia) upuszczaną przez pokonanych przeciwników.
/// Zarządza logiką przyciągania obiektu w kierunku gracza (gdy znajdzie się w odpowiednim promieniu) oraz procedurą zebrania doświadczenia.
/// </summary>
public class ExpOrb : MonoBehaviour
{
    /// <summary> Wartość punktów doświadczenia, którą kula przyznaje graczowi po zebraniu. </summary>
    public int expValue = 2;
    
    /// <summary> Szybkość, z jaką kula porusza się w kierunku gracza (gdy znajdzie się w strefie przyciągania). </summary>
    public float moveSpeed = 5f;
    
    /// <summary> Bazowy zasięg przyciągania/zebrania kuli przez gracza. </summary>
    public float pickupRange = 1.5f;

    /// <summary> Buforowana referencja do obiektu gracza. </summary>
    private Transform player;

    /// <summary>
    /// Metoda wywoływana przed pierwszą klatką. Odpowiada za zlokalizowanie gracza na scenie za pomocą przypisanego tagu.
    /// </summary>
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    /// <summary>
    /// Metoda wywoływana co klatkę. Sprawdza dystans do gracza z uwzględnieniem jego unikalnych mnożników (pickupRadius).
    /// Kieruje kulę w stronę gracza, jeśli znajduje się on dostatecznie blisko, oraz przyznaje punkty w momencie kolizji/dotarcia.
    /// </summary>
    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        float actualPickupRange = pickupRange;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            // Aplikacja modyfikatora zasięgu podnoszenia z układu statystyk gracza
            actualPickupRange *= stats.pickupRadius;
        }

        // Kulka leci do gracza, gdy znajduje się on w rozszerzonej strefie przyciągania (x3 zasięgu)
        if (dist < actualPickupRange * 3f)
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );

        // Ostateczne zebranie kuli (pickup), gdy osiągnięto dystans docelowy
        if (dist < actualPickupRange)
        {
            if (stats != null)
                stats.AddExp(expValue);
            Destroy(gameObject);
        }
    }
}