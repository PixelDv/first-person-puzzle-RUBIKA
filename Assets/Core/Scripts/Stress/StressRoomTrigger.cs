using System.Collections.Generic;
using System.Collections;
using UnityEngine;

//TODO
// get lights
// spawn a reverb
public class StressRoomTrigger : MonoBehaviour
{
    //VARIABLES
    [Header("Room Settings")]
    public string room_name = "";
    public int room_activity = 1;
    public int room_multiplier = 1;

    [Header("Jumpscare Settings")]
    public bool is_corridor;
    public float jumpscare_distance = 5;

    [Header("Room Reverb")]
    [Tooltip("Spawn une zone de reverb. -10000 = Off/Padded, -1000 = Small Room, 0 = Living Room, 800 = Hangar, 1800 = Church")]
    [Range(-10000, 2000)]
    public int reverbAmount = -1000;
    private AudioReverbZone reverbZone;

    private List<Light> roomLights = new List<Light>();
    private BoxCollider roomCollider;

    void Awake()
    {
        getLights();
    }

    //COLLISIONS
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {   
            StressManager.Instance.setRoomStats(room_activity, room_multiplier, room_name, roomLights, is_corridor);
            if (StressManager.Instance.jumpscare_active && !is_corridor)
            {
                StressManager.Instance.fireJumpscare("in_front", jumpscare_distance, 5f);
            }
        }
    }

    //LIGHTS
    private void getLights()
    {
        roomCollider = GetComponent<BoxCollider>(); //get collider

        // Calculate world center and half-size of the trigger box
        Vector3 room_center = transform.TransformPoint(roomCollider.center);
        Vector3 room_radius = Vector3.Scale(roomCollider.size * 0.5f, transform.lossyScale);

        // Grab all colliders inside the room, or filter all scene lights by bounds
        Light[] room_lights = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Bounds roomBounds = new Bounds(room_center, room_radius * 2f);

        foreach (Light light in room_lights)
        {
            if (roomBounds.Contains(light.transform.position))
            {
                roomLights.Add(light);
            }
        }
    }

    //REVERB (to do later)
    /*void SetupReverbZone()
    {
        AudioReverbZone reverbZone = gameObject.AddComponent<AudioReverbZone>();
        reverbZone.reverbPreset = AudioReverbPreset.User;
        reverbZone.reverb = reverbAmount;

        if (TryGetComponent<BoxCollider>(out BoxCollider box))
        {
            reverbZone.transform.localPosition = box.center;
            float halfDiagonal = Vector3.Scale(box.size, transform.lossyScale).magnitude * 0.5f;
            reverbZone.minDistance = halfDiagonal * 0.85f;
            reverbZone.maxDistance = halfDiagonal * 1.15f;
        }

    }*/
}