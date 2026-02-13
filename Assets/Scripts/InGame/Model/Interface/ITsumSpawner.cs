using UnityEngine;

namespace InGame.Model.Interface
{
    public interface ITsumSpawner
    {
        Vector2 GetRandomSpawnPosition();
        ITsumView SpawnTsum(int tsumId, Vector2 position, float radius, Sprite sprite, Color color, Color highlightColor);
    }
}