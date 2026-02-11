using UnityEngine;
using UniRx;
using UnityEngine.Pool;
using DG.Tweening;
using Model.Interface;

namespace View
{
    public class TsumView : MonoBehaviour, ITsumView
    {
        // 線を引くために使う位置情報 (物理計算には使わない)
        public Vector3 Position => transform.position;

        [SerializeField] private GameObject _highlightEffect;
        [SerializeField] private GameObject _tsumSpriteObject;

        private Vector3 _initialScale;
        private GameUIView _gameUIView;
        private Color _tsumColor;
        private Color _highlightColor;
        private Sprite _tsumSprite;
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _highlightSpriteRenderer;
        private IObjectPool<TsumView> _pool;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        public void Initialize(
            GameUIView gameUIView,
            Sprite tsumSprite,
            Color tsumColor,
            Color highlightColor,
            IObjectPool<TsumView> pool
        )
        {
            _gameUIView = gameUIView;
            _tsumSprite = tsumSprite;
            _tsumColor = tsumColor;
            _highlightColor = highlightColor;
            _pool = pool;

            _spriteRenderer = _tsumSpriteObject.GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null && _tsumSprite != null)
            {
                _spriteRenderer.sprite = _tsumSprite;
                _spriteRenderer.color = _tsumColor;
            }

            _highlightSpriteRenderer = _highlightEffect.GetComponent<SpriteRenderer>();
            if (_highlightSpriteRenderer != null)
            {
                _highlightSpriteRenderer.color = _highlightColor;
            }

            ResetState();
        }

        public void DeleteTsum()
        {
            // OnDeleted();

            if (_pool != null)
            {
                _pool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetDeleting()
        {

        }

        public void SetHighlight(bool highlight)
        {
            if (_highlightEffect != null && _highlightSpriteRenderer != null)
            {
                _highlightSpriteRenderer.enabled = highlight;
            }
        }

        public void OutlineTsum(bool outline)
        {

        }

        public void PlaySelectedAnimation()
        {
            if (_tsumSpriteObject != null)
            {
                _tsumSpriteObject.transform.DOKill();
                _tsumSpriteObject.transform.localScale = Vector3.one;
                _tsumSpriteObject.transform.DOScale(1.2f, 0.1f).SetLoops(2, LoopType.Yoyo);
            }
        }

        public void PlayDeletedAnimation()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0.5f);
            }
            if (_gameUIView != null)
            {
                _gameUIView.PlayDeletedTsumEffect(transform.position);
            }
        }

        public void UpdatePosition(Vector2 newPosition)
        {
            transform.position = new Vector3(newPosition.x, newPosition.y, -1f);
        }

        public void OnSelected()
        {
            OutlineTsum(true);
            SetHighlight(false);
            PlaySelectedAnimation();
        }

        public void OnUnselected()
        {
            OutlineTsum(false);
        }

        public void OnDeleted()
        {

        }

        private void ResetState()
        {
            transform.localScale = _initialScale;
            transform.rotation = Quaternion.identity;

            if (_tsumSpriteObject != null)
            {
                _tsumSpriteObject.transform.localScale = Vector3.one;
            }

            if (_spriteRenderer != null && _tsumColor != null)
            {
                _spriteRenderer.color = _tsumColor;
            }

            SetHighlight(false);
            OutlineTsum(false);
        }
    }
}