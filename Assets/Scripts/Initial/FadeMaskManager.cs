using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Initial
{
    public class FadeMaskManager : MonoBehaviour
    {
        [SerializeField] private Image _fadeMask;
        [SerializeField] private Color _fadeColor = Color.black;
        [SerializeField] private float _fadeDuration = 1f;

        private static Image _fadeMaskStatic;
        private static Color _fadeColorStatic;
        private static float _fadeDurationStatic;

        private void Awake()
        {
            if (_fadeMaskStatic == null)
            {
                _fadeMaskStatic = _fadeMask;
                _fadeDurationStatic = _fadeDuration;
                _fadeColorStatic = _fadeColor;
                DontDestroyOnLoad(gameObject);
            }
        }

        public static async UniTask FadeIn()
        {
            if (_fadeMaskStatic != null)
            {
                _fadeMaskStatic.gameObject.SetActive(true);
                _fadeMaskStatic.color = new Color(_fadeColorStatic.r, _fadeColorStatic.g, _fadeColorStatic.b, 0f);
                await _fadeMaskStatic.DOFade(1f, _fadeDurationStatic).AsyncWaitForCompletion();
            }
        }

        public static async UniTask FadeOut()
        {
            if (_fadeMaskStatic != null)
            {
                _fadeMaskStatic.gameObject.SetActive(true);
                _fadeMaskStatic.color = new Color(_fadeColorStatic.r, _fadeColorStatic.g, _fadeColorStatic.b, 1f);
                await _fadeMaskStatic.DOFade(0f, _fadeDurationStatic).AsyncWaitForCompletion();
                _fadeMaskStatic.gameObject.SetActive(false);
            }
        }
    }
}