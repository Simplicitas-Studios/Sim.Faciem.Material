using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Sim.Faciem.Controls
{
    /// <summary>
    /// Material-styled list control built on Unity's <see cref="ListView"/>.
    /// Rows are created from <see cref="ItemTemplate"/> when assigned, or from a
    /// fallback text label when no template is provided.
    /// </summary>
    [UxmlElement]
    public partial class MatList : ListView
    {
        /// <summary>Base CSS class applied to every Material list.</summary>
        public const string BaseClassName = "mat-list";

        /// <summary>CSS class applied when selection-list behavior is enabled.</summary>
        public const string SelectionListClassName = "mat-selection-list";

        /// <summary>CSS class for each realized row root.</summary>
        public const string ItemClassName = "mat-list__item";

        /// <summary>CSS class for selected rows.</summary>
        public const string ItemSelectedClassName = "mat-list__item--selected";

        /// <summary>CSS class for the row content host.</summary>
        public const string ItemContentClassName = "mat-list__item-content";

        /// <summary>CSS class for the fallback text label used when no template is assigned.</summary>
        public const string ItemFallbackLabelClassName = "mat-list__item-fallback-label";

        /// <summary>CSS class for the pseudo-checkbox shown in selection-list mode.</summary>
        public const string PseudoCheckboxClassName = "mat-list__pseudo-checkbox";

        /// <summary>CSS class for the checked pseudo-checkbox state.</summary>
        public const string PseudoCheckboxCheckedClassName = "mat-list__pseudo-checkbox--checked";

        private static readonly BindingId s_selectedIndicesId = new(nameof(SelectedIndices));

        private IList _itemSource;
        private VisualTreeAsset _itemTemplate;
        private List<int> _selectedIndices = new();
        private bool _selectionListMode;
        private bool _suppressSelectionSync;

        /// <summary>
        /// Bindable wrapper around the list's data source.
        /// The wrapped <see cref="System.Collections.IList"/> is forwarded to the underlying <see cref="BaseVerticalCollectionView.itemsSource"/>.
        /// </summary>
        [CreateProperty]
        public IList ItemSource
        {
            get => _itemSource;
            set
            {
                _itemSource = value;
                itemsSource = _itemSource;
                ClampSelectedIndices();
                Rebuild();
                ApplySelectionFromProperty();
            }
        }

        /// <summary>
        /// Template cloned for each realized row. When null, rows fall back to a simple text label.
        /// </summary>
        [UxmlAttribute]
        public VisualTreeAsset ItemTemplate
        {
            get => _itemTemplate;
            set
            {
                _itemTemplate = value;
                Rebuild();
            }
        }

        /// <summary>
        /// Selected row indices. For the first version this is the public selection API.
        /// In plain <see cref="MatList"/> mode this value is always empty.
        /// </summary>
        [CreateProperty]
        public List<int> SelectedIndices
        {
            get => _selectedIndices;
            set
            {
                var normalized = NormalizeIndices(value);
                if (_selectedIndices.SequenceEqual(normalized))
                {
                    return;
                }

                _selectedIndices = normalized;
                ApplySelectionFromProperty();
                NotifyPropertyChanged(s_selectedIndicesId);
                RefreshItems();
            }
        }

        /// <summary>
        /// Initializes a new Material list with a fallback row renderer and no selection.
        /// </summary>
        public MatList()
        {
            AddToClassList(BaseClassName);

            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            selectionType = SelectionType.None;
            reorderable = false;
            showBorder = false;

            makeItem = CreateItem;
            bindItem = BindItem;
            unbindItem = UnbindItem;

            selectedIndicesChanged += OnSelectedIndicesChanged;
        }

        /// <summary>
        /// Enables or disables Material selection-list behavior on this list.
        /// </summary>
        /// <param name="enabled">Whether multi-selection behavior should be active.</param>
        protected void SetSelectionListMode(bool enabled)
        {
            _selectionListMode = enabled;
            selectionType = enabled ? SelectionType.Multiple : SelectionType.None;
            EnableInClassList(SelectionListClassName, enabled);

            if (!enabled)
            {
                _selectedIndices = new List<int>();
                ClearSelection();
                NotifyPropertyChanged(s_selectedIndicesId);
            }

            Rebuild();
            RefreshItems();
        }

        private VisualElement CreateItem()
        {
            var row = new VisualElement();
            row.AddToClassList(ItemClassName);

            var references = new RowReferences();

            if (_selectionListMode)
            {
                references.PseudoCheckbox = new VisualElement();
                references.PseudoCheckbox.AddToClassList(PseudoCheckboxClassName);
                row.Add(references.PseudoCheckbox);
            }

            if (ItemTemplate != null)
            {
                references.Content = ItemTemplate.CloneTree();
                references.Content.AddToClassList(ItemContentClassName);
                row.Add(references.Content);
            }
            else
            {
                references.FallbackLabel = new Label();
                references.FallbackLabel.AddToClassList(ItemFallbackLabelClassName);
                row.Add(references.FallbackLabel);
            }

            row.userData = references;
            return row;
        }

        private void BindItem(VisualElement element, int index)
        {
            if (element.userData is not RowReferences references)
            {
                return;
            }

            var item = itemsSource != null && index >= 0 && index < itemsSource.Count
                ? itemsSource[index]
                : null;

            element.dataSource = item;

            if (references.Content != null)
            {
                references.Content.dataSource = item;
            }

            if (references.FallbackLabel != null)
            {
                references.FallbackLabel.text = item?.ToString() ?? string.Empty;
            }

            UpdateRowSelectionState(element, index, references);
        }

        private static void UnbindItem(VisualElement element, int _)
        {
            element.dataSource = null;

            if (element.userData is RowReferences references && references.Content != null)
            {
                references.Content.dataSource = null;
            }
        }

        private void OnSelectedIndicesChanged(IEnumerable<int> indices)
        {
            if (_suppressSelectionSync || !_selectionListMode)
            {
                return;
            }

            _selectedIndices = NormalizeIndices(indices?.ToList());
            NotifyPropertyChanged(s_selectedIndicesId);
            RefreshItems();
        }

        private void UpdateRowSelectionState(VisualElement element, int index, RowReferences references)
        {
            var isSelected = _selectionListMode && _selectedIndices.Contains(index);
            element.EnableInClassList(ItemSelectedClassName, isSelected);

            if (references.PseudoCheckbox != null)
            {
                references.PseudoCheckbox.EnableInClassList(PseudoCheckboxCheckedClassName, isSelected);
            }
        }

        private void ApplySelectionFromProperty()
        {
            if (!_selectionListMode)
            {
                return;
            }

            _suppressSelectionSync = true;
            ClearSelection();

            if (_selectedIndices.Count > 0)
            {
                SetSelectionWithoutNotify(_selectedIndices);
            }

            _suppressSelectionSync = false;
        }

        private void ClampSelectedIndices()
        {
            _selectedIndices = NormalizeIndices(_selectedIndices);
        }

        private List<int> NormalizeIndices(List<int> indices)
        {
            if (!_selectionListMode || itemsSource == null)
            {
                return new List<int>();
            }

            return indices?
                .Where(index => index >= 0 && index < itemsSource.Count)
                .Distinct()
                .OrderBy(index => index)
                .ToList() ?? new List<int>();
        }

        private sealed class RowReferences
        {
            public VisualElement Content { get; set; }
            public Label FallbackLabel { get; set; }
            public VisualElement PseudoCheckbox { get; set; }
        }
    }
}
