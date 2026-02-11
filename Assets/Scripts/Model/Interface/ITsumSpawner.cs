using UnityEngine;

namespace Model.Interface
{
    public interface ITsumSpawner
    {
        Vector2 GetRandomSpawnPosition();
        ITsumView SpawnTsum(int tsumId, Vector2 position);
    }
}