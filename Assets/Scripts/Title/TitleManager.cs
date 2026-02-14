using UnityEngine;
using UnityEngine.SceneManagement;
using Initial;
using Cysharp.Threading.Tasks;

namespace Title
{
    public class TitleManager : MonoBehaviour
    {
        private void Start()
        {
            FadeMaskManager.FadeOut().Forget();
        }
    }
}