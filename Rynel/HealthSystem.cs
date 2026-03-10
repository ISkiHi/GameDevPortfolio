/// <summary>
/// Manages player health and hunger mechanics.
/// Updates health UI (bar and warning text), handles damage and healing,
/// and manages timed hunger effects using coroutines.
/// Demonstrates singleton pattern, UI integration, and gameplay system design.
/// </summary>

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject healthText;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float lowHealthThreshold = 25f;
    private float currentHealth;

    [Header("Hunger Settings")]
    [SerializeField] private int startingHungerTime = 10;
    [SerializeField] private int hungerDamage = 2;
    private float currentHungerTime;
    private Coroutine hungerCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentHungerTime = startingHungerTime;
    }

    #region Health Management

    public void DamagePlayer(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            StartCoroutine(PlayerMovement.Instance.SpawnRespawn());
            currentHealth = maxHealth;
            UpdateHealthUI();
            healthText.SetActive(false);
        }
        else if (currentHealth < lowHealthThreshold)
        {
            healthText.SetActive(true);
            StartHungerRoutine();
        }
        else
        {
            StartHungerRoutine();
        }
    }

    public bool HealAllowed() => currentHealth < maxHealth;

    public void HealPlayer(int healAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0f, maxHealth);
        UpdateHealthUI();
        currentHungerTime = startingHungerTime;

        if (currentHealth > lowHealthThreshold)
            healthText.SetActive(false);
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = currentHealth / maxHealth;
    }

    #endregion

    #region Hunger System

    public void StartHungerRoutine()
    {
        if (hungerCoroutine != null)
            StopCoroutine(hungerCoroutine);

        hungerCoroutine = StartCoroutine(HungerLoop());
    }

    private IEnumerator HungerLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            currentHungerTime--;

            if (currentHungerTime <= 0)
            {
                DamagePlayer(hungerDamage);
                currentHungerTime = startingHungerTime;
            }
        }
    }

    public void StopHunger()
    {
        if (hungerCoroutine != null)
        {
            StopCoroutine(hungerCoroutine);
            hungerCoroutine = null;
        }
    }

    #endregion
}