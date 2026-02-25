using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

using Global;
using TMPro;

namespace Title
{
    public class TitleUIManager : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _tutorialButton;
        [SerializeField] private Button _changePageRightButton;
        [SerializeField] private Button _changePageLeftButton;
        [SerializeField] private Button _tutorialExitButton;

        [SerializeField] private GameObject _tutorialBasePanel;
        [SerializeField] private GameObject[] _tutorialContentPanels;
        [SerializeField] private TextMeshProUGUI _pageText;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clickSound;

        private int _currentTutorialIndex = 0;
        private bool _isTutorialActive = false;

        private void Start()
        {
            _startButton.onClick.AddListener(() => OnStartButtonClicked().Forget());
            _tutorialButton.onClick.AddListener(() => OnTutorialButtonClicked());
            _tutorialExitButton.onClick.AddListener(() => OnTutorialExitButtonClicked());
            _changePageRightButton.onClick.AddListener(() => OnChangePageButtonClicked(true));
            _changePageLeftButton.onClick.AddListener(() => OnChangePageButtonClicked(false));
        }

        public async UniTaskVoid OnStartButtonClicked()
        {
            PlayClickSound();
            CrossfadeAudioController.ChangeClip(1);
            await FadeMaskManager.FadeIn();
            SceneManager.LoadScene("InGame");
        }

        public void OnTutorialButtonClicked()
        {
            if (_isTutorialActive)
            {
                return;
            }
            _startButton.interactable = false;
            _tutorialButton.interactable = false;

            _isTutorialActive = true;
            _currentTutorialIndex = 0;
            _tutorialBasePanel.SetActive(true);
            _tutorialExitButton.gameObject.SetActive(true);
            foreach (var panel in _tutorialContentPanels)
            {
                panel.SetActive(false);
            }
            _tutorialContentPanels[0].SetActive(true);
            _changePageRightButton.gameObject.SetActive(true);
            _changePageLeftButton.gameObject.SetActive(false);
            _pageText.gameObject.SetActive(true);
            UpdatePageText();
            PlayClickSound();
        }

        public void OnChangePageButtonClicked(bool isRight)
        {
            Debug.Log($"OnChangePageButtonClicked called: {isRight}");
            if (isRight)
            {
                if (_currentTutorialIndex >= _tutorialContentPanels.Length - 1)
                {
                    return;
                }

                _tutorialContentPanels[_currentTutorialIndex].SetActive(false);
                _currentTutorialIndex++;
                _tutorialContentPanels[_currentTutorialIndex].SetActive(true);
                _changePageLeftButton.gameObject.SetActive(true);
                if (_currentTutorialIndex == _tutorialContentPanels.Length - 1)
                {
                    _changePageRightButton.gameObject.SetActive(false);
                }
            }
            else
            {
                if (_currentTutorialIndex <= 0)
                {
                    return;
                }

                _tutorialContentPanels[_currentTutorialIndex].SetActive(false);
                _currentTutorialIndex--;
                _tutorialContentPanels[_currentTutorialIndex].SetActive(true);
                _changePageRightButton.gameObject.SetActive(true);
                if (_currentTutorialIndex == 0)
                {
                    _changePageLeftButton.gameObject.SetActive(false);
                }
            }

            UpdatePageText();
            PlayClickSound();
        }

        public void OnTutorialExitButtonClicked()
        {
            _isTutorialActive = false;
            _tutorialBasePanel.SetActive(false);
            _tutorialExitButton.gameObject.SetActive(false);
            _changePageRightButton.gameObject.SetActive(false);
            _changePageLeftButton.gameObject.SetActive(false);
            _tutorialContentPanels[_currentTutorialIndex].SetActive(false);
            _pageText.gameObject.SetActive(false);

            _startButton.interactable = true;
            _tutorialButton.interactable = true;
            PlayClickSound();
        }

        private void UpdatePageText()
        {
            _pageText.text = $"{_currentTutorialIndex + 1} / {_tutorialContentPanels.Length}";
        }

        private void PlayClickSound()
        {
            _audioSource.PlayOneShot(_clickSound);
        }
    }
}