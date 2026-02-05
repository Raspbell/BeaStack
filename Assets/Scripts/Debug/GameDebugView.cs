using UnityEngine;
using UniRx;
using System;

public class GameDebugView : MonoBehaviour
{
    public readonly Subject<Unit> OnResetRequest = new Subject<Unit>();
    public readonly Subject<Unit> OnAddScoreRequest = new Subject<Unit>();
    public readonly Subject<Unit> OnTimeExtensionRequest = new Subject<Unit>();
    public readonly Subject<Unit> OnFeverRequest = new Subject<Unit>();

    private bool _isVisible = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
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

        GUI.matrix = Matrix4x4.Scale(Vector3.one * 2);

        GUILayout.BeginArea(new Rect(10, 10, 150, 400));
        GUILayout.BeginVertical("box");

        if (GUILayout.Button("リセット (Restart)"))
        {
            OnResetRequest.OnNext(Unit.Default);
        }

        if (GUILayout.Button("スコア +10000"))
        {
            OnAddScoreRequest.OnNext(Unit.Default);
        }

        if (GUILayout.Button("残り時間 +60秒"))
        {
            OnTimeExtensionRequest.OnNext(Unit.Default);
        }

        if (GUILayout.Button("フィーバー突入"))
        {
            OnFeverRequest.OnNext(Unit.Default);
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}