using System.Collections.Generic;
using UnityEngine;

public class Shine : MonoBehaviour
{
    public bool on = true; 
    private List<ShineObject> ShineObjects = new List<ShineObject>();

    private void Update()
    {
        //UnityEngine.Debug.Log(on);
    }

    private void OnTriggerEnter(Collider col) //Stores every Shine Objects that enters the Hitbox
    {
        if (col.CompareTag("Shine"))
        {
            if (col.TryGetComponent<ShineObject>(out ShineObject script))
            {
                if (!ShineObjects.Contains(script))
                {
                    ShineObjects.Add(script);
                }

                if (on)
                {
                    script.CatchObject();
                }
            }
        }
    }

    private void OnTriggerExit(Collider col) //Remove seens objects
    {
        if (col.CompareTag("Shine"))
        {
            if (col.TryGetComponent<ShineObject>(out ShineObject script))
            {
                ShineObjects.Remove(script);
            }
        }
    }

    public void ToggleShine() //Toggle Shine
    {
        if (on)
        {
            // Turn off: release/hide all objects and reset their positions
            foreach (ShineObject script in ShineObjects)
            {
                if (script != null)
                {
                    script.ReleaseObject();
                }
            }
            on = false;
        }
        else
        {
            on = true;
            // Turn back on: catch/show all objects currently sitting inside the hitbox
            foreach (ShineObject script in ShineObjects)
            {
                if (script != null)
                {
                    script.CatchObject();
                }
            }
        }
    }
}