using UnityEngine;
using UnityEngine.UIElements;
using StarterAssets;

/// <summary>
/// Menedżer systemu ulepszeń i zdobywania poziomów oparty o technologię UI Toolkit.
/// Odpowiada za losowanie puli trzech ulepszeń na karcie, obsługuje ich zatwierdzenie 
/// oraz aplikuje nagrody (zwiększenie odpowiednich statystyk) do postaci gracza, uprzednio wstrzymując czas.
/// </summary>
public class UIUpgradeManager : MonoBehaviour
{
    /// <summary> Referencja do głównego dokumentu interfejsu (UI Toolkit) definiującego ekran nagród. </summary>
    private UIDocument uiDocument;
    
    /// <summary> Tło (VisualElement) pełniące funkcję nakładki zatrzymującej widok podczas wyboru ulepszeń. </summary>
    private VisualElement overlay;
    
    /// <summary> Tablica przycisków reprezentująca trzy wylosowane karty z ulepszeniami. </summary>
    private Button[] cardButtons;
    
    /// <summary> Przycisk umożliwiający zignorowanie nagrody i wyjście z okna ulepszeń bez wyboru. </summary>
    private Button btnIgnore;

    [Header("Referencja do Gracza")]
    /// <summary> Centralne statystyki gracza, które zostaną zmodyfikowane w wyniku przyjęcia konkretnego ulepszenia. </summary>
    public PlayerStats playerStats;
    
    /// <summary> Kontroler wejścia (Input) gracza pochodzący z pakietu StarterAssets, modyfikowany w celu blokady poruszania kamerą w menu. </summary>
    public StarterAssetsInputs playerInputs;

    /// <summary> Wewnętrzna tablica zapamiętująca dane obecnie zaproponowanych graczowi ulepszeń. </summary>
    private UpgradeData[] currentUpgrades;

    /// <summary>
    /// Metoda wywoływana przy uruchamianiu skryptu. Pobiera drzewo interfejsu użytkownika, wiąże logikę kliknięć z odpowiednimi funkcjami
    /// i ukrywa układ wyboru ulepszeń, oczekując na właściwy TriggerLevelUp.
    /// </summary>
    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        overlay = root.Q<VisualElement>("Overlay");
        
        cardButtons = new Button[3];
        cardButtons[0] = root.Q<Button>("Card1");
        cardButtons[1] = root.Q<Button>("Card2");
        cardButtons[2] = root.Q<Button>("Card3");

        btnIgnore = root.Q<Button>("BtnIgnore");

        overlay.style.display = DisplayStyle.None;

        for (int i = 0; i < cardButtons.Length; i++)
        {
            int index = i; 
            cardButtons[i].clicked += () => ApplyUpgrade(index);
        }

