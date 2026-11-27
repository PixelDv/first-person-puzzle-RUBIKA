using UnityEngine;

public class ShineObject : MonoBehaviour
{
    // ==========================================
    // SETTINGS
    // ==========================================
    [Header("⚙️ Settings")]
    public bool persistant = false;
    [Range(0f, 1f)]
    public float needed_percentage = 0.8f;
    public GameObject auraParticles;

    // ==========================================
    // VARIABLES
    // ==========================================
    [HideInInspector] public bool catched = false;
    private MeshRenderer meshRenderer;
    //particles
    [HideInInspector] public ParticleSystem ps_main;
    [HideInInspector] public ParticleSystem ps_core;
    [HideInInspector] public ParticleSystem ps_glow;
    [HideInInspector] public ParticleSystem ps_sparks; 

    // ==========================================
    // MAIN LOGIC
    // ==========================================
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if(auraParticles != null)
        {
            GameObject spawnedAura = Instantiate(auraParticles, transform);
            spawnedAura.transform.localPosition = Vector3.zero;

            ps_main = spawnedAura.GetComponent<ParticleSystem>();
            ps_core = ps_main.transform.Find("core")?.GetComponentInChildren<ParticleSystem>();
            ps_glow = ps_main.transform.Find("glow")?.GetComponentInChildren<ParticleSystem>();
            ps_sparks = ps_main.transform.Find("sparks")?.GetComponentInChildren<ParticleSystem>();

        }
    }

    //update
    private void Update()
    {
        if(catched)
        {
            meshRenderer.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }

    // ==========================================
    // FUNCTIONS
    // ==========================================

    public void auraEffect(float percentage = 0f)
    {
        if (!ps_main.isPlaying)
        {
            ps_main.Play();
            ps_core.Play();
            ps_glow.Play();
            ps_sparks.Play();
        }

        float normalized_percent = Mathf.InverseLerp(0.2f, 1.0f, percentage);

        ps_main.customData.SetVector(ParticleSystemCustomData.Custom1, 0, normalized_percent);
        ps_glow.customData.SetVector(ParticleSystemCustomData.Custom1, 0, normalized_percent);
        ps_core.startSize = normalized_percent;
    }

    // ==========================================
    // DEBUG
    // ==========================================

    private void OnGUI()
    {
        //GUI.Label(new Rect(10, 120, 200, 30), $"Catched: {catched}");
    }

    public void onCatch()
    {
        Debug.Log("Catched !!!");
    }
}