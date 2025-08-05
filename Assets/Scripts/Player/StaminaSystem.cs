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
    int _staminaHealth = 100;
    public int staminaHealth
    {
        get => _staminaHealth;
        set
        {
            _staminaHealth = value;
            onStaminaHealthChange?.Invoke(value);
        }
    }
    public int staminaLimit = 100;
    public int staminaHealthLimit = 100;
    public event Action<int> onStaminaChange;
    public event Action<int> onStaminaHealthChange;
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
        if (state == StaminaChangeType.Instant)
        {
            Debug.Log(byHow);
            stamina += (int)byHow;       
        }
    }
    void Start()
    {
        stamina = staminaLimit;
        staminaHealth = staminaHealthLimit;
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (1 / changeValue <= timer)
        {
            if (state == StaminaChangeType.Regeneration && stamina<staminaLimit)
                stamina++;
            else if (state == StaminaChangeType.Consumption && stamina>0)
                stamina--;
            timer = 0;
        }
    }
}
