using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Editor
{
    /// <summary>
    /// Editor window that guides the user through applying a Material theme to
    /// their Panel Settings assets.
    ///
    /// Opens automatically when unconfigured Panel Settings are detected.
    /// Also reachable via <c>Tools &gt; Sim.Faciem &gt; Material &gt; Setup Stylesheets</c>.
    /// </summary>
    public class MatMaterialSetupWindow : EditorWindow
    {
        private const string PackageRoot = "Packages/com.sim.faciem-material/Runtime/Themes";

        private static readonly (string Label, string TssPath)[] s_themes =
        {
            ("Indigo / Pink (Light)",        PackageRoot + "/MatIndigoTheme.tss"),
            ("Deep Purple / Amber (Light)",  PackageRoot + "/MatDeepPurpleTheme.tss"),
            ("Pink / Blue Grey (Dark)",      PackageRoot + "/MatPinkBlueGreyTheme.tss"),
            ("Purple / Green (Dark)",        PackageRoot + "/MatPurpleGreenTheme.tss"),
        };

        private string[]           _unconfiguredPaths = Array.Empty<string>();
        private readonly List<bool> _selected          = new();
        private int                _themeIndex;
        private Vector2            _scroll;

        // ── Static entry point ─────────────────────────────────────────────────

        public static void ShowWindow()
        {
            var window = GetWindow<MatMaterialSetupWindow>(true, "Material Stylesheet Setup", true);
            window.minSize = new Vector2(460, 320);
            window.Refresh();
            window.ShowUtility();
        }

        // ── Unity messages ─────────────────────────────────────────────────────

        private void OnEnable()  => Refresh();
        private void OnFocus()   => Refresh();

        private void OnGUI()
        {
            DrawHeader();

            if (_unconfiguredPaths.Length == 0)
            {
                DrawAllConfigured();
                return;
            }

            DrawThemePicker();
            DrawPanelSettingsList();
            DrawFooter();
        }

        // ── Drawing ────────────────────────────────────────────────────────────

        private void DrawHeader()
        {
            EditorGUILayout.Space(8);
            var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
            EditorGUILayout.LabelField("Sim.Faciem — Material Stylesheet Setup", titleStyle);
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "The following Panel Settings have no themeStyleSheet assigned. " +
                "Select a Material theme below and click Apply to configure them.",
                MessageType.Info);
            EditorGUILayout.Space(8);
        }

        private void DrawAllConfigured()
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.HelpBox(
                "All Panel Settings that use Material controls already have a " +
                "themeStyleSheet assigned. Nothing to do!",
                MessageType.Info);

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Close"))
                Close();
        }

        private void DrawThemePicker()
        {
            var themeLabels = new string[s_themes.Length];
            for (var i = 0; i < s_themes.Length; i++)
                themeLabels[i] = s_themes[i].Label;

            _themeIndex = EditorGUILayout.Popup("Theme", _themeIndex, themeLabels);
            EditorGUILayout.Space(6);
        }

        private void DrawPanelSettingsList()
        {
            EditorGUILayout.LabelField("Panel Settings to configure:", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MaxHeight(160));

            for (var i = 0; i < _unconfiguredPaths.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _selected[i] = EditorGUILayout.Toggle(_selected[i], GUILayout.Width(20));

                var asset = AssetDatabase.LoadAssetAtPath<PanelSettings>(_unconfiguredPaths[i]);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(asset, typeof(PanelSettings), false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select All"))
                SetAllSelected(true);

            if (GUILayout.Button("Select None"))
                SetAllSelected(false);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Dismiss", GUILayout.Width(80)))
            {
                SessionState.SetBool(MatStyleAutoInjector.SessionDismissedKey, true);
                Close();
            }

            using (new EditorGUI.DisabledScope(!HasAnySelected()))
            {
                if (GUILayout.Button("Apply", GUILayout.Width(80)))
                    ApplyTheme();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(6);
        }

        // ── Logic ──────────────────────────────────────────────────────────────

        private void Refresh()
        {
            _unconfiguredPaths = MatStyleAutoInjector.FindUnconfiguredPanelSettingsPaths();
            _selected.Clear();
            foreach (var _ in _unconfiguredPaths)
                _selected.Add(true);
            Repaint();
        }

        private void ApplyTheme()
        {
            var (_, tssPath) = s_themes[_themeIndex];
            var tss = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(tssPath);

            if (tss == null)
            {
                EditorUtility.DisplayDialog(
                    "Material Setup",
                    $"Could not load theme file:\n{tssPath}\n\nEnsure the Sim.Faciem.Material package is correctly installed.",
                    "OK");
                return;
            }

            var applied = 0;
            for (var i = 0; i < _unconfiguredPaths.Length; i++)
            {
                if (!_selected[i]) continue;

                var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(_unconfiguredPaths[i]);
                if (settings == null) continue;

                Undo.RecordObject(settings, "Apply Material Theme");
                settings.themeStyleSheet = tss;
                EditorUtility.SetDirty(settings);
                applied++;
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Material Setup",
                $"Applied '{s_themes[_themeIndex].Label}' to {applied} Panel Settings asset(s).",
                "OK");

            SessionState.SetBool(MatStyleAutoInjector.SessionDismissedKey, true);
            Refresh();

            if (_unconfiguredPaths.Length == 0)
                Close();
        }

        private void SetAllSelected(bool value)
        {
            for (var i = 0; i < _selected.Count; i++)
                _selected[i] = value;
        }

        private bool HasAnySelected()
        {
            foreach (var s in _selected)
                if (s) return true;
            return false;
        }
    }
}






