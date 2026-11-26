using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class porteOuverture : MonoBehaviour
{
    [SerializeField] private Animator myDoor = null;

    [SerializeField] private bool openTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(openTrigger)
            {
                myDoor.Play("OuvertureAscenseur", 0, 0.0f);
                gameObject.SetActive(false);
            }
        }

    }

}
