using UnityEngine;

/// <summary>
/// Klasa odpowiedzialna za automatyczne ataki gracza skierowane w stronę najbliższego przeciwnika.
/// Zarządza logiką wyszukiwania celów, wystrzeliwaniem pocisków, a także odtwarzaniem dźwięków ataku.
/// Wymaga obecności komponentu PlayerStats na tym samym obiekcie, z którego pobiera statystyki takie jak zasięg, prędkość ataku czy szansa na trafienie krytyczne.
/// </summary>
[RequireComponent(typeof(PlayerStats))] // Wymusza obecność PlayerStats na tym samym obiekcie
public class PlayerAttack : MonoBehaviour
{
    [Header("Ustawienia Detekcji")]
    /// <summary> Warstwa fizyczna, na której znajdują się przeciwnicy, służąca do filtrowania detekcji celów. </summary>
    public LayerMask enemyLayer;

    [Header("Audio")]
    /// <summary> Klip dźwiękowy odtwarzany podczas wykonywania ataku. </summary>
    public AudioClip attackSound;
    
    /// <summary> Głośność odtwarzanego dźwięku ataku (wartość od 0 do 1). </summary>
    [Range(0f, 1f)] public float attackVolume = 0.5f;

    [Header("Projectile")]
    /// <summary> Prefabrykat pocisku, który jest wystrzeliwany w stronę przeciwnika. </summary>
    public GameObject projectilePrefab;
    
    /// <summary> Prędkość, z jaką porusza się wystrzelony pocisk. </summary>
    public float projectileSpeed = 15f;

    /// <summary> Zmienna mierząca czas od ostatniego ataku, używana do weryfikacji gotowości do kolejnego strzału. </summary>
    private float timer;
    
    /// <summary> Referencja do komponentu AudioSource odpowiedzialnego za odtwarzanie dźwięków ataku. </summary>
    private AudioSource audioSource;
    
    /// <summary> Referencja do komponentu PlayerStats, przechowującego aktualne statystyki gracza (obrażenia, zasięg itp.). </summary>
    private PlayerStats stats;
    
    /// <summary> Prealokowany bufor pamięci przechowujący wyniki detekcji sferycznej (kolizje z przeciwnikami), mający na celu uniknięcie alokacji pamięci w trakcie rozgrywki (Garbage Collection). </summary>
    private Collider[] enemyColliders = new Collider[20];

    /// <summary>
    /// Metoda inicjalizacyjna wywoływana przed pierwszą klatką.
    /// Konfiguruje niezbędne referencje do statystyk i źródła dźwięku.
    /// Jeśli komponent AudioSource nie istnieje, jest on dodawany dynamicznie.
    /// </summary>
    void Start()
    {
        stats = GetComponent<PlayerStats>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Metoda wywoływana co klatkę.
    /// Odpowiada za śledzenie upływającego czasu i inicjowanie ataku w odpowiednich interwałach, 
    /// wyliczanych na podstawie statystyki attackSpeed.
    /// </summary>
    void Update()
    {
        timer += Time.deltaTime;

        // Zabezpieczenie przed dzieleniem przez zero na wypadek błędu w Inspektorze
        if (stats.attackSpeed > 0f)
        {
            // Przeliczamy "ataki na sekundę" na "czas między atakami"
            // Np. 1 / 2 ataki na sekundę = 0.5 sekundy opóźnienia
            float currentAttackDelay = 1f / stats.attackSpeed;

            if (timer >= currentAttackDelay)
            {
                timer = 0f;
                AttackNearestEnemy();
            }
        }
    }

    /// <summary>
    /// Główna metoda logiki ataku. Wykorzystuje sferyczne sprawdzanie kolizji w celu odnalezienia przeciwników w zasięgu,
    /// a następnie wybiera tego znajdującego się najbliżej i tworzy pocisk skierowany w jego stronę.
    /// Metoda uwzględnia również statystyki dotyczące trafień krytycznych oraz zarządzanie dźwiękiem ataku.
    /// </summary>
    void AttackNearestEnemy()
    {
        int collidersCount = Physics.OverlapSphereNonAlloc(transform.position, stats.attackRange, enemyColliders, enemyLayer);

        if (collidersCount == 0) return;

        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < collidersCount; i++)
        {
            Collider hit = enemyColliders[i];

            if (hit.CompareTag("Enemy"))
            {
                float sqrDist = (transform.position - hit.transform.position).sqrMagnitude;
                if (sqrDist < minDist)
                {
                    minDist = sqrDist;
                    nearest = hit.gameObject;
                }
            }
        }

        if (nearest != null)
        {
            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound, attackVolume);
            }

            if (nearest.TryGetComponent(out EnemyHealth health))
            {
                GameObject projObj;
                if (projectilePrefab != null)
                {
                    projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                }
                else
                {
                    // Fallback jeśli nie przypisano prefabu - tworzymy prostą kulkę
                    projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    projObj.transform.position = transform.position;
                    projObj.transform.localScale = Vector3.one * 0.3f;
                    Destroy(projObj.GetComponent<Collider>()); // Usuwamy collider, bo opieramy się na dystansie a nie fizyce
                }

                Projectile proj = projObj.GetComponent<Projectile>();
                if (proj == null) proj = projObj.AddComponent<Projectile>();
                
                proj.speed = projectileSpeed;
                
                // Obliczamy obrażenia i ewentualny krytyk
                float finalDamage = stats.attackDamage;
                if (Random.Range(0f, 100f) <= stats.critChance)
                {
                    finalDamage *= stats.critDamage;
                }

                // Inicjujemy pocisk, przekazując stats by obsłużyć Lifesteal
                proj.Initialize(nearest.transform, stats, finalDamage);
            }
        }
    }

    /// <summary>
    /// Metoda silnika Unity, używana do rysowania elementów pomocniczych w oknie Editora.
    /// Wizualizuje za pomocą czerwonej sfery zasięg detekcji celów dla ataków gracza.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (stats != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.attackRange);
        }
    }
}