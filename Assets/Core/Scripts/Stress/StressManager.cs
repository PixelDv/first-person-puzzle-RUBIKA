using System;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/*
TO DO :
- JUMPSCARE
   - capture mouse velocity
   - active jumpscare
   - triggers
       - corridor turning
       - turning 180°
       - 
   - gore mod
*/


public class StressManager : MonoBehaviour
{
    public static StressManager Instance { get; private set; } //Make singletone


    // ==========================================
    // SETTINGS
    // ==========================================

    [Header("⚙️ Stress Settings")]
    [Tooltip("à quelle fréquence le stress se met à jour (en seconde)")]
    public float score_update_timer = 1f;
    [Tooltip("combien de temps minimum avant qu'un event se lance (haut stress)")]
    public float min_event_time = 3f;
    [Tooltip("combien de temps maximum avant qu'un event se lance (bas stress)")]
    public float max_event_time = 10f;

    [Header("🔊 Sound FX")]
    [Tooltip("prefab audio spatialisé qui a ses propres scripts")]
    public GameObject SpatialAudioEmitter;
    [Tooltip("liste des sons qui vont se jouer de manière aléatoire")]
    public List<AudioClip> EventSounds = new();
    [Tooltip("distance minimum de spawn de sons")]
    public float audio_min_distance = 2f;
    [Tooltip("distance maximum de spawn de sons")]
    public float audio_max_distance = 12f;
    [Tooltip("pour randomisé les sons un peu plus")]
    public float audio_pitch_shift = 0.15f;

    [Header("🚪 Doors")]
    [Tooltip("audio - porte qui se ferme")]
    public AudioClip door_slamming;
    [Tooltip("audio - porte qui s'ouvre'")]
    public AudioClip door_opening;

    [Header("👩♀️ Jumpscare")]
    [Tooltip("-")]
    public GameObject ghost_prefab;
    [Tooltip("distance à laquelle les fantômes reste des murs(en mètres)")]
    public float wall_margin = 0.5f;
    [Tooltip("layer de collisions des murs")]
    public LayerMask worldgeometry_layers = ~0;
    [Tooltip("offset des fantômes pour bien les alignés")]
    public float ghost_rotation = -90f;
    [Tooltip("distance minimum pour un fantôme pour spawn")]
    public float min_spawn_clearance = 6f;
    [Tooltip("à quelle lenteur max le joueur peu se retourner et activé un jumpscare")]
    public float maxUTurnTime = 1.5f;

    // ==========================================
    // VARIABLES
    // ==========================================
    //stress
    private int stress = 0;
    private int room_activity = 0;
    private int room_multiplicator = 1;
    private string room_name;
    private bool in_corridor = false;
    private GameObject current_room;
    private int shine_score = 1;
    private List<StressTier> stressTiers = new();
    //lights
    private List<Light> roomLights = new(); // Change back to Light[] if you originally used an array
    //jumpscares
    [HideInInspector] public bool jumpscare_active = false;
    private Vector3 savedForward;
    private Vector3 lastFrameForward;
    private float turnTimer = 0f;

    private float timer = 0f;
    [HideInInspector] public PersistentCoreManager coreManager;

    // ==========================================
    // START FUNCTIONS
    // ==========================================

