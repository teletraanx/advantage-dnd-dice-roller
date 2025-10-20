using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
    [Header("References")]
    public DiceSpinner spinner;
    public TMP_Text resultText;
    public TMP_Dropdown diceDropdown;
    public Button rollButton;
    public Transform diceContainer;
    public TMP_InputField quantityInput;
    public TMP_InputField modifierInput;
    public TMP_Text calculatedTotal;
    public Button advantageButton;
    public Button disadvantageButton;
    public Button percentileButton;

    [Header("Limits")]
    public int maxDice = 20;

    [Header("Dice Prefabs")]
    public GameObject d2Prefab;
    public GameObject d4Prefab;
    public GameObject d6Prefab;
    public GameObject d8Prefab;
    public GameObject d10Prefab;
    public GameObject d12Prefab;
    public GameObject d20Prefab;

    private DiceSpinner currentSpinner;
    private int sides = 4; // default initializing dice sides

    void Start()
    {
        rollButton.onClick.AddListener(OnRollClicked);
        advantageButton.onClick.AddListener(OnRollAdvantage);         
        disadvantageButton.onClick.AddListener(OnRollDisadvantage);
        percentileButton.onClick.AddListener(OnRollPercentile);
        diceDropdown.onValueChanged.AddListener(OnDiceSelected);

        if (quantityInput)
        {
            quantityInput.onEndEdit.AddListener(_ => ClampQuantityField());
            quantityInput.text = "1"; 
        }

        if (modifierInput)
        {
            modifierInput.onEndEdit.AddListener(_ => NormalizeModifierField());
            modifierInput.text = "0";
        }

        SpawnDie(d4Prefab, 4);
        UpdateResultText($"Ready (d{sides})");
        UpdateTotalText("-");
    }

    void OnDiceSelected(int index)
    {
        string selected = diceDropdown.options[index].text;
        if (!selected.StartsWith("d")) return;


        if (int.TryParse(selected.Substring(1), out int parsed))
        {
            sides = parsed;

            GameObject prefab = GetPrefabForSides(parsed); // pick prefab
            if (prefab) SpawnDie(prefab, parsed);

            UpdateResultText($"Selected {selected}");
            UpdateTotalText("-");
        }
        
    }

    void OnRollClicked()
    {
        if (currentSpinner) currentSpinner.Roll(); // spin dice visually

        int qty = GetQuantity();
        int mod = GetModifier();
        var rolls = new List<int>(qty);
        int subtotal = 0;

        for (int i = 0; i < qty; i++)
        {
            int r = Random.Range(1, sides + 1);
            rolls.Add(r);
            subtotal += r;
        }

        int total = subtotal + mod;

        UpdateResultText($"Rolled {qty}d{sides}: [{string.Join(", ", rolls)}] = {subtotal}");
        UpdateTotalText(total.ToString());
    }

    void SpawnDie(GameObject prefab, int newSides)
    {
        foreach (Transform child in diceContainer) // clear existing die
            Destroy(child.gameObject);

        GameObject die = Instantiate(prefab, diceContainer); // spawn new die
        currentSpinner = die.GetComponent<DiceSpinner>();
        sides = newSides;
    }

    GameObject GetPrefabForSides(int s)
    {
        switch (s)
        {
            case 4: return d4Prefab;
            case 6: return d6Prefab;
            case 8: return d8Prefab;
            case 10: return d10Prefab;
            case 12: return d12Prefab;
            case 20: return d20Prefab;
            default: return null;
        }
    }

    int GetQuantity()
    {
        if (!quantityInput) return 1;
        if (!int.TryParse(quantityInput.text, out int q)) q = 1;
        q = Mathf.Clamp(q, 1, maxDice);

        if (quantityInput.text != q.ToString()) quantityInput.text = q.ToString(); // normalize input in case input is odd 
        return q;
    }

    int GetModifier()
    {
        if (!modifierInput) return 0;
        string s = modifierInput.text?.Trim() ?? "0";
        if (s.StartsWith("+")) s = s.Substring(1);
        if (!int.TryParse(s, out int m)) m = 0;
        return m;
    }

    void ClampQuantityField()
    {
        if (!quantityInput) return;
        if (!int.TryParse(quantityInput.text, out int q)) q = 1;
        q = Mathf.Clamp(q, 1, maxDice);
        quantityInput.text = q.ToString();
    }

    void NormalizeModifierField()
    {
        if (!modifierInput) return;
        string s = modifierInput.text?.Trim() ?? "0";
        if (s == "" || s == "+" || s == "-") { modifierInput.text = "0"; return; }
        if (s.StartsWith("+")) s = s.Substring(1);
        if (!int.TryParse(s, out int m)) m = 0;
        modifierInput.text = m >= 0 ? $"+{m}" : m.ToString();
    }

    void UpdateResultText(string text)
    {
        if (resultText) resultText.text = text;
        else Debug.Log(text);
    }

    void UpdateTotalText(string text)
    {
        if (calculatedTotal) calculatedTotal.text = text;
        else Debug.Log(text);
    }

    void OnRollAdvantage()
    {
        DoAdvDisRoll(isAdvantage: true);
    }

    void OnRollDisadvantage()
    {
        DoAdvDisRoll(isAdvantage: false);
    }

    void DoAdvDisRoll(bool isAdvantage)
    {
        SetDieType(20);
        SetQuantityField(2);

        if (currentSpinner) currentSpinner.Roll();

        int mod = GetModifier();

        // Roll 2d20
        var rolls = RollNDice(20, 2, out int _);
        int chosen = isAdvantage ? Mathf.Max(rolls[0], rolls[1]) : Mathf.Min(rolls[0], rolls[1]);
        int total = chosen + mod;

        string tag = isAdvantage ? "Advantage" : "Disadvantage";

        UpdateResultText($"Rolled 2d20 ({tag}): [{rolls[0]}, {rolls[1]}] → {chosen}");
        UpdateTotalText(total.ToString());
    }

    void OnRollPercentile()
    {
        SetDieType(10);
        SetQuantityField(2);

        if (currentSpinner) currentSpinner.Roll();

        int mod = GetModifier();

        // Roll 2d10
        var rolls = new List<int>();
        for (int i = 0; i < 2; i++)
        {
            int r = Random.Range(0, 10); //0 - 9 for percentile
            rolls.Add(r);
        }

        int tens = rolls[0] * 10;
        int ones = rolls[1];
        int result = tens + ones;

        if (tens == 0 && ones == 0)
            result = 100;

        int total = result + mod;

        UpdateResultText($"Rolled d100: [{rolls[0]}, {rolls[1]}] → {result}");
        UpdateTotalText(total.ToString());
    }

    List<int> RollNDice(int s, int qty, out int subtotal)
    {
        var rolls = new List<int>(qty);
        subtotal = 0;
        for (int i = 0; i < qty; i++)
        {
            int r = Random.Range(1, s + 1);
            rolls.Add(r);
            subtotal += r;
        }
        return rolls;
    }

    void SetDieType(int s)
    {
        string label = $"d{s}";
        int idx = diceDropdown.options.FindIndex(o => o.text == label);
        if (idx >= 0)
        {
            diceDropdown.value = idx;        
            diceDropdown.RefreshShownValue();
        }
        else
        {
            
            GameObject pf = GetPrefabForSides(s);
            if (pf) SpawnDie(pf, s);
        }
    }

    void SetQuantityField(int q)
    {
        q = Mathf.Clamp(q, 1, maxDice);
        if (quantityInput) quantityInput.text = q.ToString();
    }
}
