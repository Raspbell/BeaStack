#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MatrixLogWindow : EditorWindow
{
    // ログを保持する辞書（Key: 項目名, Value: 表示内容）
    private static Dictionary<string, string> _logs = new Dictionary<string, string>();

    // スクロール位置
    private Vector2 _scrollPosition;

    // ウィンドウのインスタンス（Repaint用）
    private static MatrixLogWindow _instance;

    [MenuItem("Tools/Matrix Logger")]
    public static void ShowWindow()
    {
        var window = GetWindow<MatrixLogWindow>("Matrix Logger");
        window.Show();
    }

    private void OnEnable()
    {
        _instance = this;
    }

    private void OnDisable()
    {
        _instance = null;
    }

    /// <summary>
    /// ログを登録・更新するメソッド
    /// </summary>
    /// <param name="key">項目の識別子（行のタイトル）</param>
    /// <param name="value">表示する値</param>
    public static void Log(string key, object value)
    {
        // 値がnullの場合は文字列の"null"とする
        string valueStr = "null";
        if (value != null)
        {
            valueStr = value.ToString();
        }

        // 辞書にキーが存在すれば更新、なければ追加
        if (_logs.ContainsKey(key))
        {
            _logs[key] = valueStr;
        }
        else
        {
            _logs.Add(key, valueStr);
        }

        // ウィンドウが開いている場合のみ再描画を要求してリアルタイム更新する
        if (_instance != null)
        {
            _instance.Repaint();
        }
    }

    /// <summary>
    /// ログを全て消去する
    /// </summary>
    public static void Clear()
    {
        _logs.Clear();
        if (_instance != null)
        {
            _instance.Repaint();
        }
    }

    private void OnGUI()
    {
        DrawToolbar();

        // スクロールビューの開始
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        {
            // 辞書の内容をマトリクス（表）形式で表示
            foreach (var log in _logs)
            {
                DrawLogEntry(log.Key, log.Value);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
            {
                Clear();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLogEntry(string key, string value)
    {
        EditorGUILayout.BeginHorizontal("box");
        {
            // キー（項目名）の表示設定
            // 幅を固定または最小値を設定して整列させる
            float keyWidth = 150f;

            GUIStyle keyStyle = new GUIStyle(EditorStyles.boldLabel);
            keyStyle.alignment = TextAnchor.MiddleLeft;

            EditorGUILayout.LabelField(key, keyStyle, GUILayout.Width(keyWidth));

            // 区切り線（視覚的な装飾）
            GUILayout.Box("", GUILayout.Width(1), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            // 値の表示設定
            GUIStyle valueStyle = new GUIStyle(EditorStyles.label);
            valueStyle.alignment = TextAnchor.MiddleLeft;
            valueStyle.wordWrap = true; // 長いテキストは折り返す

            EditorGUILayout.LabelField(value, valueStyle);
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif