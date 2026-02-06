using UnityEngine;
using UniRx;
using System;

namespace View.Logic
{
    public class GameOverZone : MonoBehaviour
    {
        private float _graceTime;

        private float _timer = 0f;
        private int _triggerCount = 0;

        private readonly Subject<Unit> _onGameOver = new Subject<Unit>();
        public IObservable<Unit> OnGameOver => _onGameOver;

        public void Initialize(float graceTime)
        {
            _graceTime = graceTime;
            _triggerCount = 0;
            _timer = 0f;
        }

        private void Update()
        {
            if (_triggerCount > 0)
            {
                _timer += Time.deltaTime;

                // デバッグ用ログ（必要に応じて警告UIを表示する処理などをここに追加できます）
                // Debug.Log($"Danger... {_timer:F1}/{_graceTime}");

                if (_timer >= _graceTime)
                {
                    _onGameOver.OnNext(Unit.Default);
                    _timer = 0f; // 通知後はリセット（連続通知防止）
                }
            }
            else
            {
                _timer = 0f;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Tsum>() != null)
            {
                _triggerCount++;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<Tsum>() != null)
            {
                _triggerCount--;
                if (_triggerCount < 0) _triggerCount = 0;
            }
        }
    }
}