using System;
using System.Collections.Generic;
using System.Linq;
using Sim.Faciem.Material.Icons;
using Sim.Faciem.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// A popup menu control inspired by Angular Material's <c>mat-menu</c>.
    /// Uses a trigger button plus an overlay-attached popup panel that inherits
    /// the current Material theme, similar to <see cref="MatSelect"/>.
    /// </summary>
    [UxmlElement]
    public partial class MatMenu : VisualElement
    {
        public const string BaseClassName = "mat-menu";
        public const string OpenClassName = "mat-menu--open";
        public const string DisabledClassName = "mat-menu--disabled";
        public const string IconOnlyClassName = "mat-menu--icon-only";
        public const string TriggerClassName = "mat-menu__trigger";
        public const string TriggerIconClassName = "mat-menu__trigger-icon";
        public const string TriggerTextClassName = "mat-menu__trigger-text";
        public const string ArrowClassName = "mat-menu__trigger-arrow";
        public const string PanelClassName = "mat-menu__panel";
        public const string ItemClassName = "mat-menu-item";
        public const string ItemActiveClassName = "mat-menu-item--active";
        public const string ItemPressedClassName = "mat-menu-item--pressed";
        public const string ItemDisabledClassName = "mat-menu-item--disabled";
        public const string ItemRippleClassName = "mat-menu-item__ripple";
        public const string ItemIconClassName = "mat-menu-item__icon";
        public const string ItemTextClassName = "mat-menu-item__text";

        private const float ItemRowHeight = 48f;
        private const float PanelMaxHeight = 320f;

        private readonly VisualElement _trigger;
        private readonly VisualElement _triggerIcon;
        private readonly Label _triggerLabel;
        private readonly Label _triggerArrow;
        private readonly VisualElement _itemsContainer;
        private readonly Dictionary<MatMenuItem, Action> _itemChangedHandlers = new();

        private VisualElement _overlayRoot;
        private ScrollView _panel;
        private bool _isOpen;
        private string _text = "Open menu";
        private bool _disabled;
        private IconCollection _triggerIconCollection;
        private string _triggerIconName = string.Empty;

        public override VisualElement contentContainer => _itemsContainer;

        [UxmlAttribute]
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                UpdateTriggerVisualState();
            }
        }

        [UxmlAttribute]
        public bool Disabled
        {
            get => _disabled;
            set
            {
                _disabled = value;
                EnableInClassList(DisabledClassName, value);
                _trigger.SetEnabled(!value);
                if (value)
                {
                    ClosePanel();
                }
            }
        }

        [UxmlAttribute]
        public IconCollection TriggerIconCollection
        {
            get => _triggerIconCollection;
            set
            {
                _triggerIconCollection = value;
                UpdateTriggerVisualState();
            }
        }

        [UxmlAttribute]
        public string TriggerIconName
        {
            get => _triggerIconName;
            set
            {
                _triggerIconName = value ?? string.Empty;
                UpdateTriggerVisualState();
            }
        }

        [UxmlAttribute]
        public string OverlayContainerId { get; set; }

        public MatMenu()
        {
            AddToClassList(BaseClassName);

            _trigger = new VisualElement();
            _trigger.AddToClassList(TriggerClassName);
            _trigger.focusable = true;
            _trigger.RegisterCallback<PointerDownEvent>(OnTriggerPointerDown);
            hierarchy.Add(_trigger);

            _triggerIcon = new VisualElement();
            _triggerIcon.AddToClassList(TriggerIconClassName);
            _trigger.Add(_triggerIcon);

            _triggerLabel = new Label();
            _triggerLabel.AddToClassList(TriggerTextClassName);
            _trigger.Add(_triggerLabel);

            _triggerArrow = new Label("▾");
            _triggerArrow.AddToClassList(ArrowClassName);
            _trigger.Add(_triggerArrow);

            _itemsContainer = new VisualElement();
            _itemsContainer.style.display = DisplayStyle.None;
            _itemsContainer.pickingMode = PickingMode.Ignore;
            hierarchy.Add(_itemsContainer);

            UpdateTriggerVisualState();

            RegisterCallback<AttachToPanelEvent>(SetupOverlayPanel);
            RegisterCallback<DetachFromPanelEvent>(CleanupOverlayPanel);
            RegisterCallback<GeometryChangedEvent>(_ => UpdatePanelPosition());
        }

        private void SetupOverlayPanel(AttachToPanelEvent evt)
        {
            #if UNITY_6000_3_OR_NEWER

            bool isWorldSpace = evt.destinationPanel is IRuntimePanel runtimePanel
                && runtimePanel.panelSettings.renderMode == PanelRenderMode.WorldSpace;

            if (isWorldSpace)
            {
                _overlayRoot = this.FindPanelRootChild();
            }
            else
            {
                _overlayRoot = string.IsNullOrEmpty(OverlayContainerId)
                    ? FindThemedOverlayRoot()
                    : panel?.visualTree.Q(OverlayContainerId);
            }

            #else

            _overlayRoot = string.IsNullOrEmpty(OverlayContainerId)
                ? FindThemedOverlayRoot()
                : panel?.visualTree.Q(OverlayContainerId);
            #endif

            if (_overlayRoot == null)
            {
                return;
            }

            if (_panel == null)
            {
                _panel = new ScrollView(ScrollViewMode.Vertical);
                _panel.AddToClassList(PanelClassName);
                _panel.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                _panel.verticalScrollerVisibility = ScrollerVisibility.Auto;
                _panel.style.display = DisplayStyle.None;
                _panel.style.position = Position.Absolute;
            }

            if (_panel.parent != _overlayRoot)
            {
                _overlayRoot.Add(_panel);
            }

            panel?.visualTree.RegisterCallback<PointerDownEvent>(OnGlobalPointerDown);
            RebuildPanelItems();
            DiscoverIconsAsync();
        }

        private void CleanupOverlayPanel(DetachFromPanelEvent evt)
        {
            if (evt.originPanel?.visualTree != null)
            {
                evt.originPanel.visualTree.UnregisterCallback<PointerDownEvent>(OnGlobalPointerDown);
            }

            UnsubscribeItemChanges();

            if (_panel?.parent != null)
            {
                _panel.RemoveFromHierarchy();
            }

            _overlayRoot = null;
            _panel = null;
            _isOpen = false;
            RemoveFromClassList(OpenClassName);
        }

        private VisualElement FindThemedOverlayRoot()
        {
            var vt = panel?.visualTree;
            if (vt == null)
            {
                return null;
            }

            var themedRoot = vt.Query<VisualElement>()
                .ToList()
                .FirstOrDefault(e => e.styleSheets.count > 0);

            if (themedRoot != null)
            {
                return themedRoot;
            }

            return vt.childCount > 0 ? vt[0] : vt;
        }

        private void OnTriggerPointerDown(PointerDownEvent evt)
        {
            if (_disabled)
            {
                return;
            }

            evt.StopPropagation();
            _trigger.Focus();

            if (_isOpen)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }

        private void OnGlobalPointerDown(PointerDownEvent evt)
        {
            if (!_isOpen)
            {
                return;
            }

            var target = evt.target as VisualElement;
            if (target == null)
            {
                ClosePanel();
                return;
            }

            if (IsSelfOrDescendant(target, this) || IsSelfOrDescendant(target, _panel))
            {
                return;
            }

            ClosePanel();
        }

        private void OpenPanel()
        {
            if (_panel == null)
            {
                return;
            }

            _isOpen = true;
            AddToClassList(OpenClassName);
            RebuildPanelItems();
            _panel.style.display = DisplayStyle.Flex;
            UpdatePanelPosition();
        }

        private void ClosePanel()
        {
            if (_panel == null || !_isOpen)
            {
                return;
            }

            _isOpen = false;
            RemoveFromClassList(OpenClassName);
            _panel.style.display = DisplayStyle.None;
        }

        private void RebuildPanelItems()
        {
            if (_panel == null)
            {
                return;
            }

            UnsubscribeItemChanges();
            _panel.contentContainer.Clear();

            foreach (var item in this.Query<MatMenuItem>().ToList())
            {
                SubscribeItemChanges(item);
                if (!item.IsVisible)
                {
                    continue;
                }

                _panel.contentContainer.Add(BuildItemRow(item));
            }
        }

        private VisualElement BuildItemRow(MatMenuItem item)
        {
            var row = new VisualElement();
            row.AddToClassList(ItemClassName);

            if (item.IsEffectivelyDisabled)
            {
                row.AddToClassList(ItemDisabledClassName);
            }
            else
            {
                var ripple = new MatRippleHost(row);
                ripple.AddToClassList(ItemRippleClassName);
                ripple.CornerRadiusProvider = rect => 0f;
                row.Add(ripple);
            }

            var icon = new VisualElement();
            icon.AddToClassList(ItemIconClassName);
            SetIconVisual(icon, ResolveIcon(item.IconCollection, item.IconName));
            row.Add(icon);

            var text = new Label(item.Label);
            text.AddToClassList(ItemTextClassName);
            row.Add(text);

            if (!item.IsEffectivelyDisabled)
            {
                row.RegisterCallback<PointerEnterEvent>(_ => row.AddToClassList(ItemActiveClassName));
                row.RegisterCallback<PointerLeaveEvent>(_ =>
                {
                    row.RemoveFromClassList(ItemActiveClassName);
                    row.RemoveFromClassList(ItemPressedClassName);
                });
                row.RegisterCallback<PointerDownEvent>(_ => row.AddToClassList(ItemPressedClassName));
                row.RegisterCallback<PointerUpEvent>(evt =>
                {
                    evt.StopPropagation();
                    row.RemoveFromClassList(ItemPressedClassName);
                    item.Execute();
                    ClosePanel();
                });
            }

            return row;
        }

        private void UpdatePanelPosition()
        {
            if (_panel == null || !_isOpen || _overlayRoot == null)
            {
                return;
            }

            var triggerWorld = _trigger.worldBound;
            var overlayWorld = _overlayRoot.worldBound;
            var visibleItemCount = this.Query<MatMenuItem>().ToList().Count(item => item.IsVisible);
            var contentHeight = visibleItemCount * ItemRowHeight;
            var panelHeight = Mathf.Min(PanelMaxHeight, contentHeight);
            var panelWidth = Mathf.Max(triggerWorld.width, _trigger.resolvedStyle.width);
            var requiresScroll = contentHeight > panelHeight;

            var availableBelow = overlayWorld.yMax - triggerWorld.yMax;
            var opensUpward = panelHeight > availableBelow && triggerWorld.yMin - overlayWorld.yMin >= availableBelow;
            var topWorld = opensUpward ? triggerWorld.yMin - panelHeight : triggerWorld.yMax;
            var leftWorld = triggerWorld.xMin;

            var minLeft = overlayWorld.xMin;
            var maxLeft = Mathf.Max(minLeft, overlayWorld.xMax - panelWidth);
            var minTop = overlayWorld.yMin;
            var maxTop = Mathf.Max(minTop, overlayWorld.yMax - panelHeight);

            leftWorld = Mathf.Clamp(leftWorld, minLeft, maxLeft);
            topWorld = Mathf.Clamp(topWorld, minTop, maxTop);

            var localTopLeft = _overlayRoot.WorldToLocal(new Vector2(leftWorld, topWorld));
            _panel.style.left = localTopLeft.x;
            _panel.style.top = localTopLeft.y;
            _panel.style.width = panelWidth;
            if (panelHeight <= 0f)
            {
                _panel.style.height = StyleKeyword.Auto;
            }
            else
            {
                _panel.style.height = panelHeight;
            }

            _panel.verticalScrollerVisibility = requiresScroll ? ScrollerVisibility.Auto : ScrollerVisibility.Hidden;
        }

        private void SubscribeItemChanges(MatMenuItem item)
        {
            void Handler()
            {
                RebuildPanelItems();
                UpdateTriggerVisualState();
            }

            item.Changed += Handler;
            _itemChangedHandlers[item] = Handler;
        }

        private void UnsubscribeItemChanges()
        {
            foreach (var pair in _itemChangedHandlers)
                pair.Key.Changed -= pair.Value;

            _itemChangedHandlers.Clear();
        }

        private void UpdateTriggerVisualState()
        {
            var icon = ResolveIcon(_triggerIconCollection, _triggerIconName);
            var hasIcon = icon != null;
            var hasText = !string.IsNullOrWhiteSpace(_text);
            var iconOnly = hasIcon && !hasText;

            _triggerLabel.text = _text;
            _triggerLabel.style.display = hasText ? DisplayStyle.Flex : DisplayStyle.None;
            _triggerArrow.style.display = iconOnly ? DisplayStyle.None : DisplayStyle.Flex;
            EnableInClassList(IconOnlyClassName, iconOnly);

            SetIconVisual(_triggerIcon, icon);
        }

        private static void SetIconVisual(VisualElement element, VectorImage icon)
        {
            if (element == null)
            {
                return;
            }

            if (icon == null)
            {
                element.style.display = DisplayStyle.None;
                element.style.backgroundImage = StyleKeyword.None;
                return;
            }

            element.style.display = DisplayStyle.Flex;
            element.style.backgroundImage = new StyleBackground(icon);
        }

        private static VectorImage ResolveIcon(IconCollection collection, string iconName)
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

        private async void DiscoverIconsAsync()
        {
            try
            {
                await IconCollectionRegistry.DiscoverCollectionsAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"MatMenu icon discovery failed: {ex.Message}");
            }

            if (panel == null)
            {
                return;
            }

            UpdateTriggerVisualState();
            RebuildPanelItems();
            UpdatePanelPosition();
        }

        private static bool IsSelfOrDescendant(VisualElement target, VisualElement ancestor)
        {
            if (target == null || ancestor == null)
            {
                return false;
            }

            for (var current = target; current != null; current = current.parent)
            {
                if (current == ancestor)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
