using System;
using UnityEngine;

public class StaminaSystem : MonoBehaviour
{
    int _stamina = 100;
    public int stamina
    {
        get => _stamina;
        set
        {
            _stamina = value;
            onStaminaChange?.Invoke(value);
        }
    }
    public int staminaLimit = 100;
    public event Action<int> onStaminaChange;
    public enum StaminaChangeType : byte
    {
        Regeneration,
        Consumption,
        Instant
    }
    float changeValue;
    float timer;
    StaminaChangeType state;
    public void ChangeStaminaState(StaminaChangeType state, float byHow)
    {
        this.state = state;
        changeValue = byHow;
    }
    void Start()
    {
        stamina = staminaLimit;
    }
    void Update()
    {
        switch (state)
        {
            case StaminaChangeType.Regeneration:
                {
                    if (stamina >= staminaLimit)
                        break;
                    timer += Time.deltaTime;
                    if (1 / changeValue <= timer)
                    {
                        stamina += 1;
                        timer = 0;
                    }
                    break;
                }
            case StaminaChangeType.Consumption:
                {
                    timer += Time.deltaTime;
                    if (1 / changeValue <= timer)
                    {
                        stamina -= 1;
                        Debug.Log(stamina);
                        timer = 0;
                    }
                    break;
                }
            case StaminaChangeType.Instant:
                {
                    stamina += (int)changeValue;
                    break;
                }
        }
    }
}
