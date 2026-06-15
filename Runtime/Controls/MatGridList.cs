using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Sim.Faciem.Controls
{
    /// <summary>
    /// Material-styled grid list inspired by Angular Material's <c>mat-grid-list</c>.
    /// Tiles are rendered from <see cref="ItemTemplate"/> when assigned, and each tile's
    /// current item is attached as <see cref="VisualElement.dataSource"/>.
    /// </summary>
    [UxmlElement]
    public partial class MatGridList : ScrollView
    {
        /// <summary>Base CSS class applied to the grid-list root.</summary>
        public const string BaseClassName = "mat-grid-list";

        /// <summary>CSS class applied to the positioned tile container.</summary>
        public const string ContentClassName = "mat-grid-list__content";

        /// <summary>CSS class applied to each tile root.</summary>
        public const string TileClassName = "mat-grid-list__tile";

        /// <summary>CSS class applied to the cloned tile template root.</summary>
        public const string TileContentClassName = "mat-grid-list__tile-content";

        /// <summary>CSS class applied to the fallback label when no template is assigned.</summary>
        public const string TileFallbackLabelClassName = "mat-grid-list__tile-fallback-label";

        private const int OverscanLineCount = 2;

        private readonly VisualElement _gridContent;
        private readonly List<TileHandle> _tilePool = new();

        private IList _itemSource;
        private VisualTreeAsset _itemTemplate;
        private MatGridListDirection _direction = MatGridListDirection.Vertical;
        private int _trackCount = 4;
        private float _gutterSize = 8f;
        private string _tileSize = "1:1";

        /// <summary>Data source rendered into the grid tiles.</summary>
        [CreateProperty]
        public IList ItemSource
        {
            get => _itemSource;
            set
            {
                _itemSource = value;
                RefreshTileLayout();
            }
        }

        /// <summary>Template cloned for each tile.</summary>
        [UxmlAttribute]
        public VisualTreeAsset ItemTemplate
        {
            get => _itemTemplate;
            set
            {
                _itemTemplate = value;
                RebuildPool();
                RefreshTileLayout();
            }
        }

        /// <summary>
        /// Grid flow direction. In vertical mode, <see cref="TrackCount"/> means columns.
        /// In horizontal mode, <see cref="TrackCount"/> means rows.
        /// </summary>
        [UxmlAttribute, CreateProperty]
        public MatGridListDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                ApplyScrollDirection();
                RefreshTileLayout();
            }
        }

        /// <summary>
        /// Shared track count. In vertical mode this means columns; in horizontal mode this means rows.
        /// </summary>
        [UxmlAttribute, CreateProperty]
        public int TrackCount
        {
            get => _trackCount;
            set
            {
                _trackCount = Math.Max(1, value);
                RefreshTileLayout();
            }
        }

        /// <summary>Spacing between tiles in pixels.</summary>
        [UxmlAttribute, CreateProperty]
        public float GutterSize
        {
            get => _gutterSize;
            set
            {
                _gutterSize = Math.Max(0f, value);
                RefreshTileLayout();
            }
        }

        /// <summary>
        /// Shared main-axis tile size expression. In vertical mode this means row height.
        /// In horizontal mode this means column width. Supports Angular-like ratios such as <c>1:1</c>
        /// and fixed pixel values such as <c>120</c>.
        /// </summary>
        [UxmlAttribute, CreateProperty]
        public string TileSize
        {
            get => _tileSize;
            set
            {
                _tileSize = string.IsNullOrWhiteSpace(value) ? "1:1" : value.Trim();
                RefreshTileLayout();
            }
        }

        /// <summary>Initializes a new Material grid list.</summary>
        public MatGridList()
        {
            AddToClassList(BaseClassName);

            _gridContent = new VisualElement();
            _gridContent.AddToClassList(ContentClassName);
            contentContainer.Add(_gridContent);

            ApplyScrollDirection();
            RegisterCallback<GeometryChangedEvent>(_ => RefreshTileLayout());
            contentViewport?.RegisterCallback<GeometryChangedEvent>(_ => RefreshTileLayout());
            verticalScroller.valueChanged += _ => RefreshTileLayout();
            horizontalScroller.valueChanged += _ => RefreshTileLayout();
        }

        private void ApplyScrollDirection()
        {
            mode = Direction == MatGridListDirection.Vertical
                ? ScrollViewMode.Vertical
                : ScrollViewMode.Horizontal;

            horizontalScrollerVisibility = Direction == MatGridListDirection.Vertical
                ? ScrollerVisibility.Hidden
                : ScrollerVisibility.Auto;

            verticalScrollerVisibility = Direction == MatGridListDirection.Vertical
                ? ScrollerVisibility.Auto
                : ScrollerVisibility.Hidden;
        }

        private void RebuildPool()
        {
            _tilePool.Clear();
            _gridContent.Clear();
        }

        private void RefreshTileLayout()
        {
            var itemCount = _itemSource?.Count ?? 0;
            var crossAxisExtent = Direction == MatGridListDirection.Vertical
                ? GetViewportWidth()
                : GetViewportHeight();

            if (crossAxisExtent <= 0f)
            {
                return;
            }

            var trackCount = Math.Max(1, TrackCount);
            var gutter = Math.Max(0f, GutterSize);
            var tileCrossSize = (crossAxisExtent - gutter * (trackCount - 1)) / trackCount;
            if (tileCrossSize < 0f)
            {
                tileCrossSize = 0f;
            }

            var tileMainSize = ResolveTileMainSize(tileCrossSize);
            var lineCount = itemCount == 0 ? 0 : (itemCount + trackCount - 1) / trackCount;
            var contentMainExtent = lineCount <= 0 ? 0f : lineCount * tileMainSize + (lineCount - 1) * gutter;

            if (Direction == MatGridListDirection.Vertical)
            {
                _gridContent.style.width = crossAxisExtent;
                _gridContent.style.height = contentMainExtent;
            }
            else
            {
                _gridContent.style.width = contentMainExtent;
                _gridContent.style.height = crossAxisExtent;
            }

            if (itemCount == 0)
            {
                EnsurePoolSize(0);
                return;
            }

            var viewportMainExtent = Direction == MatGridListDirection.Vertical
                ? GetViewportHeight()
                : GetViewportWidth();

            if (viewportMainExtent <= 0f)
            {
                return;
            }

            var lineSpan = Math.Max(1f, tileMainSize + gutter);
            var scrollMainOffset = Direction == MatGridListDirection.Vertical
                ? scrollOffset.y
                : scrollOffset.x;

            var firstVisibleLine = Math.Max(0, (int)Math.Floor(scrollMainOffset / lineSpan));
            var lastVisibleLine = Math.Max(firstVisibleLine, (int)Math.Floor(Math.Max(0f, scrollMainOffset + viewportMainExtent - 1f) / lineSpan));
            var startLine = Math.Max(0, firstVisibleLine - OverscanLineCount);
            var endLine = Math.Min(lineCount - 1, lastVisibleLine + OverscanLineCount);
            var realizedItemCount = Math.Min(itemCount - startLine * trackCount, (endLine - startLine + 1) * trackCount);

            EnsurePoolSize(Math.Max(0, realizedItemCount));

            for (var i = 0; i < _tilePool.Count; i++)
            {
                var itemIndex = startLine * trackCount + i;
                var tile = _tilePool[i];

                if (itemIndex < 0 || itemIndex >= itemCount)
                {
                    tile.Root.style.display = DisplayStyle.None;
                    tile.Root.dataSource = null;
                    if (tile.Content != null)
                    {
                        tile.Content.dataSource = null;
                    }

                    tile.BoundIndex = -1;
                    continue;
                }

                BindTile(tile, itemIndex);
                PositionTile(tile.Root, itemIndex, trackCount, gutter, tileCrossSize, tileMainSize);
                tile.Root.style.display = DisplayStyle.Flex;
            }
        }

        private void EnsurePoolSize(int size)
        {
            while (_tilePool.Count < size)
            {
                var tile = CreateTile();
                _tilePool.Add(tile);
                _gridContent.Add(tile.Root);
            }

            while (_tilePool.Count > size)
            {
                var lastIndex = _tilePool.Count - 1;
                var tile = _tilePool[lastIndex];
                tile.Root.RemoveFromHierarchy();
                _tilePool.RemoveAt(lastIndex);
            }
        }

        private TileHandle CreateTile()
        {
            var root = new VisualElement();
            root.AddToClassList(TileClassName);
            root.style.position = Position.Absolute;

            if (ItemTemplate != null)
            {
                var content = ItemTemplate.CloneTree();
                content.AddToClassList(TileContentClassName);
                root.Add(content);
                return new TileHandle(root, content, null);
            }

            var label = new Label();
            label.AddToClassList(TileFallbackLabelClassName);
            root.Add(label);
            return new TileHandle(root, null, label);
        }

        private void BindTile(TileHandle tile, int itemIndex)
        {
            var item = _itemSource?[itemIndex];
            tile.Root.dataSource = item;

            if (tile.Content != null)
            {
                tile.Content.dataSource = item;
            }

            if (tile.FallbackLabel != null)
            {
                tile.FallbackLabel.text = item?.ToString() ?? string.Empty;
            }

            tile.BoundIndex = itemIndex;
        }

        private void PositionTile(
            VisualElement tile,
            int itemIndex,
            int trackCount,
            float gutter,
            float tileCrossSize,
            float tileMainSize)
        {
            var track = itemIndex % trackCount;
            var line = itemIndex / trackCount;

            if (Direction == MatGridListDirection.Vertical)
            {
                tile.style.left = track * (tileCrossSize + gutter);
                tile.style.top = line * (tileMainSize + gutter);
                tile.style.width = tileCrossSize;
                tile.style.height = tileMainSize;
            }
            else
            {
                tile.style.left = line * (tileMainSize + gutter);
                tile.style.top = track * (tileCrossSize + gutter);
                tile.style.width = tileMainSize;
                tile.style.height = tileCrossSize;
            }
        }

        private float GetViewportWidth()
        {
            var width = contentViewport?.layout.width ?? 0f;
            return width > 0f ? width : layout.width;
        }

        private float GetViewportHeight()
        {
            var height = contentViewport?.layout.height ?? 0f;
            return height > 0f ? height : layout.height;
        }

        private float ResolveTileMainSize(float tileCrossSize)
        {
            if (TryParseRatio(TileSize, out var widthPart, out var heightPart))
            {
                return Direction == MatGridListDirection.Vertical
                    ? tileCrossSize * (heightPart / widthPart)
                    : tileCrossSize * (widthPart / heightPart);
            }

            if (float.TryParse(TileSize, NumberStyles.Float, CultureInfo.InvariantCulture, out var pixels))
            {
                return Math.Max(1f, pixels);
            }

            return tileCrossSize;
        }

        private static bool TryParseRatio(string value, out float widthPart, out float heightPart)
        {
            widthPart = 1f;
            heightPart = 1f;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var parts = value.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            return float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out widthPart)
                   && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out heightPart)
                   && widthPart > 0f
                   && heightPart > 0f;
        }

        private sealed class TileHandle
        {
            public TileHandle(VisualElement root, VisualElement content, Label fallbackLabel)
            {
                Root = root;
                Content = content;
                FallbackLabel = fallbackLabel;
                BoundIndex = -1;
            }

            public VisualElement Root { get; }

            public VisualElement Content { get; }

            public Label FallbackLabel { get; }

            public int BoundIndex { get; set; }
        }
    }
}
