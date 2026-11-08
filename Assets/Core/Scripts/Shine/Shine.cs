using System.Collections.Generic;
using UnityEngine;

public class Shine : MonoBehaviour
{
    // VARIABLES
    public bool on = true; 
    private List<ShineObject> ShineObjects = new List<ShineObject>();

    void Update()
    {
        Debug.Log(on);
    }
    
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Shine"))
        {
            if (col.TryGetComponent<ShineObject>(out ShineObject script))
            {
                if (on)
                {
                    script.Catched = true;
                }
                
                ShineObjects.Add(script);
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Shine"))
        {
            if (col.TryGetComponent<ShineObject>(out ShineObject script))
            {
                ShineObjects.Remove(script);
            }
        }
    }

    public void TriggerCheck()
    {
        foreach (ShineObject script in ShineObjects)
        {
            if (script == null) continue;
                
            script.Catched = false;
        }
    }

    public void ToggleShineOn()
    {
        on = true;
    }
    
    public void ToggleShineOff()
    {
        if (on)
        {
            TriggerCheck();
        }
        on = false;
    }
    
}