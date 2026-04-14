using Sim.Faciem.Material.Editor;
using UnityEditor;
using UnityEngine;

namespace Sim.Faciem.Material.Samples.Editor
{
    /// <summary>
    /// Editor window that hosts the Material Demo using the full Sim.Faciem
    /// MVVM stack. Open via <c>Faciem &gt; Material Demo</c>.
    ///
    /// Before the window works correctly, create the following assets in Unity
    /// inside this Samples folder and wire them up to this window's Inspector:
    /// <list type="bullet">
    ///   <item>EditorViewIdAsset — MatDemoWindowViewId  (ViewId._id = "MatDemoWindow")</item>
    ///   <item>RegionNameDefinition — MatDemoWindowRegion  (_name = "MatDemo/Window")</item>
    ///   <item>EditorViewIdAsset × 3 for GettingStarted, Theming, Button pages</item>
    ///   <item>RegionNameDefinition — MatDemoContentRegion  (_name = "MatDemo/Content")</item>
    ///   <item>EditorServiceInstaller — registers all four ViewModels</item>
    /// </list>
    /// </summary>
    public class MatDemoWindow : MatFaciemEditorWindow
    {
        [MenuItem("Faciem/Material Demo")]
        public static void ShowMatDemoWindow()
        {
            var wnd = GetWindow<MatDemoWindow>();
            wnd.titleContent = new GUIContent("Material Demo");
            wnd.minSize = new Vector2(800, 520);
        }
    }
}

