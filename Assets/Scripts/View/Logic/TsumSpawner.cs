using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using Model.Interface;

namespace View.Logic
{
    public class TsumSpawner : MonoBehaviour, ITsumSpawner
    {
        [SerializeField] private BoxCollider2D _spawnArea;
        [SerializeField] private Tsum _tsumPrefab;

        [SerializeField] private int _defaultCapacity = 50;
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

        public ITsum SpawnTsum(int tsumId)
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

        public ITsum SpawnRandomFallingTsum(int maxLevelIndex)
        {
            int randomIdx = UnityEngine.Random.Range(0, maxLevelIndex + 1);
            if (randomIdx >= _tsumData.TsumEntities.Length)
            {
                randomIdx = 0;
            }
            int tsumId = _tsumData.TsumEntities[randomIdx].TsumID;

            return SpawnTsum(tsumId);
        }

        public ITsum SpawnTsumAt(int tsumId, Vector3 position)
        {
            if (tsumId < 0)
            {
                return null;
            }

            Tsum tsum = _tsumPool.Get();
            tsum.transform.position = position;
            tsum.transform.rotation = Quaternion.identity;

            TsumData.TsumEntity newTsumEntity = _tsumData.GetTsumEntityById(tsumId);

            tsum.Initialize(
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
            return Instantiate(_tsumPrefab, this.transform);
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
}