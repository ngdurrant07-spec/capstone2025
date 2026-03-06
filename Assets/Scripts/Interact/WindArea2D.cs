using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WindArea2D : MonoBehaviour
{
    [Header("Wind")]
    [SerializeField] private float acceleration = 22f;
    [SerializeField] private bool useTransformRight = true;
    [SerializeField] private Vector2 customDirection = Vector2.right;

    [Header("On/Off Cycle")]
    [SerializeField] private bool cycleEnabled;
    [SerializeField] private float windOnSeconds = 2f;
    [SerializeField] private float windOffSeconds = 1f;
    [SerializeField] private bool startOn = true;

    [Header("Optional Back-And-Forth")]
    [SerializeField] private bool oscillateDirection;
    [SerializeField] private float oscillationPeriod = 2f;
    [SerializeField] private bool startInReverse;

    [Header("Feedback")]
    [SerializeField] private ParticleSystem windParticles;
    [SerializeField] private SpriteRenderer indicatorSprite;
    [SerializeField] private Color activeTint = Color.white;
    [SerializeField] private Color inactiveTint = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private GameObject[] activeStateObjects;
    [SerializeField] private GameObject[] inactiveStateObjects;
    [SerializeField] private bool driveParticleVelocity = true;
    [SerializeField] private float particleVelocityMultiplier = 0.2f;

    readonly HashSet<PlayerScript> players = new HashSet<PlayerScript>();
    bool isWindActive;
    float cycleTimer;

    void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnEnable()
    {
        cycleTimer = 0f;
        SetWindActive(cycleEnabled ? startOn : true, true);
    }

    void Update()
    {
        UpdateCycle();

        Vector2 currentWind = isWindActive ? GetCurrentWindAcceleration() : Vector2.zero;
        UpdateParticleVelocity(currentWind);

        if (players.Count == 0)
            return;

        if (!isWindActive)
            return;

        foreach (PlayerScript player in players)
        {
            if (player == null)
                continue;

            player.SetWindSource(this, currentWind);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerScript player = other.GetComponentInParent<PlayerScript>();
        if (player == null)
            return;

        players.Add(player);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerScript player = other.GetComponentInParent<PlayerScript>();
        if (player == null)
            return;

        RemovePlayer(player);
    }

    void OnDisable()
    {
        SetWindActive(false, true);

        foreach (PlayerScript player in players)
        {
            if (player == null)
                continue;

            player.ClearWindSource(this);
        }

        players.Clear();
    }

    void UpdateCycle()
    {
        if (!cycleEnabled)
        {
            if (!isWindActive)
                SetWindActive(true);
            return;
        }

        cycleTimer += Time.deltaTime;
        float phaseDuration = isWindActive ? Mathf.Max(0.01f, windOnSeconds) : Mathf.Max(0.01f, windOffSeconds);
        if (cycleTimer < phaseDuration)
            return;

        cycleTimer = 0f;
        SetWindActive(!isWindActive);
    }

    Vector2 GetCurrentWindAcceleration()
    {
        Vector2 dir = useTransformRight ? (Vector2)transform.right : customDirection;
        if (dir.sqrMagnitude < 0.0001f)
            return Vector2.zero;
        dir.Normalize();

        float sign = startInReverse ? -1f : 1f;
        if (oscillateDirection && oscillationPeriod > 0.01f)
        {
            float wave = Mathf.Sin((Time.time * Mathf.PI * 2f) / oscillationPeriod);
            sign *= wave >= 0f ? 1f : -1f;
        }

        return dir * (acceleration * sign);
    }

    void RemovePlayer(PlayerScript player)
    {
        if (player == null)
            return;

        if (!players.Remove(player))
            return;

        player.ClearWindSource(this);
    }

    void SetWindActive(bool active, bool force = false)
    {
        if (!force && isWindActive == active)
            return;

        isWindActive = active;
        ApplyFeedback();

        if (isWindActive)
            return;

        foreach (PlayerScript player in players)
        {
            if (player == null)
                continue;
            player.ClearWindSource(this);
        }
    }

    void ApplyFeedback()
    {
        if (windParticles != null)
        {
            if (isWindActive)
            {
                // Restart so each ON phase visibly "kicks in".
                windParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                windParticles.Play(true);
            }
            else if (windParticles.isPlaying)
            {
                windParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (indicatorSprite != null)
            indicatorSprite.color = isWindActive ? activeTint : inactiveTint;

        SetObjectsActive(activeStateObjects, isWindActive);
        SetObjectsActive(inactiveStateObjects, !isWindActive);
    }

    static void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
                continue;
            objects[i].SetActive(active);
        }
    }

    void UpdateParticleVelocity(Vector2 wind)
    {
        if (windParticles == null || !driveParticleVelocity)
            return;

        Vector2 particleVelocity = wind * particleVelocityMultiplier;

        var velocityOverLifetime = windParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(particleVelocity.x);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(particleVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        Vector2 dir = useTransformRight ? (Vector2)transform.right : customDirection;
        if (dir.sqrMagnitude < 0.0001f)
            return;
        dir.Normalize();

        Vector3 origin = transform.position;
        float drawScale = 1.5f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + (Vector3)(dir * drawScale));
        Gizmos.DrawSphere(origin + (Vector3)(dir * drawScale), 0.07f);

        if (!oscillateDirection)
            return;

        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.9f);
        Gizmos.DrawLine(origin, origin - (Vector3)(dir * drawScale));
        Gizmos.DrawSphere(origin - (Vector3)(dir * drawScale), 0.07f);
    }
}
