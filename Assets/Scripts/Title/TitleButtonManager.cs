using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

using Initial;
using System;
using Unity.VisualScripting;

namespace Title
{
    public class TitleButtonManager : MonoBehaviour
    {
        [SerializeField] private Button _startButton;

        private void Start()
        {
            _startButton.onClick.AddListener(() => OnStartButtonClicked().Forget());
        }

        public async UniTaskVoid OnStartButtonClicked()
        {
            CrossfadeAudioController.ChangeClip(1);
            await FadeMaskManager.FadeIn();
            SceneManager.LoadScene("InGame");
        }
    }
}
