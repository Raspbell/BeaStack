using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using Model.Interface;

namespace View
{
    public class TsumSpawner : MonoBehaviour, ITsumSpawner
    {
        [SerializeField] private BoxCollider2D _spawnArea;
        [SerializeField] private TsumView _tsumPrefab;

        [SerializeField] private int _defaultCapacity = 50;
        [SerializeField] private int _maxSize = 100;

        private TsumData _tsumData;
        private GameUIView _gameUIView;
        private IObjectPool<TsumView> _tsumPool;

        public void Initialize(TsumData tsumData, GameUIView gameUIView)
        {
            _tsumData = tsumData;
            _gameUIView = gameUIView;

            _tsumPool = new ObjectPool<TsumView>(
                createFunc: CreateTsum,
                actionOnGet: OnGetTsum,
                actionOnRelease: OnReleaseTsum,
                actionOnDestroy: OnDestroyTsum,
                collectionCheck: true,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxSize
            );
        }

        public ITsumView SpawnTsumAtRandom(int tsumId)
        {
            if (tsumId < 0)
            {
                return null;
            }

            Vector2 spawnPosition = new Vector2(
                Random.Range(_spawnArea.bounds.min.x, _spawnArea.bounds.max.x),
                Random.Range(_spawnArea.bounds.min.y, _spawnArea.bounds.max.y)
            );

            ITsumView newTsumView = GetTsumFromPool(tsumId, spawnPosition);
            return newTsumView;
        }

        public ITsumView SpawnTsum(int tsumId, Vector3 position)
        {
            if (tsumId < 0)
            {
                return null;
            }
            ITsumView newTsumView = GetTsumFromPool(tsumId, position);
            return newTsumView;
        }

        private ITsumView GetTsumFromPool(int tsumId, Vector3 position)
        {
            TsumView tsum = _tsumPool.Get();
            tsum.transform.position = position;
            tsum.transform.rotation = Quaternion.identity;

            var entityData = _tsumData.GetTsumComponentById(tsumId);

            tsum.Initialize(
                entityData.TsumName,
                _gameUIView,
                entityData.TsumSprite,
                entityData.TsumColor,
                entityData.HighlightColor,
                _tsumPool
            );

            return tsum;
        }

        private TsumView CreateTsum()
        {
            return Instantiate(_tsumPrefab, this.transform);
        }

        private void OnGetTsum(TsumView tsum)
        {
            tsum.gameObject.SetActive(true);
        }

        private void OnReleaseTsum(TsumView tsum)
        {
            tsum.gameObject.SetActive(false);
        }

        private void OnDestroyTsum(TsumView tsum)
        {
            Destroy(tsum.gameObject);
        }
    }
}