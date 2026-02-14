using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace InGame.View
{
    public class ParticleSpawner : MonoBehaviour
    {
        [SerializeField] private ParticleView _particlePrefab;
        private ObjectPool<ParticleView> _pool;

        public async UniTask Initialize(int maxCount)
        {
            _pool = new ObjectPool<ParticleView>(
                createFunc: CreateParticle,
                actionOnGet: OnGetParticle,
                actionOnRelease: OnReleaseParticle,
                actionOnDestroy: OnDestroyParticle,
                collectionCheck: true,
                defaultCapacity: maxCount,
                maxSize: maxCount
            );

            await PrewarmPool(maxCount);
        }

        public void SpawnParticle(Vector3 position)
        {
            if (_pool == null)
            {
                return;
            }

            ParticleView particle = _pool.Get();
            particle.Play(position);
        }

        private ParticleView CreateParticle()
        {
            ParticleView particle = Instantiate(_particlePrefab, transform);
            particle.Initialize(_pool);
            particle.gameObject.SetActive(false);
            return particle;
        }

        private void OnGetParticle(ParticleView particle)
        {

        }

        private void OnReleaseParticle(ParticleView particle)
        {
            particle.StopAndClear();
        }

        private void OnDestroyParticle(ParticleView particle)
        {
            Destroy(particle.gameObject);
        }

        private async UniTask PrewarmPool(int count)
        {
            List<ParticleView> prewarmed = new List<ParticleView>(count);
            for (int i = 0; i < count; i++)
            {
                prewarmed.Add(_pool.Get());
            }
            foreach (var p in prewarmed)
            {
                _pool.Release(p);
            }
        }
    }
}