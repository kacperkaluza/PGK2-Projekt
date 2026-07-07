using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Klasa realizująca sztuczną inteligencję (AI) przeciwników przy użyciu systemu nawigacji (NavMesh).
/// Zarządza logiką poruszania się do gracza, odtwarzaniem animacji oraz zadawaniem obrażeń wraz z wywoływaniem odrzutu (knockback).
/// </summary>
public class EnemyAI : MonoBehaviour
{
    /// <summary> Referencja do transformacji namierzonego gracza, używana jako docelowy punkt poruszania się. </summary>
    private Transform player; 
    
    /// <summary> Częstotliwość (w sekundach) aktualizowania ścieżki nawigacyjnej w kierunku gracza. </summary>
    public float updateRate = 0.2f;
    
    [Header("Movement")]
    /// <summary> Dystans do gracza (w jednostkach Unity), po przekroczeniu którego przeciwnik zatrzymuje się, by zaatakować. </summary>
    public float stopDistance = 0.8f;

    [Header("Attack Settings")]
    /// <summary> Wartość obrażeń zadawanych graczowi pojedynczym atakiem. </summary>
    public float attackDamage = 10f;
    
    /// <summary> Opóźnienie czasowe (w sekundach) pomiędzy kolejnymi atakami przeciwnika. </summary>
    public float attackRate = 1f; 
    
    /// <summary> Maksymalna odległość pomiędzy graczem a przeciwnikiem, pozwalająca na wyprowadzenie ataku. </summary>
    public float attackRange = 1f; 
    
    /// <summary> Odległość, na jaką gracz zostanie odepchnięty po otrzymaniu trafienia od przeciwnika. </summary>
    public float knockbackDistance = 0.15f; 

    /// <summary> Komponent silnika Unity (NavMeshAgent) odpowiedzialny za wyliczanie i pokonywanie ścieżki do celu. </summary>
    private NavMeshAgent agent;
    
    /// <summary> Licznik mierzący czas od ostatniej aktualizacji ścieżki na NavMeshu. </summary>
    private float pathTimer;
    
    /// <summary> Licznik mierzący czas od ostatniego zadanego ciosu, regulujący szybkość ataków. </summary>
    private float attackTimer;
    
    /// <summary> Komponent Animator sterujący maszynami stanów animacji postaci przeciwnika (chodzenie, spoczynek). </summary>
    private Animator animator;
    
    /// <summary> Zmienna przechowująca poprzedni stan poruszania się, używana do detekcji zmiany stanu animacji w celu oszczędzania wywołań Triggerów. </summary>
    private bool wasMoving;
    
    /// <summary> Buforowana referencja do komponentu PlayerStats przypisanego do gracza. </summary>
    private PlayerStats playerStats;
    
    /// <summary> Buforowana referencja do komponentu CharacterController przypisanego do gracza (używana do aplikacji efektu knockback). </summary>
    private CharacterController playerCC;

    /// <summary>
    /// Metoda inicjalizacyjna wywoływana przed pierwszą klatką.
    /// Wyszukuje gracza w scenie za pomocą tagu "Player" i buforuje referencje do wymaganych dla ataków komponentów.
    /// Inicjuje komponenty agenta nawigacyjnego i animacji.
    /// </summary>
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // 1. AUTOMATYCZNE WYSZUKIWANIE GRACZA PO TAGU
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            // 2. Skoro znaleźliśmy gracza, pobieramy wszystkie jego komponenty do pamięci
            player = playerObj.transform;
            playerStats = playerObj.GetComponent<PlayerStats>();
            playerCC = playerObj.GetComponent<CharacterController>(); 
        }
        else
        {
            // Zabezpieczenie, gdybyś zapomniał ustawić tag na graczu
            Debug.LogError("Wróg zrespił się, ale nie znalazł obiektu z tagiem 'Player' na mapie!");
        }

        if (animator == null)
        {
            Debug.LogWarning("Animator component missing on " + gameObject.name + " or its children!");
        }
    }

    /// <summary>
    /// Metoda wywoływana cyklicznie co klatkę.
    /// Obejmuje całą logikę przeciwnika: przeliczanie trasy w stronę gracza, sprawdzanie bliskości celu,
    /// zarządzanie postojem, nakładanie obrażeń na gracza oraz odtwarzanie animacji ruchu w oparciu o wektor prędkości.
    /// </summary>
    void Update()
    {
        // Zabezpieczenie: Jeśli nie ma gracza, wróg nic nie robi (nie sypie błędami)
        if (player == null) return;

        pathTimer += Time.deltaTime;
        attackTimer += Time.deltaTime;

        Vector3 enemyPosFlat = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 playerPosFlat = new Vector3(player.position.x, 0f, player.position.z);
        float distance = Vector3.Distance(enemyPosFlat, playerPosFlat);

        // --- RUCH ---
        if (pathTimer >= updateRate)
        {
            pathTimer = 0f;
            agent.SetDestination(player.position);
        }

        // --- ZATRZYMANIE ---
        if (distance <= stopDistance)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }

        // --- ATAK I ODRZUT ---
        if (distance <= attackRange)
        {
            if (attackTimer >= attackRate)
            {
                attackTimer = 0f; 
                
                if (playerStats != null)
                {
                    playerStats.TakeDamage(attackDamage);
                }

                if (playerCC != null)
                {
                    Vector3 pushDirection = (player.position - transform.position).normalized;
                    pushDirection.y = 0; 
                    playerCC.Move(pushDirection * knockbackDistance);
                }
            }
        }

        // --- ANIMACJE ---
        bool isMoving = agent.velocity.magnitude > 0.1f;
        if (animator != null && isMoving != wasMoving)
        {
            if (isMoving)
                animator.SetTrigger("walk");
            else
                animator.SetTrigger("stop");

            wasMoving = isMoving;
        }
    }

    /// <summary>
    /// Metoda silnika Unity przeznaczona do wizualizacji w panelu edytora.
    /// Rysuje niebieską sferę oznaczającą dystans wymuszający zatrzymanie (stopDistance)
    /// oraz czerwoną oznaczającą zasięg ataku (attackRange).
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}