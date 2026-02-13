using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UniRx;
using DG.Tweening;

namespace View
{
    public class GameUIView : MonoBehaviour
    {
        [SerializeField] private Image _timerBar;
        [SerializeField] private Image _deadLineBar;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Button _spawnButton;
        [SerializeField] private Button _skillButton;
        [SerializeField] private Image _skillPointBar;
        [SerializeField] private Image _feverPointBar;
        [SerializeField] private Animator _readyAnimator;
        [SerializeField] private TextMeshProUGUI _readyText;

        private ParticleSpawner _particleSpawner;

        [SerializeField] private float scoreCountUpDuration = 0.25f;
        [SerializeField] private float deadLineMaxAlpha = 0.5f;

        private Tween _scoreTween;

        public IObservable<Unit> OnSpawnButtonClicked
        {
            get
            {
                return _spawnButton.OnClickAsObservable();
            }
        }

        public IObservable<Unit> OnSkillButtonClicked
        {
            get
            {
                return _skillButton.OnClickAsObservable();
            }
        }

        public void SetParticleSpawner(ParticleSpawner particleSpawner)
        {
            _particleSpawner = particleSpawner;
        }

        public void UpdateTimer(float timeRemaining, float maxTime)
        {
            float fillAmount = timeRemaining / maxTime;
            _timerBar.fillAmount = fillAmount;

            int displayTime = Mathf.CeilToInt(timeRemaining);
            _timerText.text = displayTime.ToString();
        }

        public void UpdateScore(int score)
        {
            if (_scoreTween != null)
            {
                _scoreTween.Kill();
            }
            int currentScore = int.Parse(_scoreText.text);
            _scoreTween = DOTween.To(
                () =>
                {
                    return currentScore;
                },
                x =>
                {
                    currentScore = x;
                    _scoreText.text = currentScore.ToString();
                },
                score,
                scoreCountUpDuration
            );
        }

        public void UpdateSkillPoint(float skillPoint, float maxSkillPoint)
        {
            float fillAmount = skillPoint / maxSkillPoint;
            _skillPointBar.fillAmount = fillAmount;
        }

        public void UpdateFeverPoint(float feverPoint, float maxFeverPoint)
        {
            float fillAmount = feverPoint / maxFeverPoint;
            _feverPointBar.fillAmount = fillAmount;
        }

        public void PlayDeletedTsumEffect(Vector3 position)
        {
            if (_particleSpawner != null)
            {
                _particleSpawner.SpawnParticle(position);
            }
            else
            {
                Debug.LogWarning("GameUIViewのParticleSpawnerが設定されていません。");
            }
        }

        public void UpdateDeadLineAlpha(float deadLineProgress)
        {
            Color color = _deadLineBar.color;
            color.a = deadLineProgress * deadLineMaxAlpha;
            _deadLineBar.color = color;
        }

        public void PlayReadyAnimation()
        {
            _readyAnimator.SetTrigger("Play");
        }
    }
}