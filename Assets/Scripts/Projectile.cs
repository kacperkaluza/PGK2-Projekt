using UnityEngine;

/// <summary>
/// Klasa reprezentująca pocisk wystrzeliwany w świecie gry (zarówno przez gracza, jak i potencjalnie przez przeciwnika).
/// Obsługuje poruszanie się pocisku w stronę zadanego celu, zadawanie obrażeń przy kolizji oraz logikę lifestealu.
/// </summary>
public class Projectile : MonoBehaviour
{
    /// <summary> Prędkość, z jaką porusza się pocisk w stronę celu. </summary>
    public float speed = 15f;
    
    /// <summary> Cel (obiekt), w stronę którego podąża pocisk. </summary>
    private Transform target;
    
    /// <summary> Ilość obrażeń zadawana przez pocisk w momencie trafienia w cel. </summary>
    private float damage;
    
    /// <summary> Referencja do statystyk podmiotu strzelającego (np. gracza), wykorzystywana m.in. do statystyk końcowych i lifestealu. </summary>
    private PlayerStats shooterStats;

    /// <summary>
    /// Inicjuje pocisk przypisując mu cel, zadawane obrażenia oraz referencję do statystyk strzelca.
    /// </summary>
    /// <param name="targetTransform">Transformacja obiektu, który jest celem ataku.</param>
    /// <param name="shooter">Statystyki podmiotu oddającego strzał (gracza).</param>
    /// <param name="damageAmount">Obliczona wartość obrażeń (w tym ewentualne obrażenia krytyczne).</param>
    public void Initialize(Transform targetTransform, PlayerStats shooter, float damageAmount)
    {
        target = targetTransform;
        shooterStats = shooter;
        damage = damageAmount;
    }

    /// <summary>
    /// Metoda wywoływana co klatkę. Aktualizuje pozycję pocisku w locie, weryfikuje czy cel nadal istnieje,
    /// oraz sprawdza dystans do celu celem aplikacji obrażeń po trafieniu.
    /// </summary>
    void Update()
    {
        if (target == null)
        {
            // Przeciwnik zginął zanim pocisk dotarł
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Jeśli jesteśmy wystarczająco blisko celu, zadajemy obrażenia i znikamy
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            if (target.TryGetComponent(out EnemyHealth health))
            {
                int intDamage = Mathf.RoundToInt(damage);
                health.TakeDamage(intDamage);

                if (shooterStats != null)
                {
                    shooterStats.AddDamageDealt(intDamage);
                }

                // Obsługa Lifesteal
                if (shooterStats != null && shooterStats.lifesteal > 0f)
                {
                    float healAmount = damage * shooterStats.lifesteal;
                    shooterStats.Heal(healAmount);
                }
            }
            Destroy(gameObject);
        }
    }
}
