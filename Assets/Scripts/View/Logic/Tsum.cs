using UnityEngine;
using UniRx;
using UnityEngine.Pool;
using DG.Tweening;
using Model.Interface;

namespace View.Logic
{
    // ITsum インターフェースを実装
    public class Tsum : MonoBehaviour, ITsum
    {
        [SerializeField] private GameObject _highlightEffect;
        [SerializeField] private GameObject _tsumSpriteObject;

        private bool _isConnected;
        [HideInInspector] public bool IsConnected => _isConnected;

        private int _tsumID;
        public int TsumID => _tsumID;

        private bool _isDeleting;
        public bool IsDeleting => _isDeleting;

        // ITsumの実装
        public Vector3 Position => transform.position;
        public GameObject GameObject => this.gameObject;

        private string _tsumName;
        private Vector3 _initialScale;
        private GameUIView _gameUIView;
        private Color _tsumColor;
        private Color _highlightColor;
        private Sprite _tsumSprite;
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _highlightSpriteRenderer;
        private IObjectPool<Tsum> _pool;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        public void Initialize(
            int tsumID,
            string tsumName,
            GameUIView gameUIView,
            Sprite tsumSprite,
            Color tsumColor,
            Color highlightColor,
            IObjectPool<Tsum> pool
        )
        {
            _tsumID = tsumID;
            _tsumName = tsumName;
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
            OnDeleted();

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
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Static;
            }
            _isDeleting = true;
        }

        public void HighlightTsum(bool highlight)
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

        public void OnSelected()
        {
            _isConnected = true;
            OutlineTsum(true);
            HighlightTsum(false);
            PlaySelectedAnimation();
        }

        public void OnUnselected()
        {
            _isConnected = false;
            OutlineTsum(false);
        }

        public void OnDeleted()
        {

        }

        private void ResetState()
        {
            _isConnected = false;
            _isDeleting = false;

            var rigidBody = GetComponent<Rigidbody2D>();
            if (rigidBody != null)
            {
                rigidBody.bodyType = RigidbodyType2D.Dynamic;
                rigidBody.linearVelocity = Vector2.zero;
                rigidBody.angularVelocity = 0f;
            }

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

            HighlightTsum(false);
            OutlineTsum(false);
        }
    }
}