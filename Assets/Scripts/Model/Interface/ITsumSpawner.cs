using UnityEngine;

namespace Model.Interface
{
    public interface ITsumSpawner
    {
        ITsumView SpawnTsumAtRandom(int tsumId);
        ITsumView SpawnTsum(int tsumId, Vector3 position);
    }
}