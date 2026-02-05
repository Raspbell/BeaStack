using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

public class TsumSpawner : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _spawnArea;
    [SerializeField] private Tsum _tsumPrefab;

    [SerializeField] private int _defaultCapacity = 50; // 初期サイズ
    [SerializeField] private int _maxSize = 100;

    private TsumData _tsumData;
    private GameUIView _gameUIView;
    private IObjectPool<Tsum> _tsumPool;

    public void Initialize(TsumData tsumData, GameUIView gameUIView)
    {
        _tsumData = tsumData;
        _gameUIView = gameUIView;

        _tsumPool = new ObjectPool<Tsum>(
            createFunc: CreateTsum,
            actionOnGet: OnGetTsum,
            actionOnRelease: OnReleaseTsum,
            actionOnDestroy: OnDestroyTsum,
            collectionCheck: true,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
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

        Tsum tsum = _tsumPool.Get();

        tsum.transform.position = spawnPosition;
        tsum.transform.rotation = Quaternion.identity;

        TsumData.TsumEntity newTsumEntity = _tsumData.GetTsumEntityById(tsumId);
        tsum.Initialize
        (
            newTsumEntity.TsumID,
            newTsumEntity.TsumName,
            _gameUIView,
            newTsumEntity.TsumSprite,
            newTsumEntity.TsumColor,
            newTsumEntity.HighlightColor,
            _tsumPool
        );

        return tsum;
    }

    private Tsum CreateTsum()
    {
        return Instantiate(_tsumPrefab);
    }

    private void OnGetTsum(Tsum tsum)
    {
        tsum.gameObject.SetActive(true);
    }

    private void OnReleaseTsum(Tsum tsum)
    {
        tsum.gameObject.SetActive(false);
    }

    private void OnDestroyTsum(Tsum tsum)
    {
        Destroy(tsum.gameObject);
    }
}