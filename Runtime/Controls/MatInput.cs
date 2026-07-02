using System;
using System.Globalization;
using System.Text;
using R3;
using Sim.Faciem.Shared;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// Material text / numeric input built on top of the shared <see cref="MatFormField"/> chrome.
    /// Supports fill / outline appearance, floating label, hint text, placeholder text and
    /// optional leading / trailing SVG icons.
    /// </summary>
    [UxmlElement]
    public partial class MatInput : VisualElement
    {
        public const string BaseClassName = "mat-input";
        public const string DisabledClassName = "mat-input--disabled";
        public const string TextClassName = "mat-input--text";
        public const string IntegerClassName = "mat-input--integer";
        public const string FloatClassName = "mat-input--float";
        public const string RowClassName = "mat-input__row";
        public const string IconClassName = "mat-input__icon";
        public const string LeadingIconClassName = "mat-input__icon--leading";
        public const string TrailingIconClassName = "mat-input__icon--trailing";
        public const string FieldHostClassName = "mat-input__field-host";
        public const string FieldClassName = "mat-input__field";
        public const string FieldInputClassName = "mat-input__field-input";
        public const string PlaceholderClassName = "mat-input__placeholder";

        private static readonly BindingId s_valueId = new(nameof(Value));
        private static readonly BindingId s_integerValueId = new(nameof(IntegerValue));
        private static readonly BindingId s_floatValueId = new(nameof(FloatValue));

        private readonly MatFormField _formField;
        private readonly VisualElement _row;
        private readonly VisualElement _leadingIcon;
        private readonly VisualElement _fieldHost;
        private readonly TextField _field;
        private readonly Label _placeholderLabel;
        private readonly VisualElement _trailingIcon;
        private readonly DisposableBagHolder _disposables;

        private MatFormFieldAppearance _appearance = MatFormFieldAppearance.Fill;
        private string _label = string.Empty;
        private string _hintText = string.Empty;
        private string _placeholder = string.Empty;
        private bool _disabled;
        private MatInputType _inputType = MatInputType.Text;
        private string _value = string.Empty;
        private Background _leadingIconBackground;
        private Background _trailingIconBackground;
        private bool _isFocused;
        private bool _suppressFieldChange;

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

        [UxmlAttribute]
        public string Placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value ?? string.Empty;
                _placeholderLabel.text = _placeholder;
                UpdatePlaceholderVisibility();
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
                _formField.SetDisabled(value);
                _field.SetEnabled(!value);
                UpdatePlaceholderVisibility();
            }
        }

        [UxmlAttribute]
        public MatInputType InputType
        {
            get => _inputType;
            set
            {
                if (_inputType == value)
                {
                    return;
                }

                _inputType = value;
                _value = NormalizeValue(_value, _inputType);
                UpdateInputTypeClasses();
                ApplyValueToField();
                UpdateHasValueState();
                UpdatePlaceholderVisibility();
                NotifyPropertyChanged(s_valueId);
                NotifyPropertyChanged(s_integerValueId);
                NotifyPropertyChanged(s_floatValueId);
            }
        }

        [UxmlAttribute]
        public Background LeadingIcon
        {
            get => _leadingIconBackground;
            set
            {
                _leadingIconBackground = value;
                UpdateIconVisuals();
            }
        }

        [UxmlAttribute]
        public Background TrailingIcon
        {
            get => _trailingIconBackground;
            set
            {
                _trailingIconBackground = value;
                UpdateIconVisuals();
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public string Value
        {
            get => _value;
            set
            {
                var normalized = NormalizeValue(value, _inputType);
                if (_value == normalized)
                {
                    return;
                }

                _value = normalized;
                ApplyValueToField();
                UpdateHasValueState();
                UpdatePlaceholderVisibility();
                NotifyPropertyChanged(s_valueId);
                NotifyPropertyChanged(s_integerValueId);
                NotifyPropertyChanged(s_floatValueId);
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public int IntegerValue
        {
            get => int.TryParse(_value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
            set
            {
                var nextValue = value.ToString(CultureInfo.InvariantCulture);
                if (_value == nextValue)
                {
                    return;
                }

                _value = nextValue;
                ApplyValueToField();
                UpdateHasValueState();
                UpdatePlaceholderVisibility();
                NotifyPropertyChanged(s_valueId);
                NotifyPropertyChanged(s_integerValueId);
                NotifyPropertyChanged(s_floatValueId);
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public float FloatValue
        {
            get => float.TryParse(_value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0f;
            set
            {
                var nextValue = value.ToString(CultureInfo.InvariantCulture);
                if (_value == nextValue)
                {
                    return;
                }

                _value = nextValue;
                ApplyValueToField();
                UpdateHasValueState();
                UpdatePlaceholderVisibility();
                NotifyPropertyChanged(s_valueId);
                NotifyPropertyChanged(s_integerValueId);
                NotifyPropertyChanged(s_floatValueId);
            }
        }

        public MatInput()
        {
            AddToClassList(BaseClassName);

            _disposables = this.RegisterDisposableBag();

            _row = new VisualElement();
            _row.AddToClassList(RowClassName);

            _leadingIcon = new VisualElement();
            _leadingIcon.AddToClassList(IconClassName);
            _leadingIcon.AddToClassList(LeadingIconClassName);
            _row.Add(_leadingIcon);

            _fieldHost = new VisualElement();
            _fieldHost.AddToClassList(FieldHostClassName);
            _row.Add(_fieldHost);

            _field = new TextField();
            _field.AddToClassList(FieldClassName);
            _field.labelElement.style.display = DisplayStyle.None;
            _field.style.flexGrow = 1f;
            _field.style.minWidth = 0f;
            _field.style.marginLeft = 0f;
            _field.style.marginRight = 0f;
            _field.style.marginTop = 0f;
            _field.style.marginBottom = 0f;
            _field.style.paddingLeft = 0f;
            _field.style.paddingRight = 0f;
            _field.style.paddingTop = 0f;
            _field.style.paddingBottom = 0f;
            _field.style.borderTopWidth = 0f;
            _field.style.borderBottomWidth = 0f;
            _field.style.borderLeftWidth = 0f;
            _field.style.borderRightWidth = 0f;
            _field.style.backgroundColor = Color.clear;
            _fieldHost.Add(_field);

            _placeholderLabel = new Label();
            _placeholderLabel.AddToClassList(PlaceholderClassName);
            _placeholderLabel.pickingMode = PickingMode.Ignore;
            _fieldHost.Add(_placeholderLabel);

            _trailingIcon = new VisualElement();
            _trailingIcon.AddToClassList(IconClassName);
            _trailingIcon.AddToClassList(TrailingIconClassName);
            _row.Add(_trailingIcon);

            _formField = new MatFormField();
            _formField.Infix.Add(_row);
            Add(_formField);

            _field.RegisterValueChangedCallback(OnFieldValueChanged);

            _disposables.Add(_formField.PointerDownAsObservable().Subscribe(OnFormFieldPointerDown));
            _disposables.Add(_field.FocusInAsObservable().Subscribe(_ =>
            {
                _isFocused = true;
                _formField.SetFocused(true);
                UpdatePlaceholderVisibility();
            }));
            _disposables.Add(_field.BlurAsObservable().Subscribe(_ =>
            {
                _isFocused = false;
                _formField.SetFocused(false);
                UpdatePlaceholderVisibility();
            }));

            UpdateInputTypeClasses();
            UpdateIconVisuals();
            ApplyValueToField();
            UpdateHasValueState();
            UpdatePlaceholderVisibility();
        }

        private void OnFormFieldPointerDown(PointerDownEvent evt)
        {
            if (_disabled)
            {
                return;
            }

            if (evt.target is not VisualElement target)
            {
                return;
            }

            if (target.ClassListContains(MatFormField.SubscriptClassName))
            {
                return;
            }

            _field.Focus();
        }

        private void OnFieldValueChanged(ChangeEvent<string> evt)
        {
            if (_suppressFieldChange)
            {
                return;
            }

            var normalized = NormalizeValue(evt.newValue, _inputType);
            if (!string.Equals(evt.newValue, normalized, StringComparison.Ordinal))
            {
                _suppressFieldChange = true;
                _field.SetValueWithoutNotify(normalized);
                _suppressFieldChange = false;
            }

            if (_value == normalized)
            {
                UpdateHasValueState();
                UpdatePlaceholderVisibility();
                return;
            }

            _value = normalized;
            UpdateHasValueState();
            UpdatePlaceholderVisibility();
            NotifyPropertyChanged(s_valueId);
            NotifyPropertyChanged(s_integerValueId);
            NotifyPropertyChanged(s_floatValueId);
        }

        private void ApplyValueToField()
        {
            var displayValue = _value ?? string.Empty;
            if (string.Equals(_field.value, displayValue, StringComparison.Ordinal))
            {
                return;
            }

            _suppressFieldChange = true;
            _field.SetValueWithoutNotify(displayValue);
            _suppressFieldChange = false;
        }

        private void UpdateInputTypeClasses()
        {
            EnableInClassList(TextClassName, _inputType == MatInputType.Text);
            EnableInClassList(IntegerClassName, _inputType == MatInputType.Integer);
            EnableInClassList(FloatClassName, _inputType == MatInputType.Float);
        }

        private void UpdateHasValueState()
        {
            _formField.SetHasValue(!string.IsNullOrEmpty(_value));
        }

        private void UpdatePlaceholderVisibility()
        {
            var visible = !_disabled && !_isFocused && string.IsNullOrEmpty(_value) && !string.IsNullOrEmpty(_placeholder);
            _placeholderLabel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateIconVisuals()
        {
            SetIconVisual(_leadingIcon, LeadingIcon);
            SetIconVisual(_trailingIcon, TrailingIcon);
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

        private static string NormalizeValue(string value, MatInputType inputType)
        {
            value ??= string.Empty;

            return inputType switch
            {
                MatInputType.Integer => NormalizeInteger(value),
                MatInputType.Float => NormalizeFloat(value),
                _ => value,
            };
        }

        private static string NormalizeInteger(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            var sawSign = false;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == '-' && i == 0 && !sawSign)
                {
                    builder.Append(c);
                    sawSign = true;
                    continue;
                }

                if (char.IsDigit(c))
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        private static string NormalizeFloat(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            var sawSign = false;
            var sawDecimal = false;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == '-' && i == 0 && !sawSign)
                {
                    builder.Append(c);
                    sawSign = true;
                    continue;
                }

                if ((c == '.' || c == ',') && !sawDecimal)
                {
                    builder.Append('.');
                    sawDecimal = true;
                    continue;
                }

                if (char.IsDigit(c))
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }


    }
}
