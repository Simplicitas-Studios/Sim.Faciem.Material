using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using Sim.Faciem.Material.Icons;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Samples.Icon
{
    /// <summary>
    /// ViewModel for the Material icon demo page.
    /// Enumerates Font Awesome SVG assets from both package folders and filters them by file name.
    /// </summary>
    public class IconDemoViewModel : ViewModel<IconDemoViewModel>, IIconDemoDataContext
    {
        private readonly List<IconDemoItem> _allIcons = new();

        private List<IconDemoItem> _filteredIcons = new();
        private string _searchQuery = string.Empty;

        [CreateProperty]
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        [CreateProperty]
        public IList FilteredIcons
        {
            get => _filteredIcons;
            private set => SetProperty(ref _filteredIcons, value.OfType<IconDemoItem>().ToList());
        }

        public IconDemoViewModel()
        {
            Disposables.Add(
                Property.Observe(vm => vm.SearchQuery)
                    .Select(query => query.ToLowerInvariant())
                    .Subscribe(query => ApplyFilter(query)));
        }
        
        protected override async UniTask NavigateTo()
        {
            await LoadIcons();
            ApplyFilter(SearchQuery.ToLowerInvariant());
        }

        private async ValueTask LoadIcons()
        {
            await IconCollectionRegistry.DiscoverCollectionsAsync();
            _allIcons.Clear();

            var iconCollections = IconCollectionRegistry.GetAllCollections();

            foreach (var iconCollection in iconCollections)
            {
                foreach (var icon in iconCollection.Icons)
                {
                    _allIcons.Add(new IconDemoItem
                    {
                        FileName = icon.name,
                        IconSprite = new StyleBackground(icon)
                    });
                }
            }

            _allIcons.Sort(static (left, right) => string.Compare(left.FileName, right.FileName, StringComparison.OrdinalIgnoreCase));
        }

        private void ApplyFilter(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                FilteredIcons = _allIcons.ToList();
                return;
            }

            FilteredIcons = _allIcons
                .Where(icon => icon.FileName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }
    }
}
