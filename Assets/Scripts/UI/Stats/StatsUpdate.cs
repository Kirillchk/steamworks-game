using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUpdate : MonoBehaviour
{
    StaminaSystem stamSys;
    TextMeshProUGUI staminaText;
    async void Awake()
    {
        await Task.Delay(1000);
        stamSys = FindAnyObjectByType<StaminaSystem>();
        staminaText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        stamSys.onStaminaChange += UpdateStamina;
        UpdateStamina(stamSys.stamina);
    }
    void UpdateStamina(int staminaValue)
    {
        Debug.Log(staminaText);
        staminaText.text = "Stamina:" + staminaValue.ToString();
    }
}
