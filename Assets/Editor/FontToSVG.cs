#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// OTF / TTF フォントのテキストを SVG / PNG として書き出すエディタ拡張。
/// 太さ調整・アウトライン・パラメータ永続化に対応。
/// </summary>
public class FontToSVGExporter : EditorWindow
{
    // ═══════════════════════════════════════════════
    // 定数
    // ═══════════════════════════════════════════════
    private const string PK = "FontToSVG_"; // EditorPrefs キー接頭辞

    // ═══════════════════════════════════════════════
    // 設定フィールド（すべて永続化対象）
    // ═══════════════════════════════════════════════
    private Font _font;
    private string _text = "Hello World\nこんにちは世界";
    private int _fontSize = 48;
    private float _lineSpacing = 1.4f;
    private float _letterSpacing = 0f;
    private float _boldness = 0f;
    private Color _textColor = Color.black;

    // アウトライン
    private bool _enableOutline = false;
    private float _outlineWidth = 3f;
    private Color _outlineColor = Color.white;

    // キャンバス
    private Color _bgColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    private bool _includeBg = false;
    private int _canvasWidth = 800;
    private int _canvasHeight = 400;
    private float _paddingX = 24f;
    private float _paddingY = 24f;
    private bool _autoSize = true;
    private FontStyle _fontStyle = FontStyle.Normal;

    private enum Align { Left, Center, Right }
    private Align _align = Align.Left;

    private enum ExportMode { VectorSVG, RasterizedSVG, PNG_Sprite }
    private ExportMode _exportMode = ExportMode.VectorSVG;

    // ═══════════════════════════════════════════════
    // 内部
    // ═══════════════════════════════════════════════
    private Vector2 _scroll;

    private struct LineInfo
    {
        public string text;
        public float width;
        public List<CharMetric> chars;
    }

    private struct CharMetric
    {
        public char ch;
        public float x;
        public float advance;
        public CharacterInfo info;
        public bool hasInfo;
    }

    // ═══════════════════════════════════════════════
    // ウィンドウ
    // ═══════════════════════════════════════════════

    [MenuItem("Tools/Font to SVG Exporter")]
    static void Open()
    {
        var w = GetWindow<FontToSVGExporter>("Font → SVG");
        w.minSize = new Vector2(440, 780);
    }

    // ── 永続化: 読み込み / 書き込み ──────────────

    private void OnEnable() => LoadPrefs();
    private void OnDisable() => SavePrefs();

    private void SavePrefs()
    {
        // フォントは GUID で保存
        if (_font != null)
        {
            string path = AssetDatabase.GetAssetPath(_font);
            string guid = AssetDatabase.AssetPathToGUID(path);
            EditorPrefs.SetString(PK + "fontGUID", guid);
        }
        else
        {
            EditorPrefs.SetString(PK + "fontGUID", "");
        }

        EditorPrefs.SetString(PK + "text", _text);
        EditorPrefs.SetInt(PK + "fontSize", _fontSize);
        EditorPrefs.SetFloat(PK + "lineSpacing", _lineSpacing);
        EditorPrefs.SetFloat(PK + "letterSpacing", _letterSpacing);
        EditorPrefs.SetFloat(PK + "boldness", _boldness);
        EditorPrefs.SetInt(PK + "fontStyle", (int)_fontStyle);
        EditorPrefs.SetInt(PK + "align", (int)_align);
        EditorPrefs.SetInt(PK + "exportMode", (int)_exportMode);

        SaveColor(PK + "textColor", _textColor);

        EditorPrefs.SetBool(PK + "enableOutline", _enableOutline);
        EditorPrefs.SetFloat(PK + "outlineWidth", _outlineWidth);
        SaveColor(PK + "outlineColor", _outlineColor);

        EditorPrefs.SetBool(PK + "includeBg", _includeBg);
        SaveColor(PK + "bgColor", _bgColor);

        EditorPrefs.SetInt(PK + "canvasWidth", _canvasWidth);
        EditorPrefs.SetInt(PK + "canvasHeight", _canvasHeight);
        EditorPrefs.SetFloat(PK + "paddingX", _paddingX);
        EditorPrefs.SetFloat(PK + "paddingY", _paddingY);
        EditorPrefs.SetBool(PK + "autoSize", _autoSize);
    }

    private void LoadPrefs()
    {
        // フォント
        string guid = EditorPrefs.GetString(PK + "fontGUID", "");
        if (!string.IsNullOrEmpty(guid))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
                _font = AssetDatabase.LoadAssetAtPath<Font>(path);
        }

