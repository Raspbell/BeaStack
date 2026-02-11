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
        [SerializeField] private float _spawnZPosition = -1f;

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

        public Vector2 GetRandomSpawnPosition()
        {
            Vector2 spawnPosition = new Vector2(
                 Random.Range(_spawnArea.bounds.min.x, _spawnArea.bounds.max.x),
                 Random.Range(_spawnArea.bounds.min.y, _spawnArea.bounds.max.y)
             );
            return spawnPosition;
        }

        public ITsumView SpawnTsum(int tsumId, Vector2 position)
        {
            if (tsumId < 0)
            {
                return null;
            }
            ITsumView newTsumView = GetTsumFromPool(tsumId, position);
            return newTsumView;
        }

        private ITsumView GetTsumFromPool(int tsumId, Vector2 position)
        {
            TsumView tsumView = _tsumPool.Get();
            tsumView.transform.position = new Vector3(position.x, position.y, _spawnZPosition);
            tsumView.transform.rotation = Quaternion.identity;

            var tsumEntityData = _tsumData.GetTsumComponentById(tsumId);

            tsumView.Initialize(
                _gameUIView,
                tsumEntityData.Radius,
                tsumEntityData.Sprite,
                tsumEntityData.Color,
                tsumEntityData.HighlightColor,
                _tsumPool
            );

            return tsumView;
        }

        private TsumView CreateTsum()
        {
            return Instantiate(_tsumPrefab, transform);
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