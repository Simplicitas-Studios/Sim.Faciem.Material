using InternalEditor;
using Sim.Faciem.Material.AddressableExt;
using Sim.Faciem.Material.Icons;
using UnityEditor;

namespace Sim.Faciem.Material.Editor.Icons
{
    public class IconCollectionAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                var asset = AssetDatabase.LoadAssetAtPath<IconCollection>(path);

                if (asset)
                {
                    AddressableHelper.CreateAssetEntry(asset, FaciemMaterialAddressables.IconCollectionLabel);
                }
            }
        }
    }
}