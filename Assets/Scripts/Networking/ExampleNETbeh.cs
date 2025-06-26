using System;
using UnityEngine;
using static NetworkSyncExtensions; 
public class ExampleNETbeh : NetworkActions
{
    [ContextMenu("Test1")]
    void SUS()
    {
        this.Sync(Disentary, 1, 2);
    }
    
    [ContextMenu("Test2")]
    void sus()
    {
        this.Sync(AFAF);
    }
    
    [ContextMenu("Test3")]
    void sosat()
    {
        this.Sync(AFAF);
        this.Sync(Disentary, 1, 2);
        this.Sync(Disentary, 1, 2);
        this.Sync(AFAF);
    }
    
    [CanTriggerSync]
    public void Disentary(int num1, int num2)
        => Debug.Log($"num1{num1} num2{num2}");
        
    [CanTriggerSync]
    public void AFAF()
        => Debug.Log($"triggered nahui");
}