        if (btnIgnore != null)
        {
            btnIgnore.clicked += IgnoreOffers;
        }
    }

    /// <summary>
    /// Wywołuje procedurę wejścia w tryb awansu na kolejny poziom.
    /// Zatrzymuje czas gry, aktywuje okno z kartami nagród i uwalnia kursor myszy,
    /// a na końcu uruchamia generator nagród.
    /// </summary>
    public void TriggerLevelUp()
    {
        Time.timeScale = 0f;
        overlay.style.display = DisplayStyle.Flex;

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        if (playerInputs != null)
        {
            playerInputs.cursorInputForLook = false;
        }

        GenerateUpgrades();
    }

    /// <summary>
    /// Losuje trzy niezależne nagrody dla gracza przy wykorzystaniu systemu losowości
    /// rzadkości przedmiotu (Rarity) i typu statystyki (Type), a następnie przypisuje je do komponentów UI kart.
    /// </summary>
    private void GenerateUpgrades()
    {
        currentUpgrades = new UpgradeData[3];
        
        for (int i = 0; i < 3; i++)
        {
            UpgradeType type = (UpgradeType)Random.Range(0, 12);
            UpgradeRarity rarity = GenerateRarity();
            currentUpgrades[i] = CreateUpgradeData(type, rarity);
            SetupCard(cardButtons[i], currentUpgrades[i]);
        }
    }

    /// <summary>
    /// Rzuca wirtualną kością by przydzielić losowy poziom rzadkości kolejnemu wylosowanemu ulepszeniu.
    /// Szanse wynoszą odpowiednio: 50% Common, 30% Uncommon, 15% Rare, 4% Epic, 1% Legendary.
    /// </summary>
    /// <returns>Zwraca wyliczoną rzadkość ulepszenia typu `UpgradeRarity`.</returns>
    private UpgradeRarity GenerateRarity()
    {
        float roll = Random.Range(0f, 100f);
        if (roll < 50f) return UpgradeRarity.Common;
        if (roll < 80f) return UpgradeRarity.Uncommon;
        if (roll < 95f) return UpgradeRarity.Rare;
        if (roll < 99f) return UpgradeRarity.Epic;
        return UpgradeRarity.Legendary;
    }

    /// <summary>
    /// Tworzy nowy nośnik danych ulepszenia (UpgradeData), kalkulując wartość bonusu na bazie 
    /// wyznaczonego typu wzmocnienia i przypisanego mu mnożnika rzadkości. Obejmuje konstruowanie opisów tekstowych UI z systemem formatowania kolorów HEX.
    /// </summary>
    /// <param name="type">Rodzaj wzmocnienia (HP, DMG itd.).</param>
    /// <param name="rarity">Rzadkość wylosowanej karty.</param>
    /// <returns>Gotowy zestaw danych ulepszenia (UpgradeData) przygotowany pod interfejs UI.</returns>
    private UpgradeData CreateUpgradeData(UpgradeType type, UpgradeRarity rarity)
    {
        UpgradeData data = new UpgradeData();
        data.Type = type;
        data.Rarity = rarity;

        float multiplier = 1f;

        switch (rarity)
        {
            case UpgradeRarity.Common: multiplier = 1f; data.Color = new Color(0.8f, 0.8f, 0.8f); break; // Szary
            case UpgradeRarity.Uncommon: multiplier = 1.5f; data.Color = new Color(0.2f, 0.9f, 0.2f); break; // Zielony
            case UpgradeRarity.Rare: multiplier = 2.5f; data.Color = new Color(0.2f, 0.5f, 1f); break; // Niebieski
            case UpgradeRarity.Epic: multiplier = 4f; data.Color = new Color(0.8f, 0.3f, 0.9f); break; // Fioletowy
            case UpgradeRarity.Legendary: multiplier = 7f; data.Color = new Color(1f, 0.8f, 0.2f); break; // Złoty
        }

        string rarityHex = "#" + ColorUtility.ToHtmlStringRGB(data.Color);
        string statColorHex = "#F2D379"; 
        string valString = "";

        switch (type)
        {
            case UpgradeType.AttackDamage:
                data.Value = 5f * multiplier;
                valString = $"+{data.Value:F1}";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Attack Damage</color>";
                break;
            case UpgradeType.AttackSpeed:
                data.Value = 0.5f * multiplier;
                valString = $"+{data.Value:F2}";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Attack Speed</color>";
                break;
            case UpgradeType.MaxHealth:
                data.Value = 20f * multiplier;
                valString = $"+{data.Value:F0}";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Max HP</color>";
                break;
            case UpgradeType.CritChance:
                data.Value = 3f * multiplier; // 3% base
                valString = $"+{data.Value:F1}%";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Crit Chance</color>";
                break;
            case UpgradeType.CritDamage:
                data.Value = 0.2f * multiplier; // 20% base
                valString = $"+{data.Value * 100f:F0}%";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Crit Damage</color>";
                break;
            case UpgradeType.AttackRange:
                data.Value = 0.5f * multiplier; // 0.5m base
                valString = $"+{data.Value:F1}m";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Attack Range</color>";
                break;
            case UpgradeType.MovementSpeed:
                data.Value = 0.1f * multiplier; // 10% base
                valString = $"+{data.Value * 100f:F0}%";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Move Speed</color>";
                break;
            case UpgradeType.HealthRegeneration:
                data.Value = 0.5f * multiplier; // 0.5 HP/s base
                valString = $"+{data.Value:F1} HP/s";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Health Regen</color>";
                break;
            case UpgradeType.Lifesteal:
                data.Value = 0.02f * multiplier; // 2% base
                valString = $"+{data.Value * 100f:F1}%";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Lifesteal</color>";
                break;
            case UpgradeType.Armor:
                data.Value = 1f * multiplier; // 1 armor base
                valString = $"+{Mathf.Round(data.Value)}";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Armor</color>";
                break;
            case UpgradeType.XPGain:
                data.Value = 0.1f * multiplier; // 10% base
                valString = $"+{data.Value * 100f:F0}%";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>XP Gain</color>";
                break;
            case UpgradeType.PickupRadius:
                data.Value = 0.2f * multiplier; // 20% base
                valString = $"+{data.Value * 100f:F0}%";
                data.Description = $"Gain <color={rarityHex}>{valString}</color> <color={statColorHex}>Pickup Radius</color>";
                break;
        }
        
        return data;
    }

    /// <summary>
    /// Synchronizuje fizyczną kartę w drzewie (VisualElement DOM) podanymi argumentami danego ulepszenia (zabarwia krawędzie i przypisuje string).
    /// </summary>
    /// <param name="card">Fizyczny przycisk z UI Toolkit reprezentujący konkretną kartę.</param>
    /// <param name="data">Parametry generujące wizualną otoczkę z modelu klasy (np. kolory i format opisu).</param>
    private void SetupCard(Button card, UpgradeData data)
    {
        var rarity = card.Q<Label>("CardRarity");
        var desc = card.Q<Label>("CardDesc");

        rarity.text = data.Rarity.ToString();
        
        rarity.style.borderTopColor = data.Color;
        rarity.style.borderBottomColor = data.Color;
        rarity.style.borderLeftColor = data.Color;
        rarity.style.borderRightColor = data.Color;
        rarity.style.color = data.Color;

        desc.text = data.Description;
    }

    /// <summary>
    /// Procedura zatwierdzająca wzmocnienie. Zostaje aktywowana przy bezpośrednim kliknięciu w kartę ulepszenia.
    /// Dostosowuje atrybuty gracza na bazie wytypowanej karty nagród, by finalnie wyjść z menu.
    /// </summary>
    /// <param name="index">Indeks w tablicy reprezentujący numer klikniętej karty wzmocnienia.</param>
    private void ApplyUpgrade(int index)
    {
        UpgradeData data = currentUpgrades[index];

        switch (data.Type)
        {
            case UpgradeType.AttackDamage:
                playerStats.attackDamage += data.Value;
                break;
            case UpgradeType.AttackSpeed:
                playerStats.attackSpeed += data.Value; 
                break;
            case UpgradeType.MaxHealth:
                playerStats.maxHealth += data.Value;
                playerStats.currentHealth += data.Value;
                playerStats.UpdateUI();
                break;
            case UpgradeType.CritChance:
                playerStats.critChance += data.Value;
                break;
            case UpgradeType.CritDamage:
                playerStats.critDamage += data.Value;
                break;
            case UpgradeType.AttackRange:
                playerStats.attackRange += data.Value;
                break;
            case UpgradeType.MovementSpeed:
                playerStats.moveSpeedMultiplier += data.Value;
                break;
            case UpgradeType.HealthRegeneration:
                playerStats.healthRegen += data.Value;
                break;
            case UpgradeType.Lifesteal:
                playerStats.lifesteal += data.Value;
                break;
            case UpgradeType.Armor:
                playerStats.armor += Mathf.Round(data.Value);
                break;
            case UpgradeType.XPGain:
                playerStats.xpGain += data.Value;
                break;
            case UpgradeType.PickupRadius:
                playerStats.pickupRadius += data.Value;
                break;
        }

        ClosePanel();
    }

    /// <summary>
    /// Akcja wyzwalana z poziomu przycisku "Ignore", ignorująca wszelkie ulepszenia.
    /// Zamyka po prostu proces level-upu i wznawia grę.
    /// </summary>
    private void IgnoreOffers()
    {
        ClosePanel();
    }

    /// <summary>
    /// Finalna procedura zamykająca powłokę UI (Overlay). Odpina wyświetlane nakładki menu 
    /// oraz przywraca sterowanie, kursor i TimeScale do wartości startowych z właściwej pętli rozgrywki.
    /// </summary>
    private void ClosePanel()
    {
        overlay.style.display = DisplayStyle.None;
        Time.timeScale = 1f; 

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        if (playerInputs != null)
        {
            playerInputs.cursorInputForLook = true;
        }
    }
}

