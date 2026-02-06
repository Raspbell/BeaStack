using UnityEngine;

namespace Model.Interface
{
    public interface ITsumSpawner
    {
        ITsum SpawnTsum(int tsumId);
        ITsum SpawnTsumAt(int tsumId, Vector3 position);
        ITsum SpawnRandomFallingTsum(int maxLevelIndex);
    }
}