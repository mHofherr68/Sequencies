using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Inventory UI controller (buttons, slot highlights, counts, and short messages).
///
/// Responsibilities:
/// - Hooks up UI buttons to select an <see cref="ItemType"/> in <see cref="InventoryManager"/>.
/// - Displays counts (current/cap) for each item type.
/// - Shows an "empty" overlay when an item count is zero.
/// - Highlights the currently selected item slot.
/// - Displays short, timed UI messages via InventoryManager events (and local fallback messages).
///
/// Integration:
/// - Subscribes to:
///   - <see cref="InventoryManager.OnItemChanged"/> to refresh individual slots.
///   - <see cref="InventoryManager.OnSelectedChanged"/> to update selection highlight.
///   - <see cref="InventoryManager.OnMessage"/> to display messages in the UI.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Slots (Images)")]
    [Tooltip("Slot image for Stones. Alpha is used to indicate selection.")]
    [SerializeField] private Image slotStone;

    [Tooltip("Slot image for Books. Alpha is used to indicate selection.")]
    [SerializeField] private Image slotBook;

    [Tooltip("Slot image for Crosses (stored as Bottle ItemType). Alpha is used to indicate selection.")]
    [SerializeField] private Image slotBottle;

    [Tooltip("Slot image for Drinks. Alpha is used to indicate selection.")]
    [SerializeField] private Image slotDrink;

    [Header("Empty Overlays")]
    [Tooltip("Overlay object shown when there are no Stones in the inventory.")]
    [SerializeField] private GameObject disableStone;

    [Tooltip("Overlay object shown when there are no Books in the inventory.")]
    [SerializeField] private GameObject disableBook;

    [Tooltip("Overlay object shown when there are no Crosses (Bottle) in the inventory.")]
    [SerializeField] private GameObject disableBottle;

    [Tooltip("Overlay object shown when there are no Drinks in the inventory.")]
    [SerializeField] private GameObject disableDrink;

    [Header("Buttons")]
    [Tooltip("Button used to select Stones.")]
    [SerializeField] private Button buttonStone;

    [Tooltip("Button used to select Books.")]
    [SerializeField] private Button buttonBook;

    [Tooltip("Button used to select Crosses (Bottle).")]
    [SerializeField] private Button buttonBottle;

    [Tooltip("Button used to select Drinks.")]
    [SerializeField] private Button buttonDrink;

    [Header("Counts (TMP)")]
    [Tooltip("Count label for Stones (format: current/cap).")]
    [SerializeField] private TMP_Text countStone;

    [Tooltip("Count label for Books (format: current/cap).")]
    [SerializeField] private TMP_Text countBook;

    [Tooltip("Count label for Crosses (Bottle) (format: current/cap).")]
    [SerializeField] private TMP_Text countBottle;

    [Tooltip("Count label for Drinks (format: current/cap).")]
    [SerializeField] private TMP_Text countDrink;

    [Header("Message (TMP)")]
    [Tooltip("Temporary message label shown for inventory feedback (e.g., empty/full).")]
    [SerializeField] private TMP_Text inventoryMessage;

    [Tooltip("Duration (seconds) that inventory messages stay visible.")]
    [SerializeField] private float messageDuration = 1.2f;

    private Coroutine msgRoutine;
    private bool hooked;

    private void OnEnable()
    {
        // Hook UI buttons to item selection.
        Hook(buttonStone, ItemType.Stone);
        Hook(buttonBook, ItemType.Book);
        Hook(buttonBottle, ItemType.Bottle);
        Hook(buttonDrink, ItemType.Drink);

        // Hide message label by default.
        if (inventoryMessage != null)
            inventoryMessage.gameObject.SetActive(false);

        // InventoryManager is created elsewhere; wait until singleton is ready.
        StartCoroutine(EnsureHookedRoutine());
    }

    private void OnDisable()
    {
        // Remove button listeners.
        Unhook(buttonStone);
        Unhook(buttonBook);
        Unhook(buttonBottle);
        Unhook(buttonDrink);

        // Remove InventoryManager event subscriptions.
        UnsubscribeFromInventory();
        hooked = false;
    }

    /// <summary>
    /// Waits until <see cref="InventoryManager.I"/> exists, then subscribes and refreshes UI.
    /// </summary>
    private IEnumerator EnsureHookedRoutine()
    {
        while (isActiveAndEnabled && InventoryManager.I == null)
            yield return null;

        if (!isActiveAndEnabled) yield break;

        SubscribeToInventory();
        RefreshAll();
        OnSelectedChanged(InventoryManager.I.SelectedItem);
    }

    private void SubscribeToInventory()
    {
        if (hooked) return;
        if (InventoryManager.I == null) return;

        InventoryManager.I.OnItemChanged += OnItemChanged;
        InventoryManager.I.OnMessage += ShowMessage;
        InventoryManager.I.OnSelectedChanged += OnSelectedChanged;

        hooked = true;
    }

    private void UnsubscribeFromInventory()
    {
        if (!hooked) return;
        if (InventoryManager.I == null) return;

        InventoryManager.I.OnItemChanged -= OnItemChanged;
        InventoryManager.I.OnMessage -= ShowMessage;
        InventoryManager.I.OnSelectedChanged -= OnSelectedChanged;
    }

    /// <summary>
    /// Adds an onClick listener to select the given <see cref="ItemType"/>.
    /// </summary>
    private void Hook(Button btn, ItemType type)
    {
        if (btn == null) return;
        btn.onClick.AddListener(() => OnClickSelect(type));
    }

    /// <summary>
    /// Removes all onClick listeners for safety (UI is re-hooked on enable).
    /// </summary>
    private void Unhook(Button btn)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Selects an item in the InventoryManager and shows a warning if empty.
    /// </summary>
    private void OnClickSelect(ItemType type)
    {
        if (InventoryManager.I == null) return;

        InventoryManager.I.Select(type);

        if (InventoryManager.I.GetCount(type) <= 0)
            ShowMessage($"Keine {GetGermanItemName(type)} im Inventar!");
    }

    /// <summary>
    /// Inventory callback: refresh UI for one changed item type.
    /// </summary>
    private void OnItemChanged(ItemType type, int count, int cap)
    {
        RefreshOne(type);
    }

    /// <summary>
    /// Refreshes all item slots.
    /// </summary>
    private void RefreshAll()
    {
        RefreshOne(ItemType.Stone);
        RefreshOne(ItemType.Book);
        RefreshOne(ItemType.Bottle);
        RefreshOne(ItemType.Drink);
    }

    /// <summary>
    /// Refreshes count label and empty overlay for a single <see cref="ItemType"/>.
    /// </summary>
    private void RefreshOne(ItemType type)
    {
        if (InventoryManager.I == null) return;

        int c = InventoryManager.I.GetCount(type);
        int cap = InventoryManager.I.GetCap(type);

        bool empty = c <= 0;

        switch (type)
        {
            case ItemType.Stone:
                if (countStone) countStone.text = $"{c}/{cap}";
                if (disableStone) disableStone.SetActive(empty);
                break;

            case ItemType.Book:
                if (countBook) countBook.text = $"{c}/{cap}";
                if (disableBook) disableBook.SetActive(empty);
                break;

            case ItemType.Bottle:
                if (countBottle) countBottle.text = $"{c}/{cap}";
                if (disableBottle) disableBottle.SetActive(empty);
                break;

            case ItemType.Drink:
                if (countDrink) countDrink.text = $"{c}/{cap}";
                if (disableDrink) disableDrink.SetActive(empty);
                break;
        }
    }

    /// <summary>
    /// Inventory callback: update the slot highlight for the currently selected item.
    /// </summary>
    private void OnSelectedChanged(ItemType selected)
    {
        SetSelected(slotStone, selected == ItemType.Stone);
        SetSelected(slotBook, selected == ItemType.Book);
        SetSelected(slotBottle, selected == ItemType.Bottle);
        SetSelected(slotDrink, selected == ItemType.Drink);
    }

    /// <summary>
    /// Applies a "selected" alpha state to a slot image.
    /// </summary>
    private void SetSelected(Image slot, bool selected)
    {
        if (!slot) return;

        var c = slot.color;
        c.a = selected ? 1f : 0.6f;
        slot.color = c;
    }

    /// <summary>
    /// German display names used for UI messages.
    /// Note: Bottle is repurposed as "Kreuze" (crosses).
    /// </summary>
    private static string GetGermanItemName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Stone: return "Steine";
            case ItemType.Book: return "Bücher";
            case ItemType.Bottle: return "Kreuze";
            case ItemType.Drink: return "Getränke";
            default: return type.ToString();
        }
    }

    /// <summary>
    /// Displays a temporary message in the inventory UI.
    /// If another message is currently visible, it is replaced.
    /// </summary>
    private void ShowMessage(string msg)
    {
        if (!inventoryMessage) return;

        if (msgRoutine != null)
            StopCoroutine(msgRoutine);

        msgRoutine = StartCoroutine(MessageRoutine(msg));
    }

    /// <summary>
    /// Shows a message for <see cref="messageDuration"/> seconds, then hides it.
    /// </summary>
    private IEnumerator MessageRoutine(string msg)
    {
        inventoryMessage.gameObject.SetActive(true);
        inventoryMessage.text = msg;

        yield return new WaitForSeconds(messageDuration);

        inventoryMessage.gameObject.SetActive(false);
        msgRoutine = null;
    }
}