        _text = EditorPrefs.GetString(PK + "text", _text);
        _fontSize = EditorPrefs.GetInt(PK + "fontSize", _fontSize);
        _lineSpacing = EditorPrefs.GetFloat(PK + "lineSpacing", _lineSpacing);
        _letterSpacing = EditorPrefs.GetFloat(PK + "letterSpacing", _letterSpacing);
        _boldness = EditorPrefs.GetFloat(PK + "boldness", _boldness);
        _fontStyle = (FontStyle)EditorPrefs.GetInt(PK + "fontStyle", (int)_fontStyle);
        _align = (Align)EditorPrefs.GetInt(PK + "align", (int)_align);
        _exportMode = (ExportMode)EditorPrefs.GetInt(PK + "exportMode", (int)_exportMode);

        _textColor = LoadColor(PK + "textColor", _textColor);

        _enableOutline = EditorPrefs.GetBool(PK + "enableOutline", _enableOutline);
        _outlineWidth = EditorPrefs.GetFloat(PK + "outlineWidth", _outlineWidth);
        _outlineColor = LoadColor(PK + "outlineColor", _outlineColor);

        _includeBg = EditorPrefs.GetBool(PK + "includeBg", _includeBg);
        _bgColor = LoadColor(PK + "bgColor", _bgColor);

