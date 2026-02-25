using UnityEngine;
using UnityEngine.SceneManagement;
using Global;
using Cysharp.Threading.Tasks;

// タイトル画面はMVPで実装するほどの規模はないため、特に層を分けずに管理
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