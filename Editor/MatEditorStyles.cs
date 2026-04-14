using System;
using R3;
using UnityEditor;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Editor
{
    /// <summary>
    /// Programmatically injects Material button stylesheets into editor-context
    /// <see cref="VisualElement"/> roots (EditorWindow, PopupWindowContent, ToolbarOverlay).
    ///
    /// Call <see cref="ApplyTo"/> to inject styles and receive an <see cref="IDisposable"/>
    /// subscription — dispose it when the root is destroyed so the subscription stops.
    /// The subscription re-applies styles automatically whenever the active theme changes.
    ///
    /// Change the theme via <see cref="SetTheme"/> or the <c>Faciem / Theme</c> menu.
    /// Default theme: Purple / Green.  Choice is persisted to <c>EditorPrefs</c>.
    ///
    /// Runtime panels use PanelSettings + a .tss theme file instead of this helper.
    /// </summary>
    public static class MatEditorStyles
    {
        // ── Asset paths ────────────────────────────────────────────────────────
        private const string PackageRoot      = "Packages/com.sim.faciem-material";
        private const string ButtonStylesPath = PackageRoot + "/Runtime/Controls/Styles/MatButtonStyles.uss";
        private const string IndigoPath       = PackageRoot + "/Runtime/Themes/MatIndigoTheme.uss";
        private const string DeepPurplePath   = PackageRoot + "/Runtime/Themes/MatDeepPurpleTheme.uss";
        private const string PinkBlueGreyPath = PackageRoot + "/Runtime/Themes/MatPinkBlueGreyTheme.uss";
        private const string PurpleGreenPath  = PackageRoot + "/Runtime/Themes/MatPurpleGreenTheme.uss";

        private const string ThemePrefKey = "Sim.Faciem.Material.EditorTheme";

        // ── Menu path constants (must match [MenuItem] strings exactly) ────────
        private const string MenuPurpleGreen  = "Faciem/Theme/Purple Green";
        private const string MenuIndigo       = "Faciem/Theme/Indigo Pink";
        private const string MenuDeepPurple   = "Faciem/Theme/Deep Purple Amber";
        private const string MenuPinkBlueGrey = "Faciem/Theme/Pink Blue Grey";

        // ── Style sheet cache ──────────────────────────────────────────────────
        private static StyleSheet s_buttonStyles;
        private static StyleSheet s_indigoSheet;
        private static StyleSheet s_deepPurpleSheet;
        private static StyleSheet s_pinkBlueGreySheet;
        private static StyleSheet s_purpleGreenSheet;

        // ── Active theme ───────────────────────────────────────────────────────
        private static readonly ReactiveProperty<MatEditorTheme> s_activeTheme;

        /// <summary>
        /// Observable stream of the active editor theme.
        /// Emits the current value immediately on subscribe, then on every change.
        /// </summary>
        public static Observable<MatEditorTheme> ActiveTheme => s_activeTheme;

        // ── Static constructor ─────────────────────────────────────────────────
        static MatEditorStyles()
        {
            var saved = (MatEditorTheme)EditorPrefs.GetInt(ThemePrefKey, (int)MatEditorTheme.PurpleGreen);
            s_activeTheme = new ReactiveProperty<MatEditorTheme>(saved);

            // Restore checkmarks after domain reload.
            EditorApplication.delayCall += UpdateMenuCheckmarks;
            // Clear stylesheet cache after play-mode transitions.
            EditorApplication.playModeStateChanged += _ => ClearCache();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Injects <c>MatButtonStyles.uss</c> and the active theme USS into
        /// <paramref name="root"/>, then returns an <see cref="IDisposable"/>
        /// that re-applies styles automatically whenever the active theme changes.
        /// <para>Dispose the returned value when the root is destroyed.</para>
        /// </summary>
        public static IDisposable ApplyTo(VisualElement root)
        {
            ApplyCurrent(root);
            return s_activeTheme.Subscribe(_ => ApplyCurrent(root));
        }

        /// <summary>
        /// Changes the active Material editor theme, persists it to
        /// <c>EditorPrefs</c>, updates menu checkmarks, and notifies all live
        /// <see cref="ApplyTo"/> subscriptions to re-style their roots instantly.
        /// </summary>
        public static void SetTheme(MatEditorTheme theme)
        {
            EditorPrefs.SetInt(ThemePrefKey, (int)theme);
            s_activeTheme.Value = theme;
            UpdateMenuCheckmarks();
        }

        /// <summary>Removes all Material stylesheets previously added by <see cref="ApplyTo"/>.</summary>
        public static void RemoveFrom(VisualElement root)
        {
            EnsureAllLoaded();
            TryRemove(root, s_buttonStyles);
            TryRemove(root, s_indigoSheet);
            TryRemove(root, s_deepPurpleSheet);
            TryRemove(root, s_pinkBlueGreySheet);
            TryRemove(root, s_purpleGreenSheet);
        }

        // ── Faciem / Theme menu ────────────────────────────────────────────────

        [MenuItem(MenuPurpleGreen)]
        private static void MenuSetPurpleGreen()  => SetTheme(MatEditorTheme.PurpleGreen);

        [MenuItem(MenuPurpleGreen, true)]
        private static bool ValidatePurpleGreen()
        {
            Menu.SetChecked(MenuPurpleGreen, s_activeTheme.Value == MatEditorTheme.PurpleGreen);
            return true;
        }

        [MenuItem(MenuIndigo)]
        private static void MenuSetIndigo()       => SetTheme(MatEditorTheme.Indigo);

        [MenuItem(MenuIndigo, true)]
        private static bool ValidateIndigo()
        {
            Menu.SetChecked(MenuIndigo, s_activeTheme.Value == MatEditorTheme.Indigo);
            return true;
        }

        [MenuItem(MenuDeepPurple)]
        private static void MenuSetDeepPurple()   => SetTheme(MatEditorTheme.DeepPurple);

        [MenuItem(MenuDeepPurple, true)]
        private static bool ValidateDeepPurple()
        {
            Menu.SetChecked(MenuDeepPurple, s_activeTheme.Value == MatEditorTheme.DeepPurple);
            return true;
        }

        [MenuItem(MenuPinkBlueGrey)]
        private static void MenuSetPinkBlueGrey() => SetTheme(MatEditorTheme.PinkBlueGrey);

        [MenuItem(MenuPinkBlueGrey, true)]
        private static bool ValidatePinkBlueGrey()
        {
            Menu.SetChecked(MenuPinkBlueGrey, s_activeTheme.Value == MatEditorTheme.PinkBlueGrey);
            return true;
        }

        // ── Internals ──────────────────────────────────────────────────────────

        private static void ApplyCurrent(VisualElement root)
        {
            // Guard: root may have been detached between a theme change and this callback.
            if (root?.panel == null) return;

            RemoveFrom(root);
            EnsureAllLoaded();

            if (s_buttonStyles != null)
                root.styleSheets.Add(s_buttonStyles);

            var themeSheet = GetThemeSheet(s_activeTheme.Value);
            if (themeSheet != null)
                root.styleSheets.Add(themeSheet);
        }

        private static StyleSheet GetThemeSheet(MatEditorTheme theme) => theme switch
        {
            MatEditorTheme.Indigo       => s_indigoSheet,
            MatEditorTheme.DeepPurple   => s_deepPurpleSheet,
            MatEditorTheme.PinkBlueGrey => s_pinkBlueGreySheet,
            MatEditorTheme.PurpleGreen  => s_purpleGreenSheet,
            _                           => s_purpleGreenSheet,
        };

        private static void EnsureAllLoaded()
        {
            s_buttonStyles      ??= AssetDatabase.LoadAssetAtPath<StyleSheet>(ButtonStylesPath);
            s_indigoSheet       ??= AssetDatabase.LoadAssetAtPath<StyleSheet>(IndigoPath);
            s_deepPurpleSheet   ??= AssetDatabase.LoadAssetAtPath<StyleSheet>(DeepPurplePath);
            s_pinkBlueGreySheet ??= AssetDatabase.LoadAssetAtPath<StyleSheet>(PinkBlueGreyPath);
            s_purpleGreenSheet  ??= AssetDatabase.LoadAssetAtPath<StyleSheet>(PurpleGreenPath);
        }

        private static void TryRemove(VisualElement root, StyleSheet sheet)
        {
            if (sheet != null && root.styleSheets.Contains(sheet))
                root.styleSheets.Remove(sheet);
        }

        private static void ClearCache()
        {
            s_buttonStyles      = null;
            s_indigoSheet       = null;
            s_deepPurpleSheet   = null;
            s_pinkBlueGreySheet = null;
            s_purpleGreenSheet  = null;
        }

        private static void UpdateMenuCheckmarks()
        {
            Menu.SetChecked(MenuPurpleGreen,  s_activeTheme.Value == MatEditorTheme.PurpleGreen);
            Menu.SetChecked(MenuIndigo,       s_activeTheme.Value == MatEditorTheme.Indigo);
            Menu.SetChecked(MenuDeepPurple,   s_activeTheme.Value == MatEditorTheme.DeepPurple);
            Menu.SetChecked(MenuPinkBlueGrey, s_activeTheme.Value == MatEditorTheme.PinkBlueGrey);
        }
    }
}

