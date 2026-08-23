using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "waveSpawnData", menuName ="Data/WaveSpawnData")]
public class WaveSpawnData : ScriptableObject
{
    [System.Serializable]
    public class WaveEntry
    {
        public int wave;
        public List<Vector3> positions = new();
    }

    public List<WaveEntry> waves = new();
    private Dictionary<int, List<Vector3>> usingWaves;

    public List<Vector3> GetPositions(int wave)
    {
        if (usingWaves == null) BuildLookup();
        int clampedWave = Mathf.Clamp(wave, 1, waves.Count > 0 ? waves[waves.Count - 1].wave : 1);
        return usingWaves.TryGetValue(clampedWave, out var positions) ? positions : new List<Vector3>();
    }

    public bool IsFinalIndex(int index)
    {
        if (waves.Count <= index)
            return true;
        else
            return false;
    }

    private void BuildLookup()
    {
        usingWaves = new Dictionary<int, List<Vector3>>();
        foreach (var entry in waves)
            usingWaves[entry.wave] = entry.positions;
    }
}
