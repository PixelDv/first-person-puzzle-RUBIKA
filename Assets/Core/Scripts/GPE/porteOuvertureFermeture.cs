using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class porteOuvertureFermeture : MonoBehaviour
{
    [SerializeField] private Animator myDoor2 = null;

    [SerializeField] private bool openTrigger = false;
    [SerializeField] private bool closeTrigger = false;
    
    private void OntriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            if (openTrigger)
                myDoor2.Play("DoorOpen", 0, 0.0f);
                    gameObject.SetActive(false);
        }
        else if (closeTrigger)
        {
            myDoor2.Play("DoorClose", 0, 0.0f);
            gameObject.SetActive(false);




        }
    }

    

}
