using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UniRx;
using DG.Tweening;

namespace InGame.View
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
        [SerializeField] private Image _skillButtonAnimalIcon;
        [SerializeField] private Image[] _skillCurtains;
        [SerializeField] private TextMeshProUGUI _tapText;
        [SerializeField] private Material _skillBarMaterial;

        [SerializeField] private Image _feverPointBar;
        [SerializeField] private Animator _readyAnimator;
        [SerializeField] private TextMeshProUGUI _readyText;

        private ParticleSpawner _particleSpawner;

        [SerializeField] private float _scoreCountUpDuration = 0.25f;
        [SerializeField] private float _skillCurtainMaxAlpha = 0.8f;
        [SerializeField] private float _skillCurtainFadeDuration = 0.3f;
        [SerializeField] private float _skillInvocablePopDuration = 0.5f;
        [SerializeField] private float _deadLineMaxAlpha = 0.5f;

        private Tween _scoreTween;
        private float _tapTextInitialScale;
        private bool _wasSkillInocable = false;

        public IObservable<Unit> OnSpawnButtonClicked => _spawnButton.OnClickAsObservable();
        public IObservable<Unit> OnSkillButtonClicked => _skillButton.OnClickAsObservable();

        private void Awake()
        {
            _tapTextInitialScale = _tapText.transform.localScale.x;
        }

        // インスペクタで設定してもうまくいかない(謎)ので
        public void SetParticleSpawner(ParticleSpawner particleSpawner)
        {
            _particleSpawner = particleSpawner;
        }

        public void SetSkillCurtainActive(bool isActive)
        {
            foreach (var curtain in _skillCurtains)
            {
                curtain.gameObject.SetActive(isActive);
                curtain.DOKill();
                curtain.color = new Color(curtain.color.r, curtain.color.g, curtain.color.b, isActive ? 0f : 1f);
                curtain.DOFade(isActive ? _skillCurtainMaxAlpha : 0f, _skillCurtainFadeDuration);
            }

            if (isActive)
            {
                _tapText.gameObject.SetActive(true);
                _tapText.DOKill();
                _tapText.transform.localScale = new Vector3(_tapTextInitialScale, _tapTextInitialScale, _tapTextInitialScale);
                _tapText.color = new Color(_tapText.color.r, _tapText.color.g, _tapText.color.b, 0f);
                Sequence sequence = DOTween.Sequence();
                sequence.Append(_tapText.DOFade(1f, _skillCurtainFadeDuration))
                    .Join(_tapText.transform.DOScale(1.2f, _skillCurtainFadeDuration).SetEase(Ease.OutBack)).SetLoops(2, LoopType.Yoyo);
                sequence.Play();
            }
            else
            {
                _tapText.DOKill();
                _tapText.DOFade(0f, _skillCurtainFadeDuration)
                    .OnComplete(() =>
                    {
                        _tapText.gameObject.SetActive(false);
                    });
            }
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
                _scoreCountUpDuration
            );
        }

        public void UpdateSkillPoint(float skillPoint, float maxSkillPoint)
        {
            float fillAmount = skillPoint / maxSkillPoint;
            _skillPointBar.fillAmount = fillAmount;

            if (skillPoint >= maxSkillPoint)
            {
                _skillPointBar.material = _skillBarMaterial;
                if (!_wasSkillInocable)
                {
                    PlaySkillInvocablePopEffect();
                    _wasSkillInocable = true;
                }
            }
            else
            {
                _skillPointBar.material = null;
                _wasSkillInocable = false;
            }
        }

        public void UpdateFeverPoint(float feverPoint, float maxFeverPoint)
        {
            float fillAmount = feverPoint / maxFeverPoint;
            _feverPointBar.fillAmount = fillAmount;
        }

        public void UpdateDeadLineAlpha(float deadLineProgress)
        {
            Color color = _deadLineBar.color;
            color.a = deadLineProgress * _deadLineMaxAlpha;
            _deadLineBar.color = color;
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

        public void PlaySkillInvocablePopEffect()
        {
            _skillButtonAnimalIcon.transform.DOKill();
            _skillButtonAnimalIcon.transform.localScale = Vector3.one;
            _skillButtonAnimalIcon.transform.DOScale(1.2f, _skillInvocablePopDuration).SetEase(Ease.OutBack).SetLoops(2, LoopType.Yoyo);
        }

        public void PlayReadyAnimation()
        {
            _readyAnimator.SetTrigger("Play");
        }
    }
}