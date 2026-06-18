using System;
using R3;
using Sim.Faciem.CommandBinding;
using Sim.Faciem.Shared;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// A Material-styled button implemented as a custom VisualElement so it can
    /// own its shadow, surface, ripple, and content layers explicitly.
    /// </summary>
    [UxmlElement]
    public partial class MatButton : VisualElement
    {
        public const string BaseClassName = "mat-button-base";

        public const string BasicClassName = "mat-button";
        public const string RaisedClassName = "mat-raised-button";
        public const string StrokedClassName = "mat-stroked-button";
        public const string FlatClassName = "mat-flat-button";
        public const string IconClassName = "mat-icon-button";
        public const string FabClassName = "mat-fab";
        public const string MiniFabClassName = "mat-mini-fab";

        public const string PrimaryClassName = "mat-primary";
        public const string AccentClassName = "mat-accent";
        public const string WarnClassName = "mat-warn";

        public const string ShadowClassName = "mat-button__shadow";
        public const string SurfaceClassName = "mat-button__surface";
        public const string RippleClassName = "mat-button__ripple";
        public const string ContentClassName = "mat-button__content";
        public const string LabelClassName = "mat-button__label";
        public const string FocusVisibleClassName = "mat-focus-visible";

        private static readonly string[] AllVariantClasses =
        {
            BasicClassName, RaisedClassName, StrokedClassName, FlatClassName,
            IconClassName, FabClassName, MiniFabClassName,
        };

        private static readonly string[] AllColorClasses =
        {
            PrimaryClassName, AccentClassName, WarnClassName,
        };

        private static readonly CustomStyleProperty<float> ShadowOuterOffsetXProperty = new("--mat-button-shadow-outer-offset-x");
        private static readonly CustomStyleProperty<float> ShadowOuterOffsetYProperty = new("--mat-button-shadow-outer-offset-y");
        private static readonly CustomStyleProperty<float> ShadowOuterWidthDeltaProperty = new("--mat-button-shadow-outer-width-delta");
        private static readonly CustomStyleProperty<float> ShadowOuterHeightDeltaProperty = new("--mat-button-shadow-outer-height-delta");
        private static readonly CustomStyleProperty<float> ShadowOuterRadiusProperty = new("--mat-button-shadow-outer-radius");
        private static readonly CustomStyleProperty<Color> ShadowOuterColorProperty = new("--mat-button-shadow-outer-color");
        private static readonly CustomStyleProperty<float> ShadowInnerOffsetXProperty = new("--mat-button-shadow-inner-offset-x");
        private static readonly CustomStyleProperty<float> ShadowInnerOffsetYProperty = new("--mat-button-shadow-inner-offset-y");
        private static readonly CustomStyleProperty<float> ShadowInnerWidthDeltaProperty = new("--mat-button-shadow-inner-width-delta");
        private static readonly CustomStyleProperty<float> ShadowInnerHeightDeltaProperty = new("--mat-button-shadow-inner-height-delta");
        private static readonly CustomStyleProperty<float> ShadowInnerRadiusProperty = new("--mat-button-shadow-inner-radius");
        private static readonly CustomStyleProperty<Color> ShadowInnerColorProperty = new("--mat-button-shadow-inner-color");

        private readonly MatButtonShadowLayer _shadowLayer;
        private readonly VisualElement _surfaceLayer;
        private readonly MatRippleHost _rippleLayer;
        private readonly VisualElement _contentLayer;
        private readonly Label _label;
        private readonly Clickable _clickable;

        private SerializedCommand _command;
        private DisposableBag _commandSubscriptions;
        private string _text = string.Empty;
        private MatButtonVariant _variant = MatButtonVariant.Basic;
        private MatButtonColor _themeColor = MatButtonColor.Default;
        private bool _disableRipple;
        private bool _suppressFocusVisibleOnce;

        private float _shadowOuterOffsetX;
        private float _shadowOuterOffsetY;
        private float _shadowOuterWidthDelta;
        private float _shadowOuterHeightDelta;
        private float _shadowOuterRadius;
        private Color _shadowOuterColor = new(0f, 0f, 0f, 0f);
        private float _shadowInnerOffsetX;
        private float _shadowInnerOffsetY;
        private float _shadowInnerWidthDelta;
        private float _shadowInnerHeightDelta;
        private float _shadowInnerRadius;
        private Color _shadowInnerColor = new(0f, 0f, 0f, 0f);
        private bool _hasShadowStyle;

        public override VisualElement contentContainer => _contentLayer;

        /// <summary>Text shown by the internal label, mirroring Button.text.</summary>
        [UxmlAttribute]
        [CreateProperty]
        public string text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                _label.text = _text;
                _label.style.display = string.IsNullOrEmpty(_text)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
        }

        /// <summary>Bindable command mirroring BindableButton.Command behavior.</summary>
        [UxmlAttribute]
        [CreateProperty]
        public SerializedCommand Command
        {
            get => _command;
            set
            {
                _command = value;
                RegisterCommandCallbacks();
            }
        }

        [UxmlAttribute]
        [CreateProperty]
        public MatButtonVariant Variant
        {
            get => _variant;
            set
            {
                _variant = value;
                foreach (var cls in AllVariantClasses)
                    RemoveFromClassList(cls);
                AddToClassList(GetVariantClassName(value));
                _rippleLayer.MarkDirtyRepaint();
                _shadowLayer.MarkDirtyRepaint();
            }
        }

        [UxmlAttribute]
        [CreateProperty]
        public MatButtonColor ThemeColor
        {
            get => _themeColor;
            set
            {
                _themeColor = value;
                foreach (var cls in AllColorClasses)
                    RemoveFromClassList(cls);
                if (value != MatButtonColor.Default)
                    AddToClassList(GetColorClassName(value));
            }
        }

        [UxmlAttribute]
        [CreateProperty]
        public bool DisableRipple
        {
            get => _disableRipple;
            set
            {
                _disableRipple = value;
            }
        }

        public float ShadowOuterOffsetX => _shadowOuterOffsetX;
        public float ShadowOuterOffsetY => _shadowOuterOffsetY;
        public float ShadowOuterWidthDelta => _shadowOuterWidthDelta;
        public float ShadowOuterHeightDelta => _shadowOuterHeightDelta;
        public float ShadowOuterRadius => _shadowOuterRadius;
        public Color ShadowOuterColor => _shadowOuterColor;
        public float ShadowInnerOffsetX => _shadowInnerOffsetX;
        public float ShadowInnerOffsetY => _shadowInnerOffsetY;
        public float ShadowInnerWidthDelta => _shadowInnerWidthDelta;
        public float ShadowInnerHeightDelta => _shadowInnerHeightDelta;
        public float ShadowInnerRadius => _shadowInnerRadius;
        public Color ShadowInnerColor => _shadowInnerColor;
        public bool HasShadowStyle => _hasShadowStyle;

        public event Action Clicked;

        public MatButton()
        {
            _commandSubscriptions = new DisposableBag();
            RegisterCallback<DetachFromPanelEvent>(_ => _commandSubscriptions.Dispose());

            focusable = true;
            tabIndex = 0;

            AddToClassList(BaseClassName);

            _shadowLayer = new MatButtonShadowLayer(this);
            _shadowLayer.AddToClassList(ShadowClassName);
            _shadowLayer.pickingMode = PickingMode.Ignore;
            hierarchy.Add(_shadowLayer);

            _surfaceLayer = new VisualElement();
            _surfaceLayer.AddToClassList(SurfaceClassName);
            hierarchy.Add(_surfaceLayer);

            _rippleLayer = new MatRippleHost(this)
            {
                DisableRippleEvaluator = () => DisableRipple,
                CornerRadiusProvider = rect => Variant switch
                {
                    MatButtonVariant.Icon => Mathf.Min(rect.width, rect.height) * 0.5f,
                    MatButtonVariant.Fab => Mathf.Min(rect.width, rect.height) * 0.5f,
                    MatButtonVariant.MiniFab => Mathf.Min(rect.width, rect.height) * 0.5f,
                    _ => 4f,
                },
            };
            _rippleLayer.AddToClassList(RippleClassName);
            _surfaceLayer.Add(_rippleLayer);

            _contentLayer = new VisualElement();
            _contentLayer.AddToClassList(ContentClassName);
            _surfaceLayer.Add(_contentLayer);

            _label = new Label();
            _label.AddToClassList(LabelClassName);
            _contentLayer.Add(_label);

            _clickable = new Clickable(Invoke);
            this.AddManipulator(_clickable);

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<PointerDownEvent>(OnPointerDownForFocusVisible, TrickleDown.TrickleDown);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            text = string.Empty;
            Variant = MatButtonVariant.Basic;
            ThemeColor = MatButtonColor.Default;
            DisableRipple = false;
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!enabledInHierarchy)
            {
                return;
            }

            _suppressFocusVisibleOnce = false;
            AddToClassList(FocusVisibleClassName);

            if (evt.keyCode != KeyCode.Return
                && evt.keyCode != KeyCode.KeypadEnter
                && evt.keyCode != KeyCode.Space)
            {
                return;
            }

            evt.StopPropagation();
            Invoke();
        }

        private void OnPointerDownForFocusVisible(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            _suppressFocusVisibleOnce = true;
            RemoveFromClassList(FocusVisibleClassName);
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            if (_suppressFocusVisibleOnce)
            {
                _suppressFocusVisibleOnce = false;
                RemoveFromClassList(FocusVisibleClassName);
                return;
            }

            AddToClassList(FocusVisibleClassName);
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            _suppressFocusVisibleOnce = false;
            RemoveFromClassList(FocusVisibleClassName);
        }

        private void Invoke()
        {
            if (!enabledInHierarchy)
            {
                return;
            }

            Clicked?.Invoke();
            _command?.Command?.Execute(Unit.Default);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _hasShadowStyle = false;

            TryAssignShadow(evt, ShadowOuterOffsetXProperty, ref _shadowOuterOffsetX, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowOuterOffsetYProperty, ref _shadowOuterOffsetY, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowOuterWidthDeltaProperty, ref _shadowOuterWidthDelta, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowOuterHeightDeltaProperty, ref _shadowOuterHeightDelta, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowOuterRadiusProperty, ref _shadowOuterRadius, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowOuterColorProperty, ref _shadowOuterColor, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowInnerOffsetXProperty, ref _shadowInnerOffsetX, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowInnerOffsetYProperty, ref _shadowInnerOffsetY, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowInnerWidthDeltaProperty, ref _shadowInnerWidthDelta, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowInnerHeightDeltaProperty, ref _shadowInnerHeightDelta, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowInnerRadiusProperty, ref _shadowInnerRadius, ref _hasShadowStyle);
            TryAssignShadow(evt, ShadowInnerColorProperty, ref _shadowInnerColor, ref _hasShadowStyle);

            _shadowLayer.MarkDirtyRepaint();
        }

        private static void TryAssignShadow(CustomStyleResolvedEvent evt, CustomStyleProperty<float> property, ref float field, ref bool hasShadowStyle)
        {
            if (evt.customStyle.TryGetValue(property, out var value))
            {
                field = value;
                hasShadowStyle = true;
            }
        }

        private static void TryAssignShadow(CustomStyleResolvedEvent evt, CustomStyleProperty<Color> property, ref Color field, ref bool hasShadowStyle)
        {
            if (evt.customStyle.TryGetValue(property, out var value))
            {
                field = value;
                hasShadowStyle = true;
            }
        }

        private void RegisterCommandCallbacks()
        {
            _commandSubscriptions.Dispose();
            _commandSubscriptions = new DisposableBag();

            if (_command?.Command == null)
            {
                return;
            }

            _commandSubscriptions.Add(
                _command.Command.CanExecuteObs
                    .Prepend(_command.Command.CanExecute)
                    .Subscribe(SetEnabled));

            _commandSubscriptions.Add(
                _command.Command.IsVisibleObs
                    .Subscribe(isVisible =>
                    {
                        style.display = isVisible
                            ? DisplayStyle.Flex
                            : DisplayStyle.None;
                    }));
        }

        private static string GetVariantClassName(MatButtonVariant variant) => variant switch
        {
            MatButtonVariant.Basic => BasicClassName,
            MatButtonVariant.Raised => RaisedClassName,
            MatButtonVariant.Stroked => StrokedClassName,
            MatButtonVariant.Flat => FlatClassName,
            MatButtonVariant.Icon => IconClassName,
            MatButtonVariant.Fab => FabClassName,
            MatButtonVariant.MiniFab => MiniFabClassName,
            _ => BasicClassName,
        };

        private static string GetColorClassName(MatButtonColor color) => color switch
        {
            MatButtonColor.Primary => PrimaryClassName,
            MatButtonColor.Accent => AccentClassName,
            MatButtonColor.Warn => WarnClassName,
            _ => string.Empty,
        };
    }

    internal sealed class MatButtonShadowLayer : VisualElement
    {
        private readonly MatButton _owner;

        public MatButtonShadowLayer(MatButton owner)
        {
            _owner = owner;
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
        }
    }
}
