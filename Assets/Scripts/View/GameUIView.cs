using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace View.Logic
{
    public class GameUIView : MonoBehaviour
    {
        [SerializeField] private Image _timerBar;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Image _skillPointBar;
        [SerializeField] private Image _feverPointBar;
        [SerializeField] private Animator _readyAnimator;
        [SerializeField] private TextMeshProUGUI _readyText;

        [SerializeField] private ParticleSystem _deletedTsumEffect;

        [SerializeField] private float scoreCountUpDuration = 0.25f;

        private Tween _scoreTween;


        public void UpdateTimer(float timeRemaining, float maxTime)
        {
            float fillAmount = timeRemaining / maxTime;
            _timerBar.fillAmount = fillAmount;

            int displayTime = Mathf.CeilToInt(timeRemaining);
            _timerText.text = displayTime.ToString();
        }

        public void UpdateScore(int score)
        {
            _scoreTween?.Kill();
            int currentScore = int.Parse(_scoreText.text);
            _scoreTween = DOTween.To(() => currentScore, x =>
            {
                currentScore = x;
                _scoreText.text = currentScore.ToString();
            }, score, scoreCountUpDuration);
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
            if (_deletedTsumEffect != null)
            {
                ParticleSystem effect = Instantiate(_deletedTsumEffect, position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }
        }

        public void PlayReadyAnimation()
        {
            _readyAnimator.SetTrigger("Play");
        }
    }
}