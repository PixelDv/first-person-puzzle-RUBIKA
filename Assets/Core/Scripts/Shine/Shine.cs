using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class Shine : MonoBehaviour
{
    // ==========================================
    // SETTINGS
    // ==========================================
    [Header("🔗 Links")]
    public PersistentCoreManager coreManager;

    [Header("⚙️ Settings")]
    public float ShineDistance = 1f;

    // ==========================================
    // VARIABLES
    // ==========================================
    private List<GameObject> ShineObjects = new List<GameObject>();
    private float onscreenpercentage = 0f;
    private Ray raycast;
    private bool ShineMode = true; 


    // ==========================================
    // MAIN LOGIC
    // ==========================================
    private void Awake()
    {
        raycast = new Ray(coreManager.mainCamera.transform.position, Vector3.zero);
    }

    //update
    private void Update()
    {
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            ShineMode = true;
            checkShineDistance();
        }
        else
        {
            ShineMode = false;
        }

        
    }

    private void OnTriggerEnter(Collider col)
    {   
        if (col.CompareTag("Shine"))
        {
            if (!ShineObjects.Contains(col.gameObject))
            {
                ShineObjects.Add(col.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Shine"))
        {
            if (ShineObjects.Contains(col.gameObject))
            {
                ShineObjects.Remove(col.gameObject);
            }
        }
    }

    // ==========================================
    // FUNCTIONS
    // ==========================================

    public void ToggleShine() //Toggle Shine
    {
        ShineMode = !ShineMode;
    }

    private void checkShineDistance()
    {
        if (ShineObjects.Count > 0)
        {
            Camera cam = coreManager.mainCamera;

            foreach (GameObject shineObject in ShineObjects)
            {
                //check for obstacle
                Vector3 directionToTarget = (shineObject.transform.position - cam.transform.position).normalized;
                raycast.origin = cam.transform.position;
                raycast.direction = directionToTarget;
                
                if (Physics.Raycast(raycast, out RaycastHit hit, ShineDistance))
                {
                    if (hit.collider.gameObject == shineObject)
                    {
                        //get percentage
                        Vector3 viewport_distance = cam.WorldToViewportPoint(shineObject.transform.position);
                        float distance_to_cam = Vector2.Distance(new Vector2(viewport_distance.x, viewport_distance.y), new Vector2(.5f, .5f));
                        float on_screen_percentage = Mathf.InverseLerp(1f, 0f, distance_to_cam);

                        //set variables
                        ShineObject ObjectScript = shineObject.GetComponent<ShineObject>();

                        if (ObjectScript.needed_percentage <= on_screen_percentage)
                        {
                            ObjectScript.catched = true;
                        }
                        else if (ObjectScript.catched && !ObjectScript.persistant)
                        {
                            ObjectScript.catched = false;
                        }

                        //vfx
                        /*if (on_screen_percentage > .7f)
                        {
                            ObjectScript.auraEffect(on_screen_percentage);
                        }*/

                        //debug
                        if (ShineObjects.Count == 1)
                        {
                            onscreenpercentage = on_screen_percentage;
                        }
                    }
                }
            }
        }
    }

    // ==========================================
    // DEBUG
    // ==========================================

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 120, 200, 30), $"Using Shine: {ShineMode}");
        GUI.Label(new Rect(10, 140, 200, 30), $"On Screen Percentage: {onscreenpercentage:F2}");
    }
}