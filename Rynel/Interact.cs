/// <summary>
/// Handles all player interactions with the world:
/// pickups, shops, puzzles, UI, and food/drink consumption.
/// Demonstrates singleton pattern, UI management, and raycast-based interaction.
/// Some referenced managers are not included for readability.
/// </summary>

public class Interact : MonoBehaviour
{
    public static Interact Instance { get; private set; }

    [Header("Scripts")]
    [SerializeField] private Inventory inventory;

    [Header("Control")]
    public bool inputActive = false;

    [Header("Object References")]
    [SerializeField] private GameObject cameraOBJ;
    [SerializeField] private Image crosshair;

    [Header("UI")]
    [SerializeField] private GameObject introDataPad;
    [SerializeField] private GameObject dataPadUI;
    [SerializeField] private GameObject shopUI;
    [SerializeField] private GameObject pauseUI;
    public int uiActive = 0;

    [Header("Ray Distance")]
    [SerializeField] private float rayDistance = 3f;

    [Header("Currency")]
    public int jollers = 200;

    [Header("Riddle")]
    [SerializeField] private GameObject puzzleUI;
    [SerializeField] private GameObject riddlePuzzle;

    [Header("Food & Drink")]
    [SerializeField] private List<string> foodNames = new();
    [SerializeField] private List<string> drinkNames = new();

    private bool dataPadFirstOpen = true;
    private bool dataPadLocked = true;
    private bool firstPickup = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        InputMode.Instance.GameMode();
    }

    private void Update()
    {
        if (!inputActive) return;

        HandleUIInput();
        HandleConsumptionInput();
        HandlePauseInput();
        HandleRaycastInteractions();
    }

    #region UI Input
    private void HandleUIInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (dataPadFirstOpen)
                OpenIntroDataPad();
            else if (!dataPadLocked)
                OpenDataPad();
            else
                OpenPuzzleUI();
        }
    }

    private void OpenIntroDataPad()
    {
        if (uiActive != 0) return;

        introDataPad.SetActive(true);
        AudioManager.Instance.DataPadOpen();
        InputMode.Instance.UIMode();
        Time.timeScale = 0f;
        dataPadFirstOpen = false;
    }

    private void OpenDataPad()
    {
        if (uiActive != 0) return;

        dataPadUI.SetActive(true);
        AudioManager.Instance.DataPadOpen();
        uiActive++;
        InputMode.Instance.UIMode();
        Time.timeScale = 0f;
    }

    private void OpenPuzzleUI()
    {
        if (uiActive != 0) return;

        puzzleUI.SetActive(true);
        riddlePuzzle.SetActive(true);
        InputMode.Instance.UIMode();
        Time.timeScale = 0f;
    }
    #endregion

    #region Food & Drink
    private void HandleConsumptionInput()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;
        if (Time.timeScale == 0) return;

        foreach (var food in foodNames)
        {
            if (inventory.ItemInInventory(food) && HealthSystem.Instance.HealAllowed())
            {
                inventory.RemoveItem(inventory.ItemPosition(food));
                HealthSystem.Instance.HealPlayer(20);
                AudioManager.Instance.Eat();
                if (firstPickup) 
                    ShowFirstPickupHelp();
                return;
            }
        }

        foreach (var drink in drinkNames)
        {
            if (inventory.ItemInInventory(drink) && HealthSystem.Instance.HealAllowed())
            {
                inventory.RemoveItem(inventory.ItemPosition(drink));
                HealthSystem.Instance.HealPlayer(20);
                AudioManager.Instance.Drink();
                return;
            }
        }

        StartCoroutine(HelpTextManager.Instance.DisplayHelpText("Not hungry", 2f));
    }

    private void ShowFirstPickupHelp()
    {
        StartCoroutine(HelpTextManager.Instance.DisplayHelpText("Press F to eat / drink", 3f));
        firstPickup = false;
    }
    #endregion

    #region Pause
    private void HandlePauseInput()
    {
#if UNITY_WEBGL
        if (Input.GetKeyDown(KeyCode.P)) PauseGame();
#else
        if (Input.GetKeyDown(KeyCode.Escape)) PauseGame();
#endif
    }

    private void PauseGame()
    {
        if (Time.timeScale == 0) return;

        InputMode.Instance.UIMode();
        pauseUI.SetActive(true);
        uiActive++;
        Time.timeScale = 0f;
        AudioManager.Instance.DecreaseAmbience();
    }

    public void ResumeGame()
    {
        if (Time.timeScale != 0) return;

        InputMode.Instance.GameMode();
        pauseUI.SetActive(false);
        uiActive--;
        Time.timeScale = 1f;
        AudioManager.Instance.IncreaseAmbience();
    }
    #endregion

    #region Raycast Interactions
    private void HandleRaycastInteractions()
    {
        if (!Physics.Raycast(cameraOBJ.transform.position, cameraOBJ.transform.forward, out RaycastHit hit, rayDistance))
        {
            CrosshairFadeOut();
            return;
        }

        switch (hit.collider.tag)
        {
            case "Pickup":
                HandlePickup(hit);
                break;
            case "Shop":
                HandleShop(hit);
                break;
            case "Puzzle":
                HandlePuzzle(hit);
                break;
            default:
                CrosshairFadeOut();
                break;
        }
    }

    private void HandlePickup(RaycastHit hit)
    {
        CrosshairFadeIn();
        bool hasSpace = inventory.CheckInventorySpace();

        string text = hasSpace
            ? $"Press E to {hit.collider.GetComponent<Items>().objectAction} {hit.collider.GetComponent<Items>().objectName}"
            : $"Not Enough Room for {hit.collider.GetComponent<Items>().objectName}";

        HelpTextManager.Instance.ShowCrosshairText(text, hasSpace);

        if (Input.GetKeyDown(KeyCode.E) && hasSpace)
            inventory.AddToInventory(hit.collider.gameObject);

        if (firstPickup && hasSpace) ShowFirstPickupHelp();
    }

    private void HandleShop(RaycastHit hit)
    {
        CrosshairFadeIn();
        HelpTextManager.Instance.ShowCrosshairText("Press E to Shop", true);

        if (Input.GetKeyDown(KeyCode.E) && uiActive == 0)
        {
            shopUI.SetActive(true);
            uiActive++;
            InputMode.Instance.UIMode();
            Time.timeScale = 0f;
        }
    }

    private void HandlePuzzle(RaycastHit hit)
    {
        CrosshairFadeIn();
        var puzzle = hit.collider.GetComponent<PuzzleBase>();
        HelpTextManager.Instance.ShowCrosshairText($"Press E to {puzzle.puzzleAction} {puzzle.puzzleName}", true);

        if (Input.GetKeyDown(KeyCode.E))
            puzzle.ActivatePuzzle();
    }
    #endregion

    #region Crosshair
    private void CrosshairFadeIn() => crosshair.color = new Color(1f, 0f, 0f, 1f);
    private void CrosshairFadeOut()
    {
        crosshair.color = new Color(1f, 1f, 1f, 0.5f);
        HelpTextManager.Instance.HideCrosshairText();
    }
    #endregion
}