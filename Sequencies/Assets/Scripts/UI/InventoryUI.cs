using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Slots (Images)")]
    [SerializeField] private Image slotStone;
    [SerializeField] private Image slotBook;
    [SerializeField] private Image slotBottle;
    [SerializeField] private Image slotDrink;

    [Header("Empty Overlays")]
    [SerializeField] private GameObject disableStone;
    [SerializeField] private GameObject disableBook;
    [SerializeField] private GameObject disableBottle;
    [SerializeField] private GameObject disableDrink;

    [Header("Buttons")]
    [SerializeField] private Button buttonStone;
    [SerializeField] private Button buttonBook;
    [SerializeField] private Button buttonBottle;
    [SerializeField] private Button buttonDrink;

    [Header("Counts (TMP)")]
    [SerializeField] private TMP_Text countStone;
    [SerializeField] private TMP_Text countBook;
    [SerializeField] private TMP_Text countBottle;
    [SerializeField] private TMP_Text countDrink;

    [Header("Message (TMP)")]
    [SerializeField] private TMP_Text inventoryMessage;
    [SerializeField] private float messageDuration = 1.2f;

    private Coroutine msgRoutine;
    private bool hooked;

    private void OnEnable()
    {
        Hook(buttonStone, ItemType.Stone);
        Hook(buttonBook, ItemType.Book);
        Hook(buttonBottle, ItemType.Bottle);
        Hook(buttonDrink, ItemType.Drink);

        if (inventoryMessage != null)
            inventoryMessage.gameObject.SetActive(false);

        StartCoroutine(EnsureHookedRoutine());
    }

    private void OnDisable()
    {
        Unhook(buttonStone);
        Unhook(buttonBook);
        Unhook(buttonBottle);
        Unhook(buttonDrink);

        UnsubscribeFromInventory();
        hooked = false;
    }

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

    private void Hook(Button btn, ItemType type)
    {
        if (btn == null) return;
        btn.onClick.AddListener(() => OnClickSelect(type));
    }

    private void Unhook(Button btn)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
    }

    private void OnClickSelect(ItemType type)
    {
        if (InventoryManager.I == null) return;

        InventoryManager.I.Select(type);

        if (InventoryManager.I.GetCount(type) <= 0)
            ShowMessage($"Kein {GetGermanItemName(type)} im Inventar!");
    }

    private void OnItemChanged(ItemType type, int count, int cap)
    {
        RefreshOne(type);
    }

    private void RefreshAll()
    {
        RefreshOne(ItemType.Stone);
        RefreshOne(ItemType.Book);
        RefreshOne(ItemType.Bottle);
        RefreshOne(ItemType.Drink);
    }

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

    private void OnSelectedChanged(ItemType selected)
    {
        SetSelected(slotStone, selected == ItemType.Stone);
        SetSelected(slotBook, selected == ItemType.Book);
        SetSelected(slotBottle, selected == ItemType.Bottle);
        SetSelected(slotDrink, selected == ItemType.Drink);
    }

    private void SetSelected(Image slot, bool selected)
    {
        if (!slot) return;

        var c = slot.color;
        c.a = selected ? 1f : 0.6f;
        slot.color = c;
    }

    // ? Deutsche Anzeige-Namen
    private static string GetGermanItemName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Stone: return "Stein";
            case ItemType.Book: return "Buch";
            case ItemType.Bottle: return "Flasche";
            case ItemType.Drink: return "Getränk";
            default: return type.ToString();
        }
    }

    private void ShowMessage(string msg)
    {
        if (!inventoryMessage) return;

        if (msgRoutine != null)
            StopCoroutine(msgRoutine);

        msgRoutine = StartCoroutine(MessageRoutine(msg));
    }

    private IEnumerator MessageRoutine(string msg)
    {
        inventoryMessage.gameObject.SetActive(true);
        inventoryMessage.text = msg;

        yield return new WaitForSeconds(messageDuration);

        inventoryMessage.gameObject.SetActive(false);
        msgRoutine = null;
    }
}