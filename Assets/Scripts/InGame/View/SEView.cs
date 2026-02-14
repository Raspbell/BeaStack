using UnityEngine;
using InGame.Model.Interface;

namespace InGame.View
{
    public class SEView : MonoBehaviour, ISEView
    {
        [SerializeField] private AudioClip _dropSound;
        [SerializeField] private AudioClip _deleteSound;
        [SerializeField] private AudioClip _selectedSound;
        [SerializeField] private AudioClip _explosionSound;
        [SerializeField] private AudioClip _skillChargedSound;
        [SerializeField] private AudioClip _skillActivatedSound;
        [SerializeField] private AudioClip _skillUsedSound;
        [SerializeField] private AudioClip _gameOverSound;

        [SerializeField] private float _volume = 1f;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.volume = _volume;
        }

        public void SetVolume(float volume)
        {
            _volume = Mathf.Max(volume, 0f);
            _audioSource.volume = _volume;
        }

        public void PlayDropSound()
        {
            _audioSource.PlayOneShot(_dropSound, _volume);
        }

        public void PlayDeletedSound()
        {
            _audioSource.PlayOneShot(_deleteSound, _volume);
        }

        public void PlaySelectedSound()
        {
            _audioSource.PlayOneShot(_selectedSound, _volume);
        }

        public void PlayExplosionSound()
        {
            _audioSource.PlayOneShot(_explosionSound, _volume);
        }

        public void PlaySkillChargedSound()
        {
            _audioSource.PlayOneShot(_skillChargedSound, _volume);
        }

        public void PlaySkillActivatedSound()
        {
            _audioSource.PlayOneShot(_skillActivatedSound, _volume);
        }

        public void PlaySkillUsedSound()
        {
            _audioSource.PlayOneShot(_skillUsedSound, _volume);
        }

        public void PlayGameOverSound()
        {
            _audioSource.PlayOneShot(_gameOverSound, _volume);
        }
    }
}