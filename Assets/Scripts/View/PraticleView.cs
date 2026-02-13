using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System;

namespace View
{
    public class ParticleView : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private IObjectPool<ParticleView> _pool;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        // Spawnerからプールを受け取る
        public void Initialize(IObjectPool<ParticleView> pool)
        {
            _pool = pool;
        }

        // 再生処理
        public void Play(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);

            _particleSystem.Clear(true);
            _particleSystem.Play(true);

            // 再生時間分待機してからプールに返却
            ReleaseAfterDelay().Forget();
        }

        public void StopAndClear()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            gameObject.SetActive(false);
        }

        private async UniTaskVoid ReleaseAfterDelay()
        {
            float duration = _particleSystem.main.duration;

            // シーン破棄時などのエラーを防ぐため CancellationToken を渡す
            bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();

            if (!isCanceled && gameObject.activeSelf && _pool != null)
            {
                _pool.Release(this);
            }
        }
    }
}