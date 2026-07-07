using UnityEngine;
using System.Collections;

/// <summary>
/// Klasa zarządzająca punktami życia przeciwnika.
/// Obsługuje logikę otrzymywania obrażeń, odtwarzania efektów wizualnych po trafieniu (miganie) oraz procedurę śmierci, 
/// obejmującą upuszczanie kuli doświadczenia i inkrementację statystyk gracza.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    /// <summary> Maksymalna wartość punktów zdrowia przypisanych do tego przeciwnika. </summary>
    public int maxHealth = 100;
    
    /// <summary> Aktualna, bieżąca liczba punktów zdrowia przeciwnika. </summary>
    public int currentHealth;

    [Header("Exp Orb")]
    /// <summary> Prefabrykat kuli doświadczenia, która ma zostać wygenerowana po śmierci tego przeciwnika. </summary>
    public GameObject expOrbPrefab;

    [Header("Audio")]
    /// <summary> Klip dźwiękowy odtwarzany w momencie zniszczenia/śmierci obiektu. </summary>
    public AudioClip deathSound;
    
    /// <summary> Głośność (od 0 do 1) odtwarzanego dźwięku śmierci. </summary>
    [Range(0f, 1f)] public float deathVolume = 0.5f;

    [Header("Hit Effect")]
    /// <summary> Czas (w sekundach) trwania efektu graficznego (migania) po otrzymaniu obrażeń. </summary>
    public float flashDuration = 0.1f;
    
    /// <summary> Kolor, na jaki zmienia się materiał przeciwnika w momencie trafienia. </summary>
    public Color flashColor = Color.red;
    
    /// <summary> Tablica wszystkich komponentów renderujących przypisanych do tego przeciwnika i jego dzieci. </summary>
    private Renderer[] renderers;
    
    /// <summary> Blok właściwości materiału, używany do optymalnej manipulacji kolorem bez klonowania instancji materiałów (zmniejsza zużycie pamięci). </summary>
    private MaterialPropertyBlock propBlock;
    
    /// <summary> Referencja do aktywnej korutyny obsługującej miganie koloru (pozwala na jej przerwanie przy ponownym trafieniu). </summary>
    private Coroutine flashCoroutine;
    
    /// <summary> Referencja do komponentu wizualnego paska zdrowia przydzielonego temu przeciwnikowi. </summary>
    private EnemyHealthBar healthBar;

    /// <summary>
    /// Metoda wywoływana przed pierwszą klatką. Konfiguruje początkowe punkty zdrowia,
    /// inicjuje komponenty paska zdrowia oraz pobiera renderery potrzebne do efektu trafienia.
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<EnemyHealthBar>();
        if (healthBar != null)
            healthBar.UpdateBar(currentHealth, maxHealth);

        renderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Zmniejsza poziom zdrowia przeciwnika na podstawie zadanych obrażeń.
    /// Odświeża pasek zdrowia, wywołuje efekt graficznego "migania" (hit feedback) i sprawdza warunek śmierci.
    /// </summary>
    /// <param name="damage">Ilość obrażeń, które mają zostać odjęte od puli życia.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthBar != null)
            healthBar.UpdateBar(currentHealth, maxHealth);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());

        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Korutyna obsługująca proces graficznego migania przeciwnika po otrzymaniu obrażeń.
    /// Zmienia kolor w oparciu o MaterialPropertyBlock, a po upływie `flashDuration` resetuje go do normy.
    /// </summary>
    /// <returns>Obiekt zatrzymujący wykonanie skryptu zależny od czasu `flashDuration`.</returns>
    private IEnumerator FlashRoutine()
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", flashColor);
            propBlock.SetColor("_BaseColor", flashColor);
            r.SetPropertyBlock(propBlock);
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.SetPropertyBlock(null);
        }
    }

    /// <summary>
    /// Przeprowadza procedurę śmierci przeciwnika: 
    /// odtwarza dźwięk, komunikuje się ze spawnerem w celu pomniejszenia licznika wrogów,
    /// aktualizuje statystyki zabójstw gracza, wypluwa "kulkę doświadczenia" (ExpOrb) i na końcu usuwa samego siebie ze sceny.
    /// </summary>
    void Die()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathVolume);
        }

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
            spawner.EnemyDied();

        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null)
            playerStats.AddKill();

        if (expOrbPrefab != null)
            Instantiate(expOrbPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

        Destroy(gameObject);
    }
}
