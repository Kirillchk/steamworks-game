using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUpdate : MonoBehaviour
{
    PlayerMovement plMove;
    TextMeshProUGUI staminaText;
    async void Awake()
    {
        await Task.Delay(1000);
        plMove = FindAnyObjectByType<PlayerMovement>();
        staminaText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        plMove.onStaminaChange += UpdateStamina;
        UpdateStamina(plMove.stamina);
    }
    void UpdateStamina(int staminaValue)
    {
        Debug.Log(staminaText);
        staminaText.text = "Stamina:" + staminaValue.ToString();
    }
}
