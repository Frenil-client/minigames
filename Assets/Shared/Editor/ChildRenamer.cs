using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 선택한 오브젝트의 직계 자식들을 순번(01~99)으로 일괄 이름변경하는 에디터 유틸.
///
/// 실행 방법
///   · 상단 메뉴  : Tools → Child Renamer
///   · 우클릭     : GameObject → Child Renamer
/// </summary>
public class ChildRenamer : EditorWindow
{
    // ── 설정 ──────────────────────────────────────────────────────────────
    private string _baseName    = "";
    private string _separator   = "_";
    private int    _startFrom   = 1;
    private int    _formatIndex = 0;   // 0=D2(01)  1=D3(001)  2=없음(1)

    private static readonly string[] _formatLabels  = { "01 (두 자리)", "001 (세 자리)", "1 (자릿수 없음)" };
    private static readonly string[] _formatStrings = { "D2", "D3", "" };

    // ── 상태 ──────────────────────────────────────────────────────────────
    private GameObject _target;
    private Vector2    _scrollPos;

    // ── 메뉴 진입점 ───────────────────────────────────────────────────────

    [MenuItem("Tools/Child Renamer")]
    private static void OpenFromMenu() => Open();

    [MenuItem("GameObject/Child Renamer", false, 20)]
    private static void OpenFromHierarchy() => Open();

    [MenuItem("GameObject/Child Renamer", true)]
    private static bool ValidateHierarchy() =>
        Selection.activeGameObject != null &&
        Selection.activeGameObject.transform.childCount > 0;

    private static void Open()
    {
        var win = GetWindow<ChildRenamer>("Child Renamer");
        win.minSize = new Vector2(300, 420);
        win.RefreshTarget();
    }

    // ── Unity 콜백 ────────────────────────────────────────────────────────

    private void OnEnable()        => RefreshTarget();
    private void OnSelectionChange() => RefreshTarget();

    private void RefreshTarget()
    {
        _target = Selection.activeGameObject;
        if (_target != null && _target.transform.childCount > 0)
            _baseName = StripTrailingNumbers(_target.transform.GetChild(0).name);
        Repaint();
    }

    // ── GUI ───────────────────────────────────────────────────────────────

    private void OnGUI()
    {
        EditorGUILayout.Space(8);

        // ── 대상 ─────────────────────────────────────────────────────────
        DrawHeader("대상 오브젝트");

        if (_target == null)
        {
            EditorGUILayout.HelpBox("Hierarchy 에서 부모 오브젝트를 선택하세요.", MessageType.Info);
            return;
        }

        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField(_target, typeof(GameObject), true);

        int childCount = _target.transform.childCount;
        if (childCount == 0)
        {
            EditorGUILayout.HelpBox("직계 자식이 없습니다.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField($"직계 자식 수 : {childCount} 개", EditorStyles.miniLabel);
        EditorGUILayout.Space(8);

        // ── 설정 ─────────────────────────────────────────────────────────
        DrawHeader("설정");
        _baseName    = EditorGUILayout.TextField("기본 이름",  _baseName);
        _separator   = EditorGUILayout.TextField("구분자",     _separator);
        _formatIndex = EditorGUILayout.Popup("번호 형식",      _formatIndex, _formatLabels);
        _startFrom   = Mathf.Clamp(
            EditorGUILayout.IntField("시작 번호", _startFrom), 1, 99);

        EditorGUILayout.Space(8);

        // ── 미리보기 ──────────────────────────────────────────────────────
        DrawHeader("미리보기");

        int previewCount = Mathf.Min(childCount, 6);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
            GUILayout.Height(previewCount * EditorGUIUtility.singleLineHeight + 6));

        for (int i = 0; i < previewCount; i++)
        {
            string current = _target.transform.GetChild(i).name;
            string next    = BuildName(i);
            EditorGUILayout.LabelField(
                $"  {current}",
                $"→  {next}",
                EditorStyles.miniLabel);
        }
        if (childCount > previewCount)
            EditorGUILayout.LabelField($"  … ({childCount - previewCount}개 더)", EditorStyles.miniLabel);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space(8);

        // ── 적용 ─────────────────────────────────────────────────────────
        bool valid = !string.IsNullOrEmpty(_baseName) || _formatIndex >= 0;
        using (new EditorGUI.DisabledScope(!valid))
        {
            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.45f, 0.85f, 0.45f);
            if (GUILayout.Button("적용", GUILayout.Height(34)))
                Apply();
            GUI.backgroundColor = prevColor;
        }

        EditorGUILayout.Space(4);
    }

    // ── 로직 ──────────────────────────────────────────────────────────────

    private void Apply()
    {
        if (_target == null) return;

        Undo.SetCurrentGroupName("Child Renamer");
        int group = Undo.GetCurrentGroup();

        int count = _target.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            GameObject child = _target.transform.GetChild(i).gameObject;
            Undo.RecordObject(child, "Rename Child");
            child.name = BuildName(i);
        }

        Undo.CollapseUndoOperations(group);
        EditorUtility.SetDirty(_target);

        Debug.Log($"[ChildRenamer] {count}개 완료 :  {BuildName(0)}  ~  {BuildName(count - 1)}");
    }

    private string BuildName(int index)
    {
        int    num    = _startFrom + index;
        string numStr = string.IsNullOrEmpty(_formatStrings[_formatIndex])
            ? num.ToString()
            : num.ToString(_formatStrings[_formatIndex]);

        return string.IsNullOrEmpty(_baseName)
            ? numStr
            : $"{_baseName}{_separator}{numStr}";
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────────────

    /// <summary>이름 끝의 숫자·구분자를 제거한다. "Waypoint_01" → "Waypoint"</summary>
    private static string StripTrailingNumbers(string name) =>
        Regex.Replace(name, @"[\s_\-]?\d+$", "").Trim();

    private static void DrawHeader(string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        var rect = GUILayoutUtility.GetLastRect();
        rect.y    += EditorGUIUtility.singleLineHeight - 2;
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        EditorGUILayout.Space(2);
    }
}
