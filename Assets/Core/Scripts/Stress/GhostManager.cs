using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

/*
TO DO 
  * gore (maybe later)
*/

public class GhostManager : MonoBehaviour
{
    // ==========================================
    // SETTINGS
    // ==========================================
    [Header("👻 Ghost Settings")]
    public List<MeshRenderer> object_pool;
    public List<AudioClip> sound_pool;
    public AudioSource audioSource;
    public DecalProjector blood_prefab;

    // ==========================================
    // VARIABLES
    // ==========================================

    private MeshRenderer currentMesh;
    private bool soundPlayed = false;

    // ==========================================
    // MAIN LOGIC
    // ==========================================
    void Awake()
    {
        currentMesh = object_pool[Random.Range(0, object_pool.Count)];
        currentMesh.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentMesh.isVisible && !soundPlayed)
        {
            soundPlayed = true;
            Debug.Log("PLAYING AUDIO");
            audioSource.clip = sound_pool[Random.Range(0, sound_pool.Count)];
            audioSource.Play();
        }
    }

    // ==========================================
    // FUNCTIONS
    // ==========================================
    
    /*void spawnGore()
    {
        Debug.Log("spawning gore");
        int rayCount = Random.Range(10, 25);
        float maxDistance = 5f;

        for (int i = 0; i < rayCount; i++)
        {
            Vector3 randomDir = Random.onUnitSphere;

            if (Physics.Raycast(transform.position, randomDir, out RaycastHit hit, maxDistance))
            {
                float offset = 0.01f + i * 0.001f;
                Quaternion decalRot = Quaternion.LookRotation(hit.normal, Random.onUnitSphere);
                DecalProjector decalObj = Instantiate(blood_prefab, hit.point + hit.normal * offset, decalRot, transform);
                decalObj.transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
            }
        }
    }*/
}
