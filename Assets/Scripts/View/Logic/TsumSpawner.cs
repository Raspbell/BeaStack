using UnityEngine;
using System.Collections.Generic; // Listを使うため

public class TsumSpawner : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _spawnArea;
    [SerializeField] private Tsum _tsumPrefab;

    private TsumData _tsumData;
    private GameUIView _gameUIView;

    public void Initialize(TsumData tsumData, GameUIView gameUIView)
    {
        _tsumData = tsumData;
        _gameUIView = gameUIView;
    }

    public Tsum SpawnTsum(int tsumId)
    {
        if (tsumId < 0)
        {
            return null;
        }

        Vector2 spawnPosition = new Vector2(
            Random.Range(_spawnArea.bounds.min.x, _spawnArea.bounds.max.x),
            Random.Range(_spawnArea.bounds.min.y, _spawnArea.bounds.max.y)
        );

        Tsum tsum = Instantiate(_tsumPrefab, spawnPosition, Quaternion.identity);
        TsumData.TsumEntity newTsumEntity = _tsumData.GetTsumEntityById(tsumId);
        tsum.Initialize
        (
            newTsumEntity.TsumID,
            newTsumEntity.TsumName,
            _gameUIView,
            newTsumEntity.TsumSprite,
            newTsumEntity.TsumColor,
            newTsumEntity.HighlightColor
        );
        return tsum;
    }
}