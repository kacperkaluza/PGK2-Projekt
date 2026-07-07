using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Klasa zarządzająca pojawianiem się przeciwników (spawn) na arenie gry.
/// Odpowiada za regularne tworzenie nowych instancji przeciwników, ustalanie punktów ich pojawienia
/// oraz cykliczne zwiększanie trudności rozgrywki (skalowanie statystyk i limitów przeciwników).
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    /// <summary> Prefabrykat obiektu przeciwnika, który będzie klonowany. </summary>
    public GameObject enemyPrefab;
    
    /// <summary> Referencja do transformacji gracza, służąca jako punkt odniesienia do wyznaczania strefy pojawienia się wrogów. </summary>
    public Transform player;
    
    /// <summary> Maksymalna liczba przeciwników mogących jednocześnie przebywać na mapie. </summary>
    public int maxEnemies = 5;
    
    /// <summary> Interwał czasowy (w sekundach) pomiędzy pojawieniem się kolejnych przeciwników. </summary>
    public float spawnRate = 3f;
    
    /// <summary> Minimalna odległość od gracza, w jakiej może pojawić się nowy przeciwnik. </summary>
    public float minDistance = 10f;
    
    /// <summary> Maksymalna odległość od gracza, w jakiej może pojawić się nowy przeciwnik. </summary>
    public float maxDistance = 20f;

    [Header("Difficulty Scaling")]
    /// <summary> Czas (w sekundach), co który poziom trudności gry zostaje automatycznie zwiększony. </summary>
    public float difficultyInterval = 60f;
    
    /// <summary> Liczba przeciwników dodawana do maksymalnego limitu (maxEnemies) po każdym interwale czasowym. </summary>
    public int enemiesToAddPerInterval = 2;
    
    /// <summary> Mnożnik statystyk przeciwników, stosowany po każdym cyklu podnoszenia poziomu trudności (np. 1.1 oznacza 10% przyrostu). </summary>
    public float statsMultiplierPerInterval = 1.1f;

    /// <summary> Odlicza czas od ostatniego pojawienia się przeciwnika. </summary>
    private float timer;
    
    /// <summary> Obecna liczba przeciwników na arenie. </summary>
    private int currentEnemies;
    
    /// <summary> Zegar odmierzający czas do kolejnego podniesienia poziomu trudności gry. </summary>
    private float difficultyTimer;
    
    /// <summary> Skumulowany mnożnik statystyk przydzielany nowo wygenerowanym przeciwnikom. </summary>
    private float currentStatsMultiplier = 1f;

    /// <summary>
    /// Metoda wywoływana co klatkę.
    /// Aktualizuje stany obu liczników czasowych, podnosi poziom trudności gry w wyznaczonych interwałach,
    /// a także wyzwala proces generowania kolejnych przeciwników po upływie odpowiedniego czasu.
    /// </summary>
    void Update()
    {
        timer += Time.deltaTime;
        difficultyTimer += Time.deltaTime;

        // Skalowanie poziomu trudności
        if (difficultyTimer >= difficultyInterval)
        {
            difficultyTimer = 0f;
            maxEnemies += enemiesToAddPerInterval;
            currentStatsMultiplier *= statsMultiplierPerInterval;
        }

        if (timer >= spawnRate && currentEnemies < maxEnemies)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    /// <summary>
    /// Realizuje proces pojawiania się przeciwnika w bezpiecznej strefie uwzględniając siatkę nawigacyjną (NavMesh).
    /// Metoda iteracyjnie poszukuje losowej pozycji o zadanej odległości, sprawdza jej poprawność i dokonuje instancjacji prefabu.
    /// Po wygenerowaniu przeciwnika nakładane są na niego aktualne mnożniki poziomu trudności.
    /// </summary>
    void SpawnEnemy()
    {
        // Losuj pozycję w promieniu od gracza
        for (int i = 0; i < 10; i++) // max 10 prób znalezienia miejsca
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minDistance, maxDistance);
            Vector3 spawnPos = player.position + new Vector3(
                randomCircle.x * distance,
                0f,
                randomCircle.y * distance
            );

            // Sprawdź czy pozycja jest na NavMeshu
            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);
                
                // Zastosowanie modyfikatorów trudności tylko dla nowych wrogów
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.maxHealth = Mathf.RoundToInt(health.maxHealth * currentStatsMultiplier);
                }

                EnemyAI ai = enemy.GetComponent<EnemyAI>();
                if (ai != null)
                {
                    ai.attackDamage *= currentStatsMultiplier;
                }

                currentEnemies++;
                return;
            }
        }
    }

    /// <summary>
    /// Zmniejsza licznik aktualnych wrogów o jeden.
    /// Metoda ta powinna być wywoływana zewnętrznie w momencie śmierci przeciwnika, aby spawner mógł stworzyć nowego w jego miejsce.
    /// </summary>
    public void EnemyDied()
    {
        currentEnemies--;
    }
}