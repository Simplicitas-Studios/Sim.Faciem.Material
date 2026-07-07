using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// Material-styled slide toggle with Unity UI Toolkit field semantics.
    /// </summary>
    [UxmlElement]
    public partial class MatSlideToggle : BaseField<bool>
    {
        public const string BaseClassName = "mat-slide-toggle";
        public const string CheckedClassName = "mat-slide-toggle--checked";
        public const string LabelBeforeClassName = "mat-slide-toggle--label-before";
        public const string LabelAfterClassName = "mat-slide-toggle--label-after";
        public const string PrimaryClassName = "mat-primary";
        public const string AccentClassName = "mat-accent";
        public const string WarnClassName = "mat-warn";
        public const string FocusVisibleClassName = "mat-focus-visible";

        public const string LabelClassName = "mat-slide-toggle__label";
        public const string SwitchClassName = "mat-slide-toggle__switch";
        public const string TrackClassName = "mat-slide-toggle__track";
        public const string ThumbContainerClassName = "mat-slide-toggle__thumb-container";
        public const string ThumbClassName = "mat-slide-toggle__thumb";
        public const string ThumbIconClassName = "mat-slide-toggle__thumb-icon";

        private static readonly BindingId s_valueId = new(nameof(Value));
        private static readonly BindingId s_textId = new(nameof(Text));
        private static readonly BindingId s_themeColorId = new(nameof(ThemeColor));

        private static readonly string[] AllColorClasses =
        {
            PrimaryClassName, AccentClassName, WarnClassName,
        };

        private readonly VisualElement _visualInput;
        private readonly Label _label;
        private readonly VisualElement _switchHost;
        private readonly VisualElement _track;
        private readonly VisualElement _thumbContainer;
        private readonly VisualElement _thumb;
        private readonly VisualElement _thumbIcon;
        private readonly Clickable _clickable;

        private string _text = string.Empty;
        private MatSlideToggleColor _themeColor = MatSlideToggleColor.Default;
        private MatSlideToggleLabelPosition _labelPosition = MatSlideToggleLabelPosition.After;
        private bool _suppressFocusVisibleOnce;

        [CreateProperty]
        [UxmlAttribute]
        public bool Value
        {
            get => value;
            set => this.value = value;
        }

        [CreateProperty]
        [UxmlAttribute]
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                _label.text = _text;
                _label.style.display = string.IsNullOrEmpty(_text) ? DisplayStyle.None : DisplayStyle.Flex;
                NotifyPropertyChanged(s_textId);
            }
        }
        
        [CreateProperty]
        [UxmlAttribute]
        public MatSlideToggleColor ThemeColor
        {
            get => _themeColor;
            set
            {
                if (_themeColor == value)
                {
                    return;
                }

                _themeColor = value;
                foreach (var cls in AllColorClasses)
                {
                    RemoveFromClassList(cls);
                }

                if (value != MatSlideToggleColor.Default)
                {
                    AddToClassList(GetColorClassName(value));
                }

                NotifyPropertyChanged(s_themeColorId);
            }
        }

        [UxmlAttribute]
        public MatSlideToggleLabelPosition LabelPosition
        {
            get => _labelPosition;
            set
            {
                if (_labelPosition == value)
                {
                    return;
                }

                _labelPosition = value;
                UpdateLabelPosition();
            }
        }

        public MatSlideToggle() : this(new VisualElement())
        {
        }

        private MatSlideToggle(VisualElement visualInput) : base(null, visualInput)
        {
            _visualInput = visualInput;

            AddToClassList(BaseClassName);
            focusable = true;
            tabIndex = 0;

            labelElement.style.display = DisplayStyle.None;
            _visualInput.AddToClassList("mat-slide-toggle__visual-input");

            _label = new Label();
            _label.AddToClassList(LabelClassName);
            _label.pickingMode = PickingMode.Ignore;

            _switchHost = new VisualElement();
            _switchHost.AddToClassList(SwitchClassName);
            _switchHost.pickingMode = PickingMode.Ignore;

            _track = new VisualElement();
            _track.AddToClassList(TrackClassName);
            _switchHost.Add(_track);

            _thumbContainer = new VisualElement();
            _thumbContainer.AddToClassList(ThumbContainerClassName);
            _switchHost.Add(_thumbContainer);

            _thumb = new VisualElement();
            _thumb.AddToClassList(ThumbClassName);
            _thumbContainer.Add(_thumb);

            _thumbIcon = new VisualElement();
            _thumbIcon.AddToClassList(ThumbIconClassName);
            _thumbIcon.pickingMode = PickingMode.Ignore;
            _thumb.Add(_thumbIcon);

            _clickable = new Clickable(ToggleValue);
            this.AddManipulator(_clickable);

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<PointerDownEvent>(OnPointerDownForFocusVisible, TrickleDown.TrickleDown);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<ChangeEvent<bool>>(_ =>
            {
                UpdateCheckedState();
                NotifyPropertyChanged(s_valueId);
            });

            Text = string.Empty;
            ThemeColor = MatSlideToggleColor.Default;
            _labelPosition = MatSlideToggleLabelPosition.After;
            UpdateLabelPosition();
            SetValueWithoutNotify(false);
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateCheckedState();
        }

        private void ToggleValue()
        {
            if (!enabledInHierarchy)
            {
                return;
            }

            value = !value;
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
            ToggleValue();
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

        private void UpdateCheckedState()
        {
            EnableInClassList(CheckedClassName, value);
        }

        private void UpdateLabelPosition()
        {
            EnableInClassList(LabelBeforeClassName, _labelPosition == MatSlideToggleLabelPosition.Before);
            EnableInClassList(LabelAfterClassName, _labelPosition == MatSlideToggleLabelPosition.After);

            _visualInput.Clear();
            if (_labelPosition == MatSlideToggleLabelPosition.Before)
            {
                _visualInput.Add(_label);
                _visualInput.Add(_switchHost);
            }
            else
            {
                _visualInput.Add(_switchHost);
                _visualInput.Add(_label);
            }
        }

        private static string GetColorClassName(MatSlideToggleColor color) => color switch
        {
            MatSlideToggleColor.Primary => PrimaryClassName,
            MatSlideToggleColor.Accent => AccentClassName,
            MatSlideToggleColor.Warn => WarnClassName,
            _ => string.Empty,
        };
    }
}
