using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sim.Faciem.Material.AddressableExt;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Icons
{
    public class IconCollectionRegistry
    {
        private static readonly Dictionary<string, IconCollection> s_collections = new();
        private static bool s_initialized;

        public static async ValueTask DiscoverCollectionsAsync()
        {
            if (s_initialized)
            {
                return;
            }

            s_initialized = true;
            var label = new AssetLabelReference { labelString = FaciemMaterialAddressables.IconCollectionLabel };

            var iconCollections = await Addressables
                .LoadAssetsAsync<IconCollection>(new[] { label }, _ => { }, Addressables.MergeMode.Intersection)
                .Task;

            foreach (var iconCollection in iconCollections)
            {
                RegisterCollection(iconCollection);
            }
        }

        public static void RegisterCollection(IconCollection collection)
        {
            if (collection == null || string.IsNullOrEmpty(collection.CollectionName))
            {
                Debug.LogWarning("Cannot register a null or unnamed IconCollection.");
                return;
            }

            s_collections[collection.CollectionName] = collection;
        }

        public static IconCollection GetCollection(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                Debug.LogWarning("Collection name cannot be null or empty.");
                return null;
            }

            s_collections.TryGetValue(collectionName, out var collection);
            return collection;
        }

        public static IReadOnlyList<IconCollection> GetAllCollections()
        {
            return new List<IconCollection>(s_collections.Values);
        }

        public static VectorImage GetIcon(string collectionName, string iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                var collection = GetCollection(collectionName);
                return FindIcon(collection, iconName);
            }

            foreach (var collection in s_collections.Values)
            {
                var icon = FindIcon(collection, iconName);
                if (icon != null)
                {
                    return icon;
                }
            }

            return null;
        }

        private static VectorImage FindIcon(IconCollection collection, string iconName)
        {
            if (collection?.Icons == null)
            {
                return null;
            }

            foreach (var icon in collection.Icons)
            {
                if (icon == null)
                {
                    continue;
                }

                if (string.Equals(icon.name, iconName, StringComparison.OrdinalIgnoreCase))
                {
                    return icon;
                }
            }

            return null;
        }
    }
}