        _canvasWidth = EditorPrefs.GetInt(PK + "canvasWidth", _canvasWidth);
        _canvasHeight = EditorPrefs.GetInt(PK + "canvasHeight", _canvasHeight);
        _paddingX = EditorPrefs.GetFloat(PK + "paddingX", _paddingX);
        _paddingY = EditorPrefs.GetFloat(PK + "paddingY", _paddingY);
        _autoSize = EditorPrefs.GetBool(PK + "autoSize", _autoSize);
    }

    private static void SaveColor(string key, Color c)
    {
        EditorPrefs.SetFloat(key + "_r", c.r);
        EditorPrefs.SetFloat(key + "_g", c.g);
        EditorPrefs.SetFloat(key + "_b", c.b);
        EditorPrefs.SetFloat(key + "_a", c.a);
    }

    private static Color LoadColor(string key, Color def)
    {
        if (!EditorPrefs.HasKey(key + "_r")) return def;
        return new Color(
            EditorPrefs.GetFloat(key + "_r", def.r),
            EditorPrefs.GetFloat(key + "_g", def.g),
            EditorPrefs.GetFloat(key + "_b", def.b),
            EditorPrefs.GetFloat(key + "_a", def.a));
    }

    // ═══════════════════════════════════════════════
    // OnGUI
    // ═══════════════════════════════════════════════

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        DrawHeader();
        DrawFontSection();
        DrawTextSection();
        DrawOutlineSection();
        DrawCanvasSection();
        DrawExportSection();
        DrawExportButton();
        DrawResetButton();
        DrawLivePreview();

        EditorGUILayout.EndScrollView();
    }

    // ────────────────────────────────────────────
    // セクション
    // ────────────────────────────────────────────

    private void DrawHeader()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Font → SVG Exporter", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "OTF / TTF フォントを指定し、テキストを SVG / PNG に書き出します。\n" +
            "設定はエディタ再起動後も自動で復元されます。",
            MessageType.Info);
        EditorGUILayout.Space(4);
    }

    private void DrawFontSection()
    {
        EditorGUILayout.LabelField("フォント", EditorStyles.boldLabel);
        _font = (Font)EditorGUILayout.ObjectField("フォント (TTF/OTF)", _font, typeof(Font), false);
        _fontSize = EditorGUILayout.IntSlider("フォントサイズ", _fontSize, 8, 300);
        _fontStyle = (FontStyle)EditorGUILayout.EnumPopup("スタイル", _fontStyle);
        EditorGUILayout.Space(6);
    }

    private void DrawTextSection()
    {
        EditorGUILayout.LabelField("テキスト", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("テキスト（改行対応）", EditorStyles.miniLabel);
        _text = EditorGUILayout.TextArea(_text, GUILayout.MinHeight(60));
        _lineSpacing = EditorGUILayout.Slider("行間倍率", _lineSpacing, 0.5f, 5f);
        _letterSpacing = EditorGUILayout.Slider("文字間隔 (px)", _letterSpacing, -20f, 80f);
        _boldness = EditorGUILayout.Slider("太さ (px)", _boldness, 0f, 20f);
        _textColor = EditorGUILayout.ColorField("文字色", _textColor);
        _align = (Align)EditorGUILayout.EnumPopup("揃え", _align);
        EditorGUILayout.Space(6);
    }

    private void DrawOutlineSection()
    {
        EditorGUILayout.LabelField("アウトライン", EditorStyles.boldLabel);
        _enableOutline = EditorGUILayout.Toggle("アウトラインを有効化", _enableOutline);

        EditorGUI.BeginDisabledGroup(!_enableOutline);
        _outlineWidth = EditorGUILayout.Slider("アウトライン幅 (px)", _outlineWidth, 0.5f, 30f);
        _outlineColor = EditorGUILayout.ColorField("アウトライン色", _outlineColor);
        EditorGUI.EndDisabledGroup();

        if (_enableOutline && _exportMode == ExportMode.VectorSVG)
        {
            EditorGUILayout.HelpBox(
                "ベクター SVG: stroke でアウトラインを描画します。\n" +
                "太さ（膨張）と併用する場合、太さ分は内側に適用されます。",
                MessageType.None);
        }
        EditorGUILayout.Space(6);
    }

    private void DrawCanvasSection()
    {
        EditorGUILayout.LabelField("キャンバス", EditorStyles.boldLabel);
        _autoSize = EditorGUILayout.Toggle("自動サイズ", _autoSize);
        EditorGUI.BeginDisabledGroup(_autoSize);
        _canvasWidth = Mathf.Max(64, EditorGUILayout.IntField("幅", _canvasWidth));
        _canvasHeight = Mathf.Max(64, EditorGUILayout.IntField("高さ", _canvasHeight));
        EditorGUI.EndDisabledGroup();
        _paddingX = EditorGUILayout.Slider("パディング X", _paddingX, 0, 200);
        _paddingY = EditorGUILayout.Slider("パディング Y", _paddingY, 0, 200);
        _includeBg = EditorGUILayout.Toggle("背景を含める", _includeBg);
        if (_includeBg)
            _bgColor = EditorGUILayout.ColorField("背景色", _bgColor);
        EditorGUILayout.Space(6);
    }

    private void DrawExportSection()
    {
        EditorGUILayout.LabelField("出力モード", EditorStyles.boldLabel);
        _exportMode = (ExportMode)EditorGUILayout.EnumPopup("モード", _exportMode);
        switch (_exportMode)
        {
            case ExportMode.VectorSVG:
                EditorGUILayout.HelpBox("SVG <text> 要素で出力。閲覧側にフォントが必要。", MessageType.None);
                break;
            case ExportMode.RasterizedSVG:
                EditorGUILayout.HelpBox("フォントアトラスからレンダリングし PNG を SVG に Base64 埋め込み。", MessageType.None);
                break;
            case ExportMode.PNG_Sprite:
                EditorGUILayout.HelpBox(
                    "PNG として書き出し Sprite として自動インポート。\n" +
                    "UI Image の Source Image にそのままセットできます。", MessageType.None);
                break;
        }
        EditorGUILayout.Space(6);
    }

    // ────────────────────────────────────────────
    // ボタン
    // ────────────────────────────────────────────

    private void DrawExportButton()
    {
        EditorGUI.BeginDisabledGroup(_font == null || string.IsNullOrEmpty(_text));

        bool isPNG = _exportMode == ExportMode.PNG_Sprite;
        string label = isPNG ? "PNG を書き出す" : "SVG を書き出す";
        string ext = isPNG ? "png" : "svg";

        if (GUILayout.Button(label, GUILayout.Height(36)))
        {
            var path = EditorUtility.SaveFilePanel("保存先を選択", Application.dataPath, "output", ext);
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    Export(path);
                    EditorUtility.DisplayDialog("完了", "保存しました:\n" + path, "OK");

                    if (path.StartsWith(Application.dataPath))
                    {
                        AssetDatabase.Refresh();
                        if (isPNG)
                        {
                            string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
                            ConfigureAsSpriteAsset(assetPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EditorUtility.DisplayDialog("エラー", ex.Message, "OK");
                    Debug.LogException(ex);
                }
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    private void DrawResetButton()
    {
        EditorGUILayout.Space(2);
        if (GUILayout.Button("設定をリセット"))
        {
            if (EditorUtility.DisplayDialog("確認", "すべての設定を初期値に戻しますか？", "リセット", "キャンセル"))
            {
                // EditorPrefs から全消去
                foreach (var k in new[]
                {
                    "fontGUID", "text", "fontSize", "lineSpacing", "letterSpacing",
                    "boldness", "fontStyle", "align", "exportMode",
                    "textColor_r", "textColor_g", "textColor_b", "textColor_a",
                    "enableOutline", "outlineWidth",
                    "outlineColor_r", "outlineColor_g", "outlineColor_b", "outlineColor_a",
                    "includeBg",
                    "bgColor_r", "bgColor_g", "bgColor_b", "bgColor_a",
                    "canvasWidth", "canvasHeight", "paddingX", "paddingY", "autoSize"
                })
                {
                    EditorPrefs.DeleteKey(PK + k);
                }

                // フィールドを初期値に
                _font = null;
                _text = "Hello World\nこんにちは世界";
                _fontSize = 48; _lineSpacing = 1.4f; _letterSpacing = 0f;
                _boldness = 0f; _fontStyle = FontStyle.Normal;
                _align = Align.Left; _exportMode = ExportMode.VectorSVG;
                _textColor = Color.black;
                _enableOutline = false; _outlineWidth = 3f; _outlineColor = Color.white;
                _includeBg = false; _bgColor = new Color(0.95f, 0.95f, 0.95f, 1f);
                _canvasWidth = 800; _canvasHeight = 400;
                _paddingX = 24f; _paddingY = 24f; _autoSize = true;

                Repaint();
            }
        }
        EditorGUILayout.Space(4);
    }

    private void Export(string path)
    {
        switch (_exportMode)
        {
            case ExportMode.VectorSVG:
                File.WriteAllText(path, BuildVectorSVG(), Encoding.UTF8);
                break;
            case ExportMode.RasterizedSVG:
                File.WriteAllText(path, BuildRasterizedSVG(), Encoding.UTF8);
                break;
            case ExportMode.PNG_Sprite:
                ExportPNG(path);
                break;
        }
    }

    // ═══════════════════════════════════════════════
    // ★ ライブプレビュー
    //    太さ: オフセット多重描画
    //    アウトライン: さらに外側をアウトライン色で多重描画
    // ═══════════════════════════════════════════════

    private void DrawLivePreview()
    {
        if (_font == null)
        {
            EditorGUILayout.HelpBox("フォントを設定するとプレビューが表示されます。", MessageType.None);
            return;
        }

        EditorGUILayout.LabelField("プレビュー", EditorStyles.boldLabel);

        float textW, textH;
        var lines = MeasureLines(out textW, out textH);
        int cw, ch;
        ResolveCanvasSize(textW, textH, out cw, out ch);

        float availW = EditorGUIUtility.currentViewWidth - 32;
        float scale = Mathf.Min(1f, availW / Mathf.Max(1, cw));
        float dispW = cw * scale;
        float dispH = Mathf.Min(ch * scale, 500f);

        Rect area = GUILayoutUtility.GetRect(dispW, dispH + 8);
        Rect box = new Rect(area.x, area.y + 4, dispW, dispH);

        // 背景
        if (!_includeBg)
            DrawCheckerboard(box, 8f);
        else
            EditorGUI.DrawRect(box, _bgColor);

        DrawRectOutline(box, new Color(0.45f, 0.45f, 0.45f));

        GUI.BeginClip(box);
        {
            float lineH = _fontSize * _lineSpacing * scale;
            float innerW = (cw - _paddingX * 2) * scale;
            float padX = _paddingX * scale;
            float padY = _paddingY * scale;
            float margin = TotalMargin();
            float mScaled = margin * scale;

            // ── パス 1: アウトラインレイヤー ──
            if (_enableOutline)
            {
                var outStyle = MakeStyle(scale, _outlineColor);
                float outR = (_boldness + _outlineWidth) * scale;
                var outOffsets = BuildDiskOffsets(outR);

                DrawTextLayer(lines, outStyle, outOffsets, lineH, innerW, padX, padY,
                              mScaled, dispW, scale);
            }

            // ── パス 2: 文字色レイヤー（太さ込み） ──
            {
                var style = MakeStyle(scale, _textColor);
                float boldR = _boldness * scale;
                var offsets = BuildDiskOffsets(boldR);

                DrawTextLayer(lines, style, offsets, lineH, innerW, padX, padY,
                              mScaled, dispW, scale);
            }
        }
        GUI.EndClip();

        EditorGUILayout.LabelField($"  キャンバス: {cw} × {ch} px", EditorStyles.miniLabel);
        EditorGUILayout.Space(8);
    }

    private void DrawTextLayer(
        List<LineInfo> lines, GUIStyle style, List<Vector2> offsets,
        float lineH, float innerW, float padX, float padY,
        float marginScaled, float dispW, float scale)
    {
        for (int li = 0; li < lines.Count; li++)
        {
            var line = lines[li];
            float lineWs = line.width * scale;
            float ox = padX + LineOffsetX(lineWs, innerW);
            float oy = padY + li * lineH;

            foreach (var off in offsets)
            {
                if (Mathf.Approximately(_letterSpacing, 0f))
                {
                    Rect r = new Rect(
                        ox + marginScaled + off.x,
                        oy + marginScaled + off.y,
                        dispW, lineH + style.fontSize);
                    GUI.Label(r, line.text, style);
                }
                else
                {
                    foreach (var cm in line.chars)
                    {
                        float cx = ox + cm.x * scale + marginScaled + off.x;
                        Rect r = new Rect(cx, oy + marginScaled + off.y,
                                          cm.advance * scale + 20, lineH + style.fontSize);
                        GUI.Label(r, cm.ch.ToString(), style);
                    }
                }
            }
        }
    }

    private GUIStyle MakeStyle(float scale, Color color)
    {
        return new GUIStyle
        {
            font = _font,
            fontSize = Mathf.Max(4, Mathf.RoundToInt(_fontSize * scale)),
            fontStyle = _fontStyle,
            wordWrap = false,
            clipping = TextClipping.Overflow,
            richText = false,
            normal = { textColor = color }
        };
    }

    /// <summary> 太さ + アウトラインで必要な片側マージン </summary>
    private float TotalMargin()
    {
        float m = _boldness;
        if (_enableOutline) m += _outlineWidth;
        return m;
    }

    // ═══════════════════════════════════════════════
    // テキスト計測
    // ═══════════════════════════════════════════════

    private List<LineInfo> MeasureLines(out float totalWidth, out float totalHeight)
    {
        string clean = _text.Replace("\r\n", "\n").Replace("\r", "\n");
        _font.RequestCharactersInTexture(clean, _fontSize, _fontStyle);

        string[] rawLines = clean.Split('\n');
        float lineH = _fontSize * _lineSpacing;
        var lines = new List<LineInfo>();
        float maxW = 0;
        float extra = TotalMargin() * 2f;

        foreach (var raw in rawLines)
        {
            var li = new LineInfo { text = raw, chars = new List<CharMetric>() };
            float cx = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                CharacterInfo ci;
                bool has = _font.GetCharacterInfo(c, out ci, _fontSize, _fontStyle);
                float adv = has ? ci.advance : _fontSize * 0.5f;

                li.chars.Add(new CharMetric
                {
                    ch = c,
                    x = cx,
                    advance = adv,
                    info = ci,
                    hasInfo = has
                });
                cx += adv + _letterSpacing;
            }
            li.width = (cx > 0 ? cx - _letterSpacing : 0) + extra;
            if (li.width > maxW) maxW = li.width;
            lines.Add(li);
        }

        totalWidth = maxW;
        totalHeight = lines.Count * lineH + extra;
        return lines;
    }

    private void ResolveCanvasSize(float textW, float textH, out int w, out int h)
    {
        if (_autoSize)
        {
            w = Mathf.Max(64, Mathf.CeilToInt(textW + _paddingX * 2));
            h = Mathf.Max(64, Mathf.CeilToInt(textH + _paddingY * 2));
            _canvasWidth = w;
            _canvasHeight = h;
        }
        else { w = _canvasWidth; h = _canvasHeight; }
    }

    private float LineOffsetX(float lineWidth, float canvasInnerW)
    {
        switch (_align)
        {
            case Align.Center: return (canvasInnerW - lineWidth) * 0.5f;
            case Align.Right: return canvasInnerW - lineWidth;
            default: return 0;
        }
    }

    // ═══════════════════════════════════════════════
    // ベクター SVG
    // ═══════════════════════════════════════════════

    private string BuildVectorSVG()
    {
        float textW, textH;
        var lines = MeasureLines(out textW, out textH);
        int cw, ch;
        ResolveCanvasSize(textW, textH, out cw, out ch);

        float lineH = _fontSize * _lineSpacing;
        float innerW = cw - _paddingX * 2;
        float margin = TotalMargin();
        string hex = ColHex(_textColor);
        string name = _font.name;
        string wt = (_fontStyle == FontStyle.Bold || _fontStyle == FontStyle.BoldAndItalic) ? "bold" : "normal";
        string it = (_fontStyle == FontStyle.Italic || _fontStyle == FontStyle.BoldAndItalic) ? "italic" : "normal";

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" " +
                       $"width=\"{cw}\" height=\"{ch}\" viewBox=\"0 0 {cw} {ch}\">");

        if (_includeBg)
            sb.AppendLine($"  <rect width=\"{cw}\" height=\"{ch}\" " +
                           $"fill=\"{ColHex(_bgColor)}\" opacity=\"{_bgColor.a:F2}\"/>");

        // ── アウトラインレイヤー（有効時） ──
        if (_enableOutline)
        {
            float sw = (_outlineWidth + _boldness) * 2f;
            sb.AppendLine($"  <g font-family=\"'{name}', sans-serif\" font-size=\"{_fontSize}\" " +
                           $"font-weight=\"{wt}\" font-style=\"{it}\" " +
                           $"fill=\"{ColHex(_outlineColor)}\" opacity=\"{_outlineColor.a:F2}\" " +
                           $"stroke=\"{ColHex(_outlineColor)}\" stroke-width=\"{sw:F1}\" " +
                           $"stroke-linejoin=\"round\" stroke-linecap=\"round\" " +
                           $"paint-order=\"stroke\">");

            AppendTextElements(sb, lines, lineH, innerW, margin);
            sb.AppendLine("  </g>");
        }

        // ── 文字色レイヤー ──
        {
            string strokeAttr = "";
            if (_boldness > 0.1f)
            {
                strokeAttr = $" stroke=\"{hex}\" stroke-width=\"{(_boldness * 2f):F1}\" " +
                             $"stroke-linejoin=\"round\" stroke-linecap=\"round\" paint-order=\"stroke\"";
            }

            sb.AppendLine($"  <g font-family=\"'{name}', sans-serif\" font-size=\"{_fontSize}\" " +
                           $"font-weight=\"{wt}\" font-style=\"{it}\" " +
                           $"fill=\"{hex}\" opacity=\"{_textColor.a:F2}\"{strokeAttr}>");

            AppendTextElements(sb, lines, lineH, innerW, margin);
            sb.AppendLine("  </g>");
        }

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private void AppendTextElements(
        StringBuilder sb, List<LineInfo> lines,
        float lineH, float innerW, float margin)
    {
        for (int li = 0; li < lines.Count; li++)
        {
            var line = lines[li];
            float ox = _paddingX + margin + LineOffsetX(line.width, innerW);
            float by = _paddingY + margin + li * lineH + _fontSize * 0.85f;

            if (Mathf.Approximately(_letterSpacing, 0f))
            {
                sb.AppendLine($"    <text x=\"{ox:F1}\" y=\"{by:F1}\">{Esc(line.text)}</text>");
            }
            else
            {
                sb.Append($"    <text y=\"{by:F1}\">");
                foreach (var cm in line.chars)
                    sb.Append($"<tspan x=\"{(ox + cm.x):F1}\">{Esc(cm.ch.ToString())}</tspan>");
                sb.AppendLine("</text>");
            }
        }
    }

    // ═══════════════════════════════════════════════
    // ラスタライズ SVG / PNG
    // ═══════════════════════════════════════════════

    private string BuildRasterizedSVG()
    {
        float textW, textH;
        MeasureLines(out textW, out textH);
        int cw, ch;
        ResolveCanvasSize(textW, textH, out cw, out ch);

        Texture2D rendered = RenderTextToTexture(cw, ch);
        byte[] png = rendered.EncodeToPNG();
        string base64 = Convert.ToBase64String(png);
        DestroyImmediate(rendered);

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" " +
                       $"xmlns:xlink=\"http://www.w3.org/1999/xlink\" " +
                       $"width=\"{cw}\" height=\"{ch}\" viewBox=\"0 0 {cw} {ch}\">");
        if (_includeBg)
            sb.AppendLine($"  <rect width=\"{cw}\" height=\"{ch}\" " +
                           $"fill=\"{ColHex(_bgColor)}\" opacity=\"{_bgColor.a:F2}\"/>");
        sb.AppendLine($"  <image width=\"{cw}\" height=\"{ch}\" " +
                       $"href=\"data:image/png;base64,{base64}\"/>");
        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private void ExportPNG(string path)
    {
        float textW, textH;
        MeasureLines(out textW, out textH);
        int cw, ch;
        ResolveCanvasSize(textW, textH, out cw, out ch);

        Texture2D rendered = RenderTextToTexture(cw, ch);
        File.WriteAllBytes(path, rendered.EncodeToPNG());
        DestroyImmediate(rendered);
    }

    /// <summary>
    /// フォントアトラスからグリフをブリットし、
    /// ダイレーションでアウトライン → 太さ → 文字色の順に合成。
    /// </summary>
    private Texture2D RenderTextToTexture(int w, int h)
    {
        string clean = _text.Replace("\r\n", "\n").Replace("\r", "\n");
        _font.RequestCharactersInTexture(clean, _fontSize, _fontStyle);

        // ── アトラスコピー ──
        Texture fontTex = _font.material.mainTexture;
        RenderTexture rtTmp = RenderTexture.GetTemporary(
            fontTex.width, fontTex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(fontTex, rtTmp);
        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = rtTmp;
        Texture2D atlas = new Texture2D(fontTex.width, fontTex.height, TextureFormat.RGBA32, false);
        atlas.ReadPixels(new Rect(0, 0, fontTex.width, fontTex.height), 0, 0);
        atlas.Apply();
        RenderTexture.active = prevRT;
        RenderTexture.ReleaseTemporary(rtTmp);

        int atlasW = atlas.width;
        int atlasH = atlas.height;
        Color[] atlasPx = atlas.GetPixels();

        // ── 出力ピクセル ──
        Color bg = _includeBg ? _bgColor : Color.clear;
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;

        // ── 元アルファバッファ ──
        float[] rawAlpha = new float[w * h];

        float textW, textH;
        var lines = MeasureLines(out textW, out textH);
        float lineH = _fontSize * _lineSpacing;
        float innerW = w - _paddingX * 2;
        float margin = TotalMargin();

        // ── グリフ描画 ──
        for (int li = 0; li < lines.Count; li++)
        {
            var line = lines[li];
            float ox = _paddingX + margin + LineOffsetX(line.width, innerW);
            float baselineY = _paddingY + margin + li * lineH + _fontSize;

            foreach (var cm in line.chars)
            {
                if (!cm.hasInfo) continue;
                CharacterInfo ci = cm.info;
                int gw = ci.glyphWidth, gh = ci.glyphHeight;
                if (gw <= 0 || gh <= 0) continue;

                int dstX = Mathf.RoundToInt(ox + cm.x) + ci.minX;
                int dstTopY = Mathf.RoundToInt(baselineY) - ci.maxY;

                Vector2 uvTL = ci.uvTopLeft, uvTR = ci.uvTopRight;
                Vector2 uvBL = ci.uvBottomLeft, uvBR = ci.uvBottomRight;

                for (int py = 0; py < gh; py++)
                {
                    float tY = (float)py / Mathf.Max(1, gh - 1);
                    for (int px = 0; px < gw; px++)
                    {
                        float tX = (float)px / Mathf.Max(1, gw - 1);
                        float u = Mathf.Lerp(Mathf.Lerp(uvTL.x, uvTR.x, tX), Mathf.Lerp(uvBL.x, uvBR.x, tX), tY);
                        float v = Mathf.Lerp(Mathf.Lerp(uvTL.y, uvTR.y, tX), Mathf.Lerp(uvBL.y, uvBR.y, tX), tY);

                        int sx = Mathf.Clamp(Mathf.RoundToInt(u * (atlasW - 1)), 0, atlasW - 1);
                        int sy = Mathf.Clamp(Mathf.RoundToInt(v * (atlasH - 1)), 0, atlasH - 1);

                        float srcA = atlasPx[sy * atlasW + sx].a;
                        if (srcA < 0.004f) continue;

                        int tx = dstX + px;
                        int ty = (h - 1) - (dstTopY + py);
                        if (tx < 0 || tx >= w || ty < 0 || ty >= h) continue;

                        rawAlpha[ty * w + tx] = Mathf.Max(rawAlpha[ty * w + tx], srcA);
                    }
                }
            }
        }

        // ── ダイレーション: アウトライン用 ──
        float outlineR = _enableOutline ? _boldness + _outlineWidth : 0f;
        float boldR = _boldness;

        float[] outlineAlpha = (outlineR > 0.25f) ? Dilate(rawAlpha, w, h, outlineR) : null;
        float[] boldAlpha = (boldR > 0.25f) ? Dilate(rawAlpha, w, h, boldR) : rawAlpha;

        // ── 合成: アウトライン → 文字色 ──
        for (int i = 0; i < pixels.Length; i++)
        {
            Color dst = pixels[i];

            // アウトライン
            if (outlineAlpha != null)
            {
                float a = outlineAlpha[i] * _outlineColor.a;
                if (a > 0.004f)
                {
                    dst = new Color(
                        Mathf.Lerp(dst.r, _outlineColor.r, a),
                        Mathf.Lerp(dst.g, _outlineColor.g, a),
                        Mathf.Lerp(dst.b, _outlineColor.b, a),
                        Mathf.Clamp01(dst.a + a * (1f - dst.a)));
                }
            }

            // 文字色（太さ済み）
            {
                float a = boldAlpha[i] * _textColor.a;
                if (a > 0.004f)
                {
                    dst = new Color(
                        Mathf.Lerp(dst.r, _textColor.r, a),
                        Mathf.Lerp(dst.g, _textColor.g, a),
                        Mathf.Lerp(dst.b, _textColor.b, a),
                        Mathf.Clamp01(dst.a + a * (1f - dst.a)));
                }
            }

            pixels[i] = dst;
        }

        Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
        result.SetPixels(pixels);
        result.Apply();
        DestroyImmediate(atlas);
        return result;
    }

    /// <summary>ディスク型ダイレーション</summary>
    private static float[] Dilate(float[] src, int w, int h, float radius)
    {
        float[] dst = new float[w * h];
        int r = Mathf.CeilToInt(radius);
        float r2 = radius * radius;

        for (int y = 0; y < h; y++)
        {
            int yMin = Mathf.Max(0, y - r);
            int yMax = Mathf.Min(h - 1, y + r);
            for (int x = 0; x < w; x++)
            {
                float maxA = 0f;
                int xMin = Mathf.Max(0, x - r);
                int xMax = Mathf.Min(w - 1, x + r);

                for (int sy = yMin; sy <= yMax; sy++)
                {
                    int dy = sy - y;
                    int dy2 = dy * dy;
                    for (int sx = xMin; sx <= xMax; sx++)
                    {
                        int dx = sx - x;
                        if (dx * dx + dy2 > r2) continue;
                        float a = src[sy * w + sx];
                        if (a > maxA) maxA = a;
                    }
                }
                dst[y * w + x] = maxA;
            }
        }
        return dst;
    }

    // ═══════════════════════════════════════════════
    // Sprite 自動設定
    // ═══════════════════════════════════════════════

    private static void ConfigureAsSpriteAsset(string assetPath)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();

        Debug.Log($"<b>[FontToSVG]</b> Sprite 設定完了: {assetPath}\n" +
                  "→ Image の <b>Source Image</b> にドラッグ＆ドロップで使用できます。");
    }

    // ═══════════════════════════════════════════════
    // ユーティリティ
    // ═══════════════════════════════════════════════

    private List<Vector2> BuildDiskOffsets(float radius)
    {
        var list = new List<Vector2>();
        if (radius < 0.25f) { list.Add(Vector2.zero); return list; }
        int r = Mathf.CeilToInt(radius);
        float r2 = radius * radius;
        for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
                if (dx * dx + dy * dy <= r2)
                    list.Add(new Vector2(dx, dy));
        return list;
    }

    private void DrawCheckerboard(Rect area, float cellSize)
    {
        Color c1 = new Color(0.92f, 0.92f, 0.92f), c2 = new Color(0.78f, 0.78f, 0.78f);
        int cols = Mathf.CeilToInt(area.width / cellSize);
        int rows = Mathf.CeilToInt(area.height / cellSize);
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                EditorGUI.DrawRect(new Rect(
                    area.x + x * cellSize, area.y + y * cellSize,
                    Mathf.Min(cellSize, area.xMax - (area.x + x * cellSize)),
                    Mathf.Min(cellSize, area.yMax - (area.y + y * cellSize))),
                    ((x + y) % 2 == 0) ? c1 : c2);
    }

    private void DrawRectOutline(Rect r, Color c)
    {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), c);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), c);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 1, r.height), c);
        EditorGUI.DrawRect(new Rect(r.xMax - 1, r.y, 1, r.height), c);
    }

    private static string ColHex(Color c) =>
        string.Format("#{0:X2}{1:X2}{2:X2}",
            Mathf.RoundToInt(c.r * 255), Mathf.RoundToInt(c.g * 255), Mathf.RoundToInt(c.b * 255));

    private static string Esc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
         .Replace("\"", "&quot;").Replace("'", "&apos;");
}
#endif