/// <summary>
/// Typ wyliczeniowy reprezentujący wszystkie potencjalne kategorie ulepszeń oferowane graczowi.
/// </summary>
public enum UpgradeType
{
    AttackDamage,
    AttackSpeed,
    MaxHealth,
    CritChance,
    CritDamage,
    AttackRange,
    MovementSpeed,
    HealthRegeneration,
    Lifesteal,
    Armor,
    XPGain,
    PickupRadius
}

/// <summary>
/// Typ wyliczeniowy kategoryzujący stopnie rzadkości dropów i wynikających z tego bonusowych mnożników.
/// </summary>
public enum UpgradeRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Czysty (plain object) kontener danych opisujący jednorazową jednostkę modyfikatora.
/// Kontekstualnie przechowuje pre-kalkulowany tekst interfejsu (Description), konkretną wartość zwiększenia (Value) oraz dopasowany rzadki kolor okienka.
/// </summary>
public class UpgradeData
{
    /// <summary> Docelowy podtyp ulepszanej statystyki. </summary>
    public UpgradeType Type;
    
    /// <summary> Systemowa ranga stopnia rzadkości wylosowanego ulepszenia. </summary>
    public UpgradeRarity Rarity;
    
    /// <summary> Właściwa przeliczona wartość, która posłuży zwiększeniu docelowych statystyk gracza. </summary>
    public float Value;
    
    /// <summary> Opis modyfikatora w formacie RichText, przygotowany do wysłania do Label'a (UI Toolkit). </summary>
    public string Description;
    
    /// <summary> Zakodowana informacja o RGB kolorystyki odpowiadającej stopniu rzadkości. </summary>
    public Color Color;
}