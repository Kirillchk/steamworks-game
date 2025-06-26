using System;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public event Action OnMicChange;
    string _mic;
    public string mic
    {
        get { return _mic; }
        set
        {
            _mic = value;
            OnMicChange?.Invoke();
        }
    }
}
