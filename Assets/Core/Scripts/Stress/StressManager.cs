using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StressManager : MonoBehaviour
{
    // VARIABLES
    [Header("⚙️ Settings")]
    public float score_update_timer = 1f;

    // stats
    private int stress = 0;

    // score variables
    private int room_activity = 0;
    private int room_multiplicator = 1;
    private int shine_score = 1;

    // tables
    [Header("📊 Event Tables")]
    public List<StressTier> stressTiers = new();

    [Header("🧣 Events Parameters")]
    //sound fx
    public List<AudioClip> eerieSounds = new();

    // misc
    private float timer = 0f;

    //-------------------------------------------------------

    // INITALIZE TABLES
    void Awake()
    {
        stressTiers = new List<StressTier>
        {
            // 20% Stress
            new StressTier
            {
                maxStress = 20f,
                events = new List<WeightedEvent>
                {
                    CreateEvent("Sound", 5f, PlayNoise),
                    CreateEvent("Door", 5f, CloseDoor),
                    CreateEvent("Light", 0f, LightFlicker),
                    CreateEvent("Jumpscare", 0f, JumpscareSystem),
                    CreateEvent("Nothing", 90f, null)
                }
            },

            // 40% Stress
            new StressTier
            {
                maxStress = 40f,
                events = new List<WeightedEvent>
                {
                    CreateEvent("Sound", 5f, PlayNoise),
                    CreateEvent("Door", 5f, CloseDoor),
                    CreateEvent("Light", 5f, LightFlicker),
                    CreateEvent("Jumpscare", 5f, JumpscareSystem),
                    CreateEvent("Nothing", 80f, null)
                }
            },

            // 60% Stress
            new StressTier
            {
                maxStress = 60f,
                events = new List<WeightedEvent>
                {
                    CreateEvent("Sound", 9f, PlayNoise),
                    CreateEvent("Door", 5f, CloseDoor),
                    CreateEvent("Light", 5f, LightFlicker),
                    CreateEvent("Jumpscare", 16f, JumpscareSystem),
                    CreateEvent("Nothing", 65f, null)
                }
            },

            // 80% - 100%
            new StressTier
            {
                maxStress = 100f, // Set to 100f so it catches any stress from 61% up to 100%
                events = new List<WeightedEvent>
                {
                    CreateEvent("Sound", 5f, PlayNoise),
                    CreateEvent("Door", 5f, CloseDoor),
                    CreateEvent("Light", 10f, LightFlicker),
                    CreateEvent("Jumpscare", 30f, JumpscareSystem),
                    CreateEvent("Nothing", 50f, null)
                }
            }
        };
    }

    // START
    void Start()
    {
        StartCoroutine(EventLoop());
    }

    // UPDATE
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
    }

    // Coroutines
    private IEnumerator EventLoop()
    {
        while (true)
        {
            //Dynamically wait time based on stress
            float waitTime = Mathf.Lerp(10f, 2f, stress / 100f); //prend un temps entre 10 et 2 sec
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

    // DEBUG --------------------------------------------------
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 30), $"Stress: {stress:F1}");
    }

    // FUNCTIONS -----------------------------------------------
    //events
    private void PlayNoise()
    {
        Debug.Log("Playing noise");
        //pick a random sound in a list, play it with randomized pitch. play spatialized somewhere around the player
    }

    private void CloseDoor()
    {
        Debug.Log("Closing a door");
        //pick the closest door to the player, close it while playing a bang sound with randomized pitch
    }

    private void LightFlicker()
    {
        Debug.Log("Flickering the lights!");
        //pick all the lights in the room and make them flicker
    }

    private void JumpscareSystem()
    {
        Debug.Log("Jumpscare");
        //jumpscare mode is on : the dynamic jumpscare system now waits for a trigger to pop one
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