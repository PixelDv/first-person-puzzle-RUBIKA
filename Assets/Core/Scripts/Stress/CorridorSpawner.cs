using System.Collections.Generic;
using UnityEngine;

//TODO
// get lights
// spawn a reverb
public class CorridorTrigger : MonoBehaviour
{
    //VARIABLES
    [Header("Room Settings")]
    public float jumpscare_duration = 1.5f;

    private Transform Out_1;
    private Transform Out_2;
    private string playerTag = "Player";

    void Awake()
    {
        Out_1 = transform.Find("Out1");
        Out_2 = transform.Find("Out2");

        FaceTrigger(Out_1);
        FaceTrigger(Out_2);
    }

    //COLLISIONS
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && StressManager.Instance.jumpscare_active)
        {
            Transform spawn_pos = GetOppositeSpawn(other.transform.position);
            FaceTrigger(spawn_pos);
            StressManager.Instance.fireJumpscare(spawning_type: "spawn_pos", pos: spawn_pos);
        }
    }

    //OPPOSITE SIDE
    private Transform GetOppositeSpawn(Vector3 playerEntryPos) 
    {
        Vector3 localPlayer = transform.InverseTransformPoint(playerEntryPos); //convertit l'orientation du transform du joueur par rapport au local du trigger
        localPlayer.y = 0f; //met à niveau du sol
        localPlayer.Normalize(); 

        Vector3 localOut1 = Out_1.localPosition;
        localOut1.y = 0f;
        localOut1.Normalize();

        Vector3 localOut2 = Out_2.localPosition;
        localOut2.y = 0f;
        localOut2.Normalize();

        //calcule à quelle point deux vecteurs pointent dans la même direction
        float dot1 = Vector3.Dot(localPlayer, localOut1);
        float dot2 = Vector3.Dot(localPlayer, localOut2);

        return (dot1 < dot2) ? Out_1 : Out_2;
    }

    //ROTATION 
    private void FaceTrigger(Transform marker) //s'assure que le fantôme regarde vers le trigger
    {
        Vector3 target = new Vector3(transform.position.x, marker.position.y, transform.position.z);
        marker.LookAt(target);
    }
}