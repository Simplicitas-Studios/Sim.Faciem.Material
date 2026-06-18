using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Sim.Faciem.Controls;
using Sim.Faciem.Shared;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// A select / dropdown control that mirrors Angular Material's <c>mat-select</c>.
    /// The form-field chrome (appearance, label, hint) is provided internally by
    /// <see cref="MatFormField"/>; users only interact with this single element.
    ///
    /// Declare <see cref="Faciem.Material.Controls.MatOption"/> elements as UXML children to define options:
    /// <code>
    /// &lt;Sim.Faciem.Controls.MatSelect label="Region" placeholder="Choose a region"&gt;
    ///     &lt;Sim.Faciem.Controls.MatOption value="sinnoh"  label="Sinnoh"  /&gt;
    ///     &lt;Sim.Faciem.Controls.MatOption value="johto"   label="Johto"   /&gt;
    /// &lt;/Sim.Faciem.Controls.MatSelect&gt;
    /// </code>
    ///
    /// USS styling is loaded exclusively via PanelSettings (.tss theme files) for
    /// runtime panels, and via <see cref="Editor.MatEditorStyles"/>
    /// for editor windows.
    /// </summary>
    [UxmlElement]
    public partial class MatSelect : VisualElement
    {
        // ── CSS class constants ────────────────────────────────────────────────
        public const string BaseClassName             = "mat-select";
        public const string OpenClassName             = "mat-select--open";
        public const string DisabledClassName         = "mat-select--disabled";
        public const string MultipleClassName         = "mat-select--multiple";
        public const string TriggerClassName          = "mat-select__trigger";
        public const string ValueTextClassName        = "mat-select__value-text";
        public const string PlaceholderClassName      = "mat-select__placeholder";
        public const string ArrowWrapperClassName     = "mat-select__arrow-wrapper";
        public const string ArrowClassName            = "mat-select__arrow";
        public const string PanelClassName            = "mat-select__panel";
        public const string OptionClassName           = "mat-option";
        public const string OptionSelectedClassName   = "mat-option--selected";
        public const string OptionDisabledClassName   = "mat-option--disabled";
        public const string OptionActiveClassName     = "mat-option--active";
        public const string OptionPressedClassName    = "mat-option--pressed";
        public const string OptionRippleClassName     = "mat-option__ripple";
        public const string OptionTextClassName       = "mat-option__text";
        public const string PseudoCheckboxClassName       = "mat-pseudo-checkbox";
        public const string PseudoCheckboxCheckedClassName = "mat-pseudo-checkbox--checked";

        // ── Internal DOM ───────────────────────────────────────────────────────
        private readonly MatFormField  _formField;
        private readonly VisualElement _trigger;
        private readonly Label         _valueLabel;
        private readonly Label         _arrowLabel;
        private readonly DisposableBagHolder _disposables;

        private const float OptionRowHeight = 48f;
        private const float PanelMaxHeight  = 256f;

        private VisualElement _overlayRoot;
        private ScrollView    _panel;
        private IDisposable   _globalPointerSubscription;
        private int           _lastLocalPointerDownFrame = -1;

        // ── Backing fields ─────────────────────────────────────────────────────
        private MatFormFieldAppearance _appearance   = MatFormFieldAppearance.Fill;
        private string                 _label        = string.Empty;
        private string                 _hintText     = string.Empty;
        private string                 _placeholder  = string.Empty;
        private bool                   _multiple;
        private bool                   _disabled;
        private bool                   _required;
        private bool                   _disableRipple;
        private string                 _value;
        private List<string>           _values       = new();
        private bool                   _isOpen;

        // ── UXML attributes ────────────────────────────────────────────────────

        /// <summary>Fill or Outline chrome style, forwarded to the internal MatFormField.</summary>
        [UxmlAttribute]
        public MatFormFieldAppearance Appearance
        {
            get => _appearance;
            set
            {
                _appearance = value;
                _formField.Appearance = value;
            }
        }

        /// <summary>Floating label text shown above the field.</summary>
        [UxmlAttribute]
        public string Label
        {
            get => _label;
            set
            {
                _label = value ?? string.Empty;
                _formField.LabelText = _label;
            }
        }

        /// <summary>Hint text shown in the subscript below the field.</summary>
        [UxmlAttribute]
        public string HintText
        {
            get => _hintText;
            set
            {
                _hintText = value ?? string.Empty;
                _formField.SetHintText(_hintText);
            }
        }

        /// <summary>Grey placeholder text shown in the trigger when nothing is selected.</summary>
        [UxmlAttribute]
        public string Placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value ?? string.Empty;
                UpdateValueDisplay();
            }
        }

        /// <summary>When true multiple options can be selected simultaneously.</summary>
        [UxmlAttribute]
        public bool Multiple
        {
            get => _multiple;
            set
            {
                _multiple = value;
                EnableInClassList(MultipleClassName, value);
                _value  = null;
                _values = new List<string>();
                RebuildPanelOptions();
                UpdateValueDisplay();
            }
        }

        /// <summary>Disables the control — trigger is not clickable and colours are muted.</summary>
        [UxmlAttribute]
        public bool Disabled
        {
            get => _disabled;
            set
            {
                _disabled = value;
                EnableInClassList(DisabledClassName, value);
                _formField.SetDisabled(value);
                _trigger.SetEnabled(!value);
            }
        }

        /// <summary>Marks the field as required (visual indicator only in USS).</summary>
        [UxmlAttribute]
        public bool Required
        {
            get => _required;
            set => _required = value;
        }

        /// <summary>When true the hover/active ripple overlay is suppressed.</summary>
        [UxmlAttribute]
        public bool DisableRipple
        {
            get => _disableRipple;
            set => _disableRipple = value;
        }

        // ── Data-bindable properties ───────────────────────────────────────────

        /// <summary>Selected value in single-select mode.</summary>
        [CreateProperty]
        public string Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                UpdateValueDisplay();
                _formField.SetHasValue(!string.IsNullOrEmpty(_value));
                RefreshOptionStates();
            }
        }

        /// <summary>Selected values in multiple-select mode.</summary>
        [CreateProperty]
        public List<string> Values
        {
            get => _values;
            set
            {
                _values = value ?? new List<string>();
                UpdateValueDisplay();
                _formField.SetHasValue(_values.Count > 0);
                RefreshOptionStates();
            }
        }

        // ── Constructor ────────────────────────────────────────────────────────

        public MatSelect()
        {
            AddToClassList(BaseClassName);

            _disposables = this.RegisterDisposableBag();

            // ── Build trigger row ──────────────────────────────────────────────
            _trigger = new VisualElement();
            _trigger.AddToClassList(TriggerClassName);
            _trigger.focusable = true;

            _valueLabel = new Label();
            _valueLabel.AddToClassList(ValueTextClassName);
            _valueLabel.AddToClassList(PlaceholderClassName);
            _trigger.Add(_valueLabel);

            var arrowWrapper = new VisualElement();
            arrowWrapper.AddToClassList(ArrowWrapperClassName);
            _arrowLabel = new Label("▾");
            _arrowLabel.AddToClassList(ArrowClassName);
            arrowWrapper.Add(_arrowLabel);
            _trigger.Add(arrowWrapper);

            // ── Build form field and slot the trigger inside ───────────────────
            _formField = new MatFormField();
            _formField.Infix.Add(_trigger);

            Add(_formField);

            // ── Wire observable subscriptions ──────────────────────────────────
            _disposables.Add(this.AttachToPanelAsObservable()
                .Subscribe(SetupOverlayPanel));

            _disposables.Add(_formField.PointerDownAsObservable()
                .Subscribe(OnFormFieldPointerDown));

            _disposables.Add(_trigger.FocusInAsObservable()
                .Subscribe(_ => _formField.SetFocused(true)));

            _disposables.Add(_trigger.BlurAsObservable()
                .Subscribe(_ =>
                {
                    _formField.SetFocused(false);
                }));

            _disposables.Add(this.GeometryChangedAsObservable()
                .Subscribe(_ => UpdatePanelPosition()));

            RegisterCallback<DetachFromPanelEvent>(CleanupOverlayPanel);

            UpdateValueDisplay();
        }

        // ── Overlay setup ──────────────────────────────────────────────────────

        private void SetupOverlayPanel(AttachToPanelEvent evt)
        {
            _overlayRoot = evt.destinationPanel.IsWorldSpaceRuntimePanel()
                ? this.FindPanelRootChild()
                : FindThemedOverlayRoot();

            if (_overlayRoot == null) return;

            _panel = new ScrollView(ScrollViewMode.Vertical);
            _panel.AddToClassList(PanelClassName);
            _panel.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _panel.verticalScrollerVisibility   = ScrollerVisibility.Auto;
            _panel.style.display  = DisplayStyle.None;
            _panel.style.position = Position.Absolute;
            _overlayRoot.Add(_panel);

            panel?.visualTree.RegisterCallback<PointerDownEvent>(OnPanelPointerDownTrickle, TrickleDown.TrickleDown);
            panel?.visualTree.RegisterCallback<PointerDownEvent>(OnPanelPointerDown);
            _globalPointerSubscription?.Dispose();
            _globalPointerSubscription = GlobalPointerInputWatcher.Subscribe(OnGlobalPointerDown);
            RebuildPanelOptions();
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
            if (vt == null) return null;

            // Pick the first visual element that has stylesheets attached.
            // This is usually rootVisualElement and ensures our panel inherits
            // the material theme variables and class rules.
            var themedRoot = vt.Query<VisualElement>()
                .ToList()
                .FirstOrDefault(e => e.styleSheets.count > 0);

            if (themedRoot != null)
                return themedRoot;

            // Fallback for edge cases where styles are injected differently.
            return vt.childCount > 0 ? vt[0] : vt;
        }

        // ── Option rendering ───────────────────────────────────────────────────

        private void RebuildPanelOptions()
        {
            if (_panel == null) return;
            _panel.contentContainer.Clear();

            foreach (var opt in this.Query<Material.Controls.MatOption>().ToList())
                _panel.contentContainer.Add(BuildOptionRow(opt));
        }

        private VisualElement BuildOptionRow(Material.Controls.MatOption opt)
        {
            var row = new VisualElement();
            row.AddToClassList(OptionClassName);

            if (opt.Disabled)
                row.AddToClassList(OptionDisabledClassName);
            else
            {
                var ripple = new MatRippleHost(row)
                {
                    DisableRippleEvaluator = () => DisableRipple,
                    CornerRadiusProvider = _ => 0f,
                };
                ripple.AddToClassList(OptionRippleClassName);
                row.Add(ripple);
            }

            // Pseudo-checkbox visible in multiple mode
            if (_multiple)
            {
                var checkbox = new VisualElement();
                checkbox.AddToClassList(PseudoCheckboxClassName);
                row.Add(checkbox);
            }

            var text = new Label(string.IsNullOrEmpty(opt.Label) ? opt.Value : opt.Label);
            text.AddToClassList(OptionTextClassName);
            row.Add(text);

            if (!opt.Disabled)
            {
                row.RegisterCallback<PointerDownEvent>(_ => row.AddToClassList(OptionPressedClassName));
                row.RegisterCallback<PointerUpEvent>(evt =>
                {
                    evt.StopPropagation();
                    row.RemoveFromClassList(OptionPressedClassName);
                    SelectOption(opt);
                });
                row.RegisterCallback<PointerEnterEvent>(_ =>
                    row.AddToClassList(OptionActiveClassName));
                row.RegisterCallback<PointerLeaveEvent>(_ =>
                {
                    row.RemoveFromClassList(OptionActiveClassName);
                    row.RemoveFromClassList(OptionPressedClassName);
                });
            }

            ApplyOptionSelectedState(row, opt);
            return row;
        }

        private void SelectOption(Material.Controls.MatOption opt)
        {
            if (_multiple)
            {
                var list = new List<string>(_values);
                if (list.Contains(opt.Value))
                    list.Remove(opt.Value);
                else
                    list.Add(opt.Value);
                Values = list;
            }
            else
            {
                Value = opt.Value;
                ClosePanel();
            }
        }

        private void RefreshOptionStates()
        {
            if (_panel == null) return;

            var options = this.Query<Material.Controls.MatOption>().ToList();
            var rows    = _panel.contentContainer.Query<VisualElement>(className: OptionClassName).ToList();

            for (var i = 0; i < Mathf.Min(options.Count, rows.Count); i++)
                ApplyOptionSelectedState(rows[i], options[i]);
        }

        private void ApplyOptionSelectedState(VisualElement row, Material.Controls.MatOption opt)
        {
            var selected = _multiple
                ? _values.Contains(opt.Value)
                : _value == opt.Value;

            row.EnableInClassList(OptionSelectedClassName, selected);

            var checkbox = row.Q(className: PseudoCheckboxClassName);
            checkbox?.EnableInClassList(PseudoCheckboxCheckedClassName, selected);
        }

        // ── Panel open / close ─────────────────────────────────────────────────

        private void OnFormFieldPointerDown(PointerDownEvent evt)
        {
            if (_disabled) return;

            // Keep the hint/description row non-interactive for panel toggling.
            var target = evt.target as VisualElement;
            while (target != null)
            {
                if (target.ClassListContains(MatFormField.SubscriptClassName))
                    return;

                if (target == _formField)
                    break;

                target = target.parent;
            }

            evt.StopPropagation();
            _trigger.Focus();
            if (_isOpen) ClosePanel(); else OpenPanel();
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
            if (_panel == null) return;
            _isOpen = true;
            AddToClassList(OpenClassName);
            RebuildPanelOptions();
            _panel.style.display = DisplayStyle.Flex;
            UpdatePanelPosition();
        }

        private void ClosePanel()
        {
            if (_panel == null || !_isOpen) return;
            _isOpen = false;
            RemoveFromClassList(OpenClassName);
            _panel.style.display = DisplayStyle.None;
        }

        private static bool IsSelfOrDescendant(VisualElement target, VisualElement root)
        {
            if (target == null || root == null)
            {
                return false;
            }

            for (var current = target; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdatePanelPosition()
        {
            if (_panel == null || !_isOpen || _overlayRoot == null) return;

            var fieldRect = _formField.GetLocalRectIn(_overlayRoot);
            var overlayRect = new Rect(Vector2.zero, _overlayRoot.layout.size);

            var panelWidth  = Mathf.Max(fieldRect.width, _formField.layout.width, _formField.resolvedStyle.width);
            var optionCount = this.Query<Material.Controls.MatOption>().ToList().Count;
            var contentHeight = optionCount * OptionRowHeight;
            var panelHeight   = Mathf.Min(PanelMaxHeight, contentHeight);
            var requiresScroll = contentHeight > panelHeight;

            var availableBelow = overlayRect.yMax - fieldRect.yMax;
            var top = panelHeight > availableBelow
                ? fieldRect.yMin - panelHeight
                : fieldRect.yMax;

            var left = fieldRect.xMin;

            var minLeft = overlayRect.xMin;
            var maxLeft = Mathf.Max(minLeft, overlayRect.xMax - panelWidth);
            var minTop  = overlayRect.yMin;
            var maxTop  = Mathf.Max(minTop, overlayRect.yMax - panelHeight);

            left = Mathf.Clamp(left, minLeft, maxLeft);
            top  = Mathf.Clamp(top, minTop, maxTop);

            _panel.style.left  = left;
            _panel.style.top   = top;
            _panel.style.width = panelWidth;
            _panel.style.height = panelHeight;
            _panel.verticalScrollerVisibility = requiresScroll
                ? ScrollerVisibility.Auto
                : ScrollerVisibility.Hidden;
        }

        // ── Value display ──────────────────────────────────────────────────────

        private void UpdateValueDisplay()
        {
            if (_multiple)
            {
                var hasValue = _values.Count > 0;
                _valueLabel.text = hasValue
                    ? string.Join(", ", _values.Select(GetDisplayLabel))
                    : _placeholder;
                _valueLabel.EnableInClassList(PlaceholderClassName, !hasValue);
            }
            else
            {
                var hasValue = !string.IsNullOrEmpty(_value);
                _valueLabel.text = hasValue
                    ? GetDisplayLabel(_value)
                    : _placeholder;
                _valueLabel.EnableInClassList(PlaceholderClassName, !hasValue);
            }
        }

        private string GetDisplayLabel(string val)
        {
            var opt = this.Query<Material.Controls.MatOption>().ToList()
                .FirstOrDefault(o => o.Value == val);
            return opt != null && !string.IsNullOrEmpty(opt.Label)
                ? opt.Label
                : val;
        }
    }
}