    // INITALIZE TABLES
    void Awake()
    {
        Instance = this;
        coreManager = GetComponent<PersistentCoreManager>();
        

        stressTiers = new List<StressTier>
        {
            // 0% - 20% Stress: ~1 subtle cue every 2 minutes (92% Nothing, ~10s roll interval -> ~125s avg)
            new StressTier
            {
                maxStress = 20f,
                events = new List<WeightedEvent>
                {
                    CreateEvent("Sound", 6f, PlayNoise),
                    CreateEvent("Door", 2f, CloseDoor),
                    CreateEvent("Light", 0f, LightFlicker),
                    CreateEvent("Jumpscare", 0f, JumpscareSystem),
                    CreateEvent("Nothing", 92f, null)
                }
            },

            // 21% - 40% Stress: ~1 event every 90 seconds (90% Nothing, ~8.5s roll interval -> ~85s avg)
            new StressTier
            {
                maxStress = 40f,
                events = new List<WeightedEvent>
                {
                    CreateEvent("Sound", 5f, PlayNoise),
                    CreateEvent("Door", 3f, CloseDoor),
                    CreateEvent("Light", 2f, LightFlicker),
                    CreateEvent("Jumpscare", 0f, JumpscareSystem),
                    CreateEvent("Nothing", 90f, null)
                }
            },

            // 41% - 60% Stress: ~1 event every 65 seconds (89% Nothing, ~6.5s roll interval -> ~60s avg)
            new StressTier
            {
                maxStress = 60f,
                events = new List<WeightedEvent>
                {
                    CreateEvent("Sound", 4f, PlayNoise),
                    CreateEvent("Door", 3f, CloseDoor),
                    CreateEvent("Light", 3f, LightFlicker),
                    CreateEvent("Jumpscare", 1f, JumpscareSystem),
                    CreateEvent("Nothing", 89f, null)
                }
            },

            // 61% - 100% Stress: ~1 event every 30-45 seconds (88% Nothing, ~3-4.5s roll interval -> ~35s avg)
            new StressTier
            {
                maxStress = 100f,
                events = new List<WeightedEvent>
                {
                    CreateEvent("Sound", 3f, PlayNoise),
                    CreateEvent("Door", 3f, CloseDoor),
                    CreateEvent("Light", 3f, LightFlicker),
                    CreateEvent("Jumpscare", 3f, JumpscareSystem),
                    CreateEvent("Nothing", 88f, null)
                }
            }
        };
    }

    // START
    void Start()
    {
        StartCoroutine(EventLoop());
    }

    // ==========================================
    // LOOPS
    // ==========================================
    void Update()
    {
        // UPDATE STRESS every 1 second
        timer += Time.deltaTime;
        if (timer >= score_update_timer)
        {
            timer -= score_update_timer; 
            stress += (room_activity + shine_score) * room_multiplicator;
            stress = Mathf.Clamp(stress, 0, 100);
        }

        //TESTING
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            //fireJumpscare("in_front", 10f, 3f);
            jumpscare_active = !jumpscare_active;
        }

