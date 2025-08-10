using System.Threading.Tasks;
using TMPro;
using UnityEngine;
public class StatsUpdate : MonoBehaviour
{
    TextMeshProUGUI staminaText;
    TextMeshProUGUI health;
    int previousStamina;
    int previousHealth;
    float timer = 0;
    void Start()
    {
        health = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        staminaText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }
    void FixedUpdate()
    {
        if (timer <= 1)
        {
            timer+=Time.deltaTime;
            return;
        }
        if (StaminaSystem.stamina == previousStamina && StaminaSystem.health == previousHealth)
                return;

        staminaText.text = "Stamina:" + StaminaSystem.stamina.ToString();
        health.text = "Health:" + StaminaSystem.health.ToString();

        previousStamina = StaminaSystem.stamina;
        previousHealth = StaminaSystem.health;
    }
}
