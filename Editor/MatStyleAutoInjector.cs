using System.IO;
using System.Linq;
using UnityEditor;

namespace Sim.Faciem.Material.Editor
{
    /// <summary>
    /// Automatically detects Material Design controls in UXML files and prompts
    /// the user to configure their Panel Settings once per editor session.
    /// Manual trigger: <c>Tools &gt; Sim.Faciem &gt; Material &gt; Setup Stylesheets</c>.
    /// </summary>
    [InitializeOnLoad]
    public static class MatStyleAutoInjector
    {
        internal const string SessionDismissedKey = "Sim.Faciem.Material.StylesSetup.Dismissed";

        static MatStyleAutoInjector()
        {
            // Delay the initial check so AssetDatabase is fully initialised.
            EditorApplication.delayCall += CheckOnStartup;
            EditorApplication.projectChanged += OnProjectChanged;
        }

        // ── Menu item ──────────────────────────────────────────────────────────

        [MenuItem("Tools/Sim.Faciem/Material/Setup Stylesheets")]
        public static void OpenSetupWindowManual()
        {
            // Manual open always shows the window regardless of session state.
            SessionState.SetBool(SessionDismissedKey, false);
            MatMaterialSetupWindow.ShowWindow();
        }

        // ── Detection helpers (internal so MatMaterialSetupWindow can reuse) ───

        internal static bool HasMaterialControlsInProject()
        {
            return AssetDatabase
                .FindAssets("t:VisualTreeAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(File.Exists)
                .Any(path =>
                {
                    var content = File.ReadAllText(path);
                    return content.Contains("MatButton")
                        || content.Contains("Sim.Faciem.Controls.Mat");
                });
        }

        internal static string[] FindUnconfiguredPanelSettingsPaths()
        {
            if (!HasMaterialControlsInProject())
                return System.Array.Empty<string>();

            return AssetDatabase
                .FindAssets("t:PanelSettings")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path =>
                {
                    var settings = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>(path);
                    return settings != null && settings.themeStyleSheet == null;
                })
                .ToArray();
        }

        // ── Private ────────────────────────────────────────────────────────────

        private static void CheckOnStartup()
        {
            TryOpenSetupWindow();
        }

        private static void OnProjectChanged()
        {
            TryOpenSetupWindow();
        }

        private static void TryOpenSetupWindow()
        {
            if (SessionState.GetBool(SessionDismissedKey, false))
                return;

            if (FindUnconfiguredPanelSettingsPaths().Length == 0)
                return;

            MatMaterialSetupWindow.ShowWindow();
        }
    }

    /// <summary>
    /// Post-processor that triggers the setup check whenever UXML files are imported.
    /// </summary>
    public class MatControlAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (SessionState.GetBool(MatStyleAutoInjector.SessionDismissedKey, false))
                return;

            var hasNewMaterialUxml = importedAssets.Any(path =>
                path.EndsWith(".uxml")
                && File.Exists(path)
                && File.ReadAllText(path).Contains("MatButton"));

            if (!hasNewMaterialUxml)
                return;

            if (MatStyleAutoInjector.FindUnconfiguredPanelSettingsPaths().Length > 0)
                MatMaterialSetupWindow.ShowWindow();
        }
    }
}