        SimpleUTurnCheck();
    }

    // ==========================================
    // DEBUG
    // ==========================================

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 30), $"Stress: {stress:F1}");
        GUI.Label(new Rect(10, 30, 200, 30), $"Room: {room_name}");
        GUI.Label(new Rect(10, 50, 200, 30), $"Jumpscare Active: {jumpscare_active}");
    }

    // CHANGE SCORE -----------------------------------------------
    public void setRoomStats(int activity, int mutliplier, String name, List<Light> allLights, bool corridor)
    {
        room_activity = activity;
        room_multiplicator = mutliplier;
        room_name = name;
        roomLights = allLights;
        in_corridor = corridor;
    }

    public void setStress(int e)
    {
        stress = e;
    }

    // ==========================================
    // EVENTS
    // ==========================================

    // Coroutines
    private IEnumerator EventLoop()
    {
        while (true)
        {
            //Dynamically wait time based on stress
            float waitTime = Mathf.Lerp(max_event_time, min_event_time, stress / 100f); //prend un temps entre 10 et 2 sec
            yield return new WaitForSeconds(waitTime);

            WeightedEvent pickedEvent = PickEvent();

            //Pick an event
            if(pickedEvent != null && pickedEvent.name != "Nothing")
            {
                pickedEvent.onTrigger?.Invoke();
            }
            else
            {
                UnityEngine.Debug.Log("Nothing");
            }
            
        }
    }

    // TABLES LOGIC ------------------------------------------
    private WeightedEvent PickEvent() //
    {
        //find the right stress tier
        StressTier tier = stressTiers.Find(t => stress <= t.maxStress);
        if (tier == null || tier.events == null || tier.events.Count == 0) return null;

        //find the total weight of every event combined
        float totalWeight = 0; 
        foreach (var e in tier.events) totalWeight += e.weight;

        //pick a random weight
        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0;

        //create "weight slots" for each event, picks the of that slot
        foreach (var e in tier.events)
        {
            cumulative += e.weight;
            if (roll <= cumulative) return e;
        }

        return tier.events[0]; //returns the index of the picked action
    }

    //MAIN FUNCTIONS
    private void PlayNoise()
    {
        Debug.Log("Playing noise");
        //pick a random sound in a list, play it with randomized pitch. play spatialized somewhere around the player

        if (EventSounds != null)
        {
            //pick a random sound
            int random_index = UnityEngine.Random.Range(0, EventSounds.Count);
            AudioClip playing_sound = EventSounds[random_index];

            //pick a position around the player camera
            Vector3 cam_pos = coreManager.mainCamera.transform.position;
            Vector3 spawn_pos = cam_pos + new Vector3(cam_pos.x + UnityEngine.Random.Range(audio_min_distance, audio_max_distance), cam_pos.y + UnityEngine.Random.Range(audio_min_distance, audio_max_distance), cam_pos.z + UnityEngine.Random.Range(audio_min_distance, audio_max_distance));

            playSound(spawn_pos, playing_sound);
        }
    }

    private void CloseDoor()
    {
        GameObject nearestDoor = findNearestDoor();

        if (nearestDoor != null && nearestDoor.TryGetComponent<SimpleMover>(out SimpleMover mover))
        {   
            mover.ToggleMove();
            if (mover.isOpen == true)
            {
                Debug.Log($"Slaming door : {nearestDoor.transform.position}");
                playSound(nearestDoor.transform.position, door_slamming);
            }
            if (!mover.isOpen)
            {   
                playSound(nearestDoor.transform.position, door_opening);
                Debug.Log($"Opening door : {nearestDoor.transform.position}");
            }
        }
    }

    private void LightFlicker()
    {
        Debug.Log("Flickering the lights!");
        flickerLights();
    }

    private void JumpscareSystem()
    {
        Debug.Log("Jumpscare");
        //jumpscare mode is on : the dynamic jumpscare system now waits for a trigger to pop one
    }

    //NOISE
    public void playSound(Vector3 spawn_pos, AudioClip playing_sound) //sons supprimé après play !!
    {
        //play a sound
        GameObject emitter = Instantiate(SpatialAudioEmitter, spawn_pos, Quaternion.identity);

        if (emitter.TryGetComponent<AudioSource>(out AudioSource source))
        {
            //audio parameters
            source.clip = playing_sound;
            source.pitch = UnityEngine.Random.Range(1f-audio_pitch_shift, 1f+audio_pitch_shift);
            source.Play();

            Debug.Log($"Playing '{playing_sound.name}' | isPlaying: {source.isPlaying} | Vol: {source.volume} | ListenerVol: {AudioListener.volume}");

            // Destroy the entire spawned GameObject after the clip finishes
            float duration = playing_sound.length / Mathf.Abs(source.pitch);
            Destroy(emitter, duration + 0.2f);
        }
    }

    //LIGHTS
    public void flickerLights()
    {
        foreach (Light light in roomLights)
        {
            if (light != null)
            {
                StartCoroutine(flickerRoutine(light, UnityEngine.Random.Range(1f, 2f))); //change random for flickering duration
            }
        }
    }

    //DOORS
    private GameObject findNearestDoor()
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Doors"); //find all doors
        GameObject closestDoor = null;

        float closestDistance = 10000000000000;

        foreach (GameObject door in doors)
        {
            float distance = Vector3.Distance(door.transform.position, coreManager.mainCamera.transform.position);
            if (closestDistance > distance)
            {
                closestDistance = distance;
                closestDoor = door;
            }
        }

        return closestDoor;
    }

    private IEnumerator flickerRoutine(Light targetLight, float duration) //couroutine qui fais flicker la light
    {
        float originalIntensity = targetLight.intensity;
        float elapsed = 0f;
        float noiseOffset = UnityEngine.Random.Range(0f, 100f);

        while (elapsed < duration)
        {
            float noise = Mathf.PerlinNoise(Time.time * 8f, noiseOffset); //generate a noise (change 8f for light speed)
            targetLight.intensity = Mathf.Lerp(0.1f, originalIntensity, noise); //use noise to change intensity
            elapsed += Time.deltaTime;
            yield return null; //pause until next frame
        }

        targetLight.intensity = originalIntensity; //go back to original intensity
    }

    //JUMPSCARES
    public void fireJumpscare(string spawning_type, float jumpscare_distance = 5f, float jumpscare_duration = 1f, Transform pos = null)
    {
        Transform cam = coreManager.mainCamera.transform;
        Vector3 spawnPos = cam.position;

        //fires a raycast in front of the player, spawn ghost at hit point with a margin (or at max distance if hit nothing)
        if (spawning_type == "in_front")
        {
            Vector3 forwardFlat = cam.forward;
            forwardFlat.y = 0f;
            forwardFlat.Normalize();

            float finalDistance = jumpscare_distance;

            if (Physics.Raycast(cam.position, forwardFlat, out RaycastHit hit, jumpscare_distance + wall_margin, worldgeometry_layers, QueryTriggerInteraction.Ignore))
            {
                finalDistance = Mathf.Max(0f, hit.distance - wall_margin);
            }

            spawnPos = cam.position + forwardFlat * finalDistance;
        }

        //spawn at provided position
        if (spawning_type == "spawn_pos")
        {
            spawnPos = pos.position;
        }

        //snap to the ground
        if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit groundHit, 10f, worldgeometry_layers, QueryTriggerInteraction.Ignore))
        {
            spawnPos.y = groundHit.point.y;
        }

        //Spawn the ghost
        GameObject ghost = Instantiate(ghost_prefab, spawnPos, Quaternion.identity);

        //set looking angle
        if (spawning_type == "in_front")
        {
            // Make the ghost look at the player on its own height level (no tilting up/down)
            Vector3 targetPos = new Vector3(cam.position.x, ghost.transform.position.y, cam.position.z);
            ghost.transform.LookAt(targetPos);
        }

        //take the provided angle
        if (spawning_type == "spawn_pos")
        {
            ghost.transform.rotation = pos.rotation;
        }

        //Model wrong export rotation correction
        ghost.transform.Rotate(0f, ghost_rotation, 0f);

        jumpscare_active = false;
        Destroy(ghost, jumpscare_duration);
    }

    private void SimpleUTurnCheck()
    {
        //si pas jumpscare active, reset direction sauvegardée camera
        if (!jumpscare_active)
        {
            savedForward = Vector3.zero;
            return;
        }

        //normlise vecteur de direction de la camera
        Transform cam = coreManager.mainCamera.transform;
        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();

        //initialise saved forward sur la 1ère frame de l'activation d'attente du jumpscare
        if (savedForward == Vector3.zero)
        {
            savedForward = forward;
            lastFrameForward = forward;
            return;
        }
        
        float dot = Vector3.Dot(savedForward, forward);

        //Si le joueur ne se retourne pas, met à jour 
        if (dot > 0.5f)
        {
            savedForward = Vector3.Lerp(savedForward, forward, Time.deltaTime * 2f).normalized;
        }
        else //check itmer
        {
            turnTimer += Time.deltaTime;
            if (turnTimer > maxUTurnTime) //si trop lent, on reset
            {
                savedForward = forward;
                turnTimer = 0f;
                return;
            }
        }

        float turnSpeed = Vector3.Angle(lastFrameForward, forward) / Time.deltaTime;
        lastFrameForward = forward;

        bool turnedEnough = dot < -0.5f;
        bool settled = turnSpeed < 60f;
        bool hasClearance = !Physics.Raycast(cam.position, forward, min_spawn_clearance, worldgeometry_layers); //check s'il y a pas un mur trop proche et s'il faut pas attendre un distance plus longue pour spawn

        if (turnedEnough && settled && hasClearance)
        {
            savedForward = Vector3.zero;
            jumpscare_active = false;
            fireJumpscare("in_front", min_spawn_clearance, 1.5f);
        }
    }

    //HELPERS
    //create an event in one line instead of multiples
    private WeightedEvent CreateEvent(string eventName, float weight, UnityAction action) 
    {
        WeightedEvent evt = new WeightedEvent
        {
            name = eventName,
            weight = weight,
            onTrigger = new UnityEvent()
        };

        if (action != null)
        {
            evt.onTrigger.AddListener(action);
        }

        return evt;
    }
}

// TABLES SYSTEM
[Serializable]
public class WeightedEvent
{
    public string name;
    public float weight;
    public UnityEvent onTrigger; 
}

[Serializable]
public class StressTier
{
    [Tooltip("Niveau minimum/maximum de stress pour avoir les paramètres")]
    public float maxStress;
    public List<WeightedEvent> events;
}