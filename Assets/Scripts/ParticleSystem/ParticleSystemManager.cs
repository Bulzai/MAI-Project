using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
    // --- Singleton instance ---
    public static ParticleSystemManager Instance { get; private set; }

    private Dictionary<string, ParticleSystem> _particleDict = new Dictionary<string, ParticleSystem>();

    private void Awake()
    {
        // --- Singleton Setup ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keep it between scenes

        // --- Cache all Particle Systems (including inactive) ---
        var allPS = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in allPS)
        {
            _particleDict[ps.gameObject.name] = ps;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += ActivateParticles;
        GameEvents.OnMainGameStateExited += DeactivateParticles;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= ActivateParticles;
        GameEvents.OnMainGameStateExited -= DeactivateParticles;
    }

    private void ActivateParticles()
    {
        foreach (var ps in _particleDict.Values)
        {
            if (ps != null && !ps.isPlaying)
                ps.Play();
        }
    }

    private void DeactivateParticles()
    {
        foreach (var ps in _particleDict.Values)
        {
            if (ps != null && ps.isPlaying)
                ps.Stop();
        }
    }

    // --- Access helpers ---
    public ParticleSystem GetParticleSystem(string name)
    {
        _particleDict.TryGetValue(name, out var ps);
        return ps;
    }
}
