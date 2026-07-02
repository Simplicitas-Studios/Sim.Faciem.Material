using System;
using System.Collections.Generic;
using System.Linq;
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
        private IDisposable _globalPointerSubscription;
        private bool _isOpen;
        private int _lastLocalPointerDownFrame = -1;
        private string _text = "Open menu";
        private bool _disabled;
        private Background _triggerIconBackground;

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
        public Background TriggerIcon
        {
            get => _triggerIconBackground;
            set
            {
                _triggerIconBackground = value;
                UpdateTriggerVisualState();
            }
        }

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
            _overlayRoot = evt.destinationPanel.IsWorldSpaceRuntimePanel()
                ? this.FindPanelRootChild()
                : FindThemedOverlayRoot();

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

            panel?.visualTree.RegisterCallback<PointerDownEvent>(OnPanelPointerDownTrickle, TrickleDown.TrickleDown);
            panel?.visualTree.RegisterCallback<PointerDownEvent>(OnPanelPointerDown);
            _globalPointerSubscription?.Dispose();
            _globalPointerSubscription = GlobalPointerInputWatcher.Subscribe(OnGlobalPointerDown);
            RebuildPanelItems();
        }

        private void CleanupOverlayPanel(DetachFromPanelEvent evt)
        {
            if (evt.originPanel?.visualTree != null)
            {
                evt.originPanel.visualTree.UnregisterCallback<PointerDownEvent>(OnPanelPointerDownTrickle, TrickleDown.TrickleDown);
                evt.originPanel.visualTree.UnregisterCallback<PointerDownEvent>(OnPanelPointerDown);
            }

            _globalPointerSubscription?.Dispose();
            _globalPointerSubscription = null;
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

        private void OnPanelPointerDownTrickle(PointerDownEvent evt)
        {
            if (!_isOpen)
            {
                return;
            }

            if (evt.target is VisualElement target
                && (IsSelfOrDescendant(target, this) || IsSelfOrDescendant(target, _panel)))
            {
                _lastLocalPointerDownFrame = Time.frameCount;
            }
        }

        private void OnPanelPointerDown(PointerDownEvent evt)
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

        private void OnGlobalPointerDown(int frameCount)
        {
            if (!_isOpen || _lastLocalPointerDownFrame == frameCount)
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
            _lastLocalPointerDownFrame = Time.frameCount;
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
            SetIconVisual(icon, item.Icon);
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

            var triggerRect = _trigger.GetLocalRectIn(_overlayRoot);
            var overlayRect = new Rect(Vector2.zero, _overlayRoot.layout.size);
            var visibleItemCount = this.Query<MatMenuItem>().ToList().Count(item => item.IsVisible);
            var contentHeight = visibleItemCount * ItemRowHeight;
            var panelHeight = Mathf.Min(PanelMaxHeight, contentHeight);
            var panelWidth = Mathf.Max(triggerRect.width, _trigger.layout.width, _trigger.resolvedStyle.width);
            var requiresScroll = contentHeight > panelHeight;

            var availableBelow = overlayRect.yMax - triggerRect.yMax;
            var availableAbove = triggerRect.yMin - overlayRect.yMin;
            var opensUpward = panelHeight > availableBelow && availableAbove >= availableBelow;
            var top = opensUpward ? triggerRect.yMin - panelHeight : triggerRect.yMax;
            var left = triggerRect.xMin;

            var minLeft = overlayRect.xMin;
            var maxLeft = Mathf.Max(minLeft, overlayRect.xMax - panelWidth);
            var minTop = overlayRect.yMin;
            var maxTop = Mathf.Max(minTop, overlayRect.yMax - panelHeight);

            left = Mathf.Clamp(left, minLeft, maxLeft);
            top = Mathf.Clamp(top, minTop, maxTop);

            _panel.style.left = left;
            _panel.style.top = top;
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
            var icon = TriggerIcon;
            var hasIcon = icon != null;
            var hasText = !string.IsNullOrWhiteSpace(_text);
            var iconOnly = hasIcon && !hasText;

            _triggerLabel.text = _text;
            _triggerLabel.style.display = hasText ? DisplayStyle.Flex : DisplayStyle.None;
            _triggerArrow.style.display = iconOnly ? DisplayStyle.None : DisplayStyle.Flex;
            EnableInClassList(IconOnlyClassName, iconOnly);

            SetIconVisual(_triggerIcon, icon);
        }

        private static void SetIconVisual(VisualElement element, Background icon)
        {
            if (element == null)
            {
                return;
            }

            if (!HasBackground(icon))
            {
                element.style.display = DisplayStyle.None;
                element.style.backgroundImage = StyleKeyword.None;
                return;
            }

            element.style.display = DisplayStyle.Flex;
            element.style.backgroundImage = new StyleBackground(icon);
        }

        private static bool HasBackground(Background background)
        {
            return background.texture != null
                || background.sprite != null
                || background.renderTexture != null
                || background.vectorImage != null;
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
