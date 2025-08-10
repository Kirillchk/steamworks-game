using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using Unity.Burst;
using UnityEngine;

public class StaminaSystem : MonoBehaviour
{
    static int _stamina, _health;
    public static int stamina
    {
        get => _stamina;
        set
        {
            _stamina = value;
            _stamina = Mathf.Clamp(_stamina, 0, staminaLimit);
            if (_stamina <= 0)
            {
                foreach (var item in staminaPerSecondList)
                {
                    if (item.Item3 == null && item.Item1!="staminaBaseReg" && !healthPerSecondList.Contains(item))
                    {
                        healthPerSecondList.Add(item);
                    }
                }
            }
        }
    }
    public static int health
    {
        get => _health;
        set
        {
            _health = value;
            _health = Mathf.Clamp(_health, 0, healthLimit);
        }
    }
    public static int staminaLimit = 100, healthLimit = 100;
    public static float staminaPerSecond = 0, healthPerSecond = 0;
    //name, value, useStamina? null - if u want to switch to health when stamina = 0
    public static ObservableCollection<(string, float, bool?)> staminaPerSecondList = new();
    public static ObservableCollection<(string, float, bool?)> healthPerSecondList = new();
    static float staminaTimer = 0, healthTimer = 0, waitForRegTimer = 0;
    float waitForReg = .5f;
    void Start()
    {
        stamina = staminaLimit;
        health = healthLimit;
        staminaPerSecondList.CollectionChanged += staminaPerSecondListChanged;
        healthPerSecondList.CollectionChanged += healthPerSecondListChanged;
    }
    void FixedUpdate()
    {
        if (staminaPerSecondList.Count == 0)
        {
            waitForRegTimer += Time.deltaTime;
            if (waitForReg <= waitForRegTimer)
                staminaPerSecondList.Add(("staminaBaseReg", 10, true));
        }
        else
            waitForRegTimer = 0;

        if (staminaPerSecond != 0)
        {
            staminaTimer += Time.deltaTime;
            if (1 / Mathf.Abs(staminaPerSecond) <= staminaTimer)
            {
                stamina += (int)System.Math.Truncate(staminaTimer / (1 / staminaPerSecond));
                staminaTimer = 0;
            }
        }

        if (healthPerSecond != 0)
        {
            healthTimer += Time.deltaTime;
            if (1 / Mathf.Abs(healthPerSecond) <= healthTimer)
            {
                health += (int)System.Math.Truncate(healthTimer / (1 / healthPerSecond));
                healthTimer = 0;
            }
        }
    }
    public static void Instant(float byHow)
    {
        if (stamina > 0)
            stamina += (int)byHow;
        else
            health += (int)byHow;
        if (byHow < 0)
        {
            if(staminaPerSecondList.Contains(("staminaBaseReg", 10, true)))
                staminaPerSecondList.Remove(("staminaBaseReg", 10, true));
            waitForRegTimer = 0;
        }
    }
    void staminaPerSecondListChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (staminaPerSecondList.Count > 1)
            staminaPerSecondList.Remove(("staminaBaseReg", 10, true));
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    foreach (var item in e.NewItems)
                    {
                        var tuple = ((string, float, bool?))item;
                        staminaPerSecond += tuple.Item2;
                    }
                    break;
                }
            case NotifyCollectionChangedAction.Remove:
                {
                    foreach (var item in e.OldItems)
                    {
                        var tuple = ((string, float, bool?))item;
                        staminaPerSecond -= tuple.Item2;
                        if (healthPerSecondList.Contains(tuple))
                            healthPerSecondList.Remove(tuple);
                    }
                    break;
                }
        }
    }
    void healthPerSecondListChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add: 
                {
                    foreach (var item in e.NewItems)
                    {
                        var tuple = ((string, float, bool?))item;
                        healthPerSecond += tuple.Item2;
                    }
                    break;
                }
            case NotifyCollectionChangedAction.Remove:
                {
                    foreach (var item in e.OldItems)
                    {
                        var tuple = ((string, float, bool?))item;
                        healthPerSecond -= tuple.Item2;
                    }
                    break;
                }
        }
    }
}
