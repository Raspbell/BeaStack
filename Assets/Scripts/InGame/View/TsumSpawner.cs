using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using InGame.Model.Interface;

namespace InGame.View
{
    public class TsumSpawner : MonoBehaviour, ITsumSpawner
    {
        [SerializeField] private BoxCollider2D _spawnArea;
        [SerializeField] private TsumView _tsumPrefab;
        [SerializeField] private float _spawnZPosition = -1f;

        // [SerializeField] private int _defaultCapacity = 100;
        // [SerializeField] private int _maxSize = 100;

        private GameUIView _gameUIView;
        private IObjectPool<TsumView> _tsumPool;

        public void Initialize(GameUIView gameUIView, int maxTsumCount)
        {
            _gameUIView = gameUIView;

            _tsumPool = new ObjectPool<TsumView>(
                createFunc: CreateTsum,
                actionOnGet: OnGetTsum,
                actionOnRelease: OnReleaseTsum,
                actionOnDestroy: OnDestroyTsum,
                collectionCheck: true,
                defaultCapacity: maxTsumCount,
                maxSize: maxTsumCount
            );

            PrewarmPool(maxTsumCount);
        }

        public Vector2 GetRandomSpawnPosition()
        {
            Vector2 spawnPosition = new Vector2(
                 Random.Range(_spawnArea.bounds.min.x, _spawnArea.bounds.max.x),
                 Random.Range(_spawnArea.bounds.min.y, _spawnArea.bounds.max.y)
             );
            return spawnPosition;
        }

        public ITsumView SpawnTsum(int tsumId, Vector2 position, float radius, Sprite sprite, Color color, Color highlightColor)
        {
            if (tsumId < 0)
            {
                return null;
            }
            ITsumView newTsumView = GetTsumFromPool(tsumId, position, radius, sprite, color, highlightColor);
            return newTsumView;
        }

        private void PrewarmPool(int count)
        {
            List<TsumView> prewarmedTsums = new List<TsumView>(count);
            for (int i = 0; i < count; i++)
            {
                prewarmedTsums.Add(_tsumPool.Get());
            }

            foreach (var tsum in prewarmedTsums)
            {
                _tsumPool.Release(tsum);
            }
        }

        private ITsumView GetTsumFromPool(int tsumId, Vector2 position, float radius, Sprite sprite, Color color, Color highlightColor)
        {
            TsumView tsumView = _tsumPool.Get();
            tsumView.transform.position = new Vector3(position.x, position.y, _spawnZPosition);
            tsumView.transform.rotation = Quaternion.identity;

            tsumView.Initialize(
                _gameUIView,
                radius,
                sprite,
                color,
                highlightColor,
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