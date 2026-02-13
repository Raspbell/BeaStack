using UnityEngine;
using UniRx;
using UnityEngine.InputSystem;

namespace InGame.GameDebug.View
{
    public class GameDebugView : MonoBehaviour
    {
        public readonly Subject<Unit> OnResetRequest = new Subject<Unit>();
        public readonly Subject<Unit> OnAddScoreRequest = new Subject<Unit>();
        public readonly Subject<Unit> OnTimeExtensionRequest = new Subject<Unit>();
        public readonly Subject<Unit> OnFeverRequest = new Subject<Unit>();
        public readonly Subject<int> OnSpawnTsumRequest = new Subject<int>();

        private bool _isVisible = false;
        private string _spawnTsumIdInput = "0";

        // --- 表示用データ ---
        private float _currentGraceTime;
        private float _maxGraceTime;
        private int _targetTsumCount;
        private bool _isGameOverZone;

        // Presenterから情報を受け取るメソッド
        public void UpdateGameoverInfo(float currentGraceTime, float maxGraceTime, int targetTsumCount)
        {
#if UNITY_EDITOR
            // エディタ拡張ウィンドウへログを送信
            MatrixLogWindow.Log("Game Over Debug/判定対象ツム数", targetTsumCount);
            MatrixLogWindow.Log("Game Over Debug/猶予時間", $"{currentGraceTime:F2} / {maxGraceTime:F2}");
            MatrixLogWindow.Log("Game Over Debug/進行度", maxGraceTime > 0 ? (currentGraceTime / maxGraceTime).ToString("P0") : "0%");

            // 必要ならウィンドウを表示（初回のみなどの制御はお好みで）
            // MatrixLogWindow.ShowWindow(); 
#endif
        }

        private void Update()
        {
            if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                _isVisible = !_isVisible;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible)
            {
                return;
            }

            // --- 左上：デバッグボタン ---
            // サイズをコンパクトにし、左上に配置
            GUILayout.BeginArea(new Rect(10, 10, 160, 260));
            GUILayout.BeginVertical("box");

            if (GUILayout.Button("リセット"))
            {
                OnResetRequest.OnNext(Unit.Default);
            }

            if (GUILayout.Button("スコア +10000"))
            {
                OnAddScoreRequest.OnNext(Unit.Default);
            }

            if (GUILayout.Button("時間 +60秒"))
            {
                OnTimeExtensionRequest.OnNext(Unit.Default);
            }

            if (GUILayout.Button("フィーバー突入"))
            {
                OnFeverRequest.OnNext(Unit.Default);
            }

            GUILayout.Space(10);
            GUILayout.Label("ツム生成 (ID指定)");
            _spawnTsumIdInput = GUILayout.TextField(_spawnTsumIdInput);

            if (GUILayout.Button("指定IDのツムを降らせる"))
            {
                if (int.TryParse(_spawnTsumIdInput, out int id))
                {
                    OnSpawnTsumRequest.OnNext(id);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

            // --- 右上：ゲームオーバー情報 ---
            // 画面右端に寄せるため Screen.width を使用して計算
            // float infoWidth = 220f;
            // float infoHeight = 120f;
            // float margin = 10f;

            // GUILayout.BeginArea(new Rect(Screen.width - infoWidth - margin, margin, infoWidth, infoHeight));
            // GUILayout.BeginVertical("box");

            // GUI.color = Color.white;
            // GUILayout.Label($"<b>[Game Over Logic]</b>");

            // // 判定対象ツム数
            // if (_targetTsumCount > 0)
            // {
            //     GUI.color = Color.red;
            //     GUILayout.Label($"判定対象: {_targetTsumCount}個 (危険!)");
            // }
            // else
            // {
            //     GUI.color = Color.white; // 通常色に戻す
            //     GUILayout.Label($"判定対象: {_targetTsumCount}個");
            // }

            // // カウントダウンタイマー
            // GUILayout.Space(4);
            // GUI.color = Color.white;
            // GUILayout.Label($"猶予: {_currentGraceTime:F2} / {_maxGraceTime:F2} sec");

            // // ゲージバー
            // if (_maxGraceTime > 0)
            // {
            //     float ratio = Mathf.Clamp01(_currentGraceTime / _maxGraceTime);
            //     // 緑(安全) -> 赤(危険) へ色変化
            //     GUI.color = Color.Lerp(Color.green, Color.red, ratio);
            //     GUILayout.HorizontalSlider(ratio, 0f, 1f);
            // }

            // GUILayout.EndVertical();
            // GUILayout.EndArea();
        }
    }
}