using Sim.Faciem.Controls;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// Internal form-field chrome shared by MatSelect (and future MatInput / MatTextarea).
    /// Provides the fill / outline appearance, floating label, underline indicator and
    /// subscript (hint / error) row.
    ///
    /// Not exposed as a UxmlElement — each control that needs form-field styling
    /// constructs one privately. Promoting this to a public [UxmlElement] later only
    /// requires adding the attribute; no structural changes are needed.
    /// </summary>
    internal class MatFormField : VisualElement
    {
        // ── CSS class constants ────────────────────────────────────────────────
        public const string BaseClassName              = "mat-form-field";
        public const string FillClassName              = "mat-form-field--fill";
        public const string OutlineClassName           = "mat-form-field--outline";
        public const string FocusedClassName           = "mat-form-field--focused";
        public const string DisabledClassName          = "mat-form-field--disabled";
        public const string HasValueClassName          = "mat-form-field--has-value";
        public const string WrapperClassName           = "mat-form-field__wrapper";
        public const string FlexClassName              = "mat-form-field__flex";
        public const string InfixClassName             = "mat-form-field__infix";
        public const string LabelClassName             = "mat-form-field__label";
        public const string UnderlineClassName         = "mat-form-field__underline";
        public const string UnderlineRippleClassName   = "mat-form-field__underline-ripple";
        public const string OutlineContainerClassName  = "mat-form-field__outline-container";
        public const string SubscriptClassName         = "mat-form-field__subscript";

        // ── Internal DOM ───────────────────────────────────────────────────────
        private readonly VisualElement _wrapper;
        private readonly VisualElement _flex;
        private readonly Label         _label;
        private readonly VisualElement _underline;
        private readonly VisualElement _outlineContainer;
        private readonly Label         _subscript;

        // ── Public slot — host control injects its trigger / input here ────────
        public VisualElement Infix { get; }

        // ── Backing fields (ready for future [UxmlAttribute] promotion) ────────
        private MatFormFieldAppearance _appearance = MatFormFieldAppearance.Fill;
        private string                 _labelText  = string.Empty;

        // ── Properties ────────────────────────────────────────────────────────

        public MatFormFieldAppearance Appearance
        {
            get => _appearance;
            set
            {
                _appearance = value;
                RemoveFromClassList(FillClassName);
                RemoveFromClassList(OutlineClassName);
                AddToClassList(value == MatFormFieldAppearance.Outline
                    ? OutlineClassName
                    : FillClassName);
                _underline.style.display       = value == MatFormFieldAppearance.Fill
                    ? DisplayStyle.Flex : DisplayStyle.None;
                _outlineContainer.style.display = value == MatFormFieldAppearance.Outline
                    ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public string LabelText
        {
            get => _labelText;
            set
            {
                _labelText  = value ?? string.Empty;
                _label.text = _labelText;
                _label.style.display = string.IsNullOrEmpty(_labelText)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
        }

        // ── Constructor ────────────────────────────────────────────────────────

        public MatFormField()
        {
            AddToClassList(BaseClassName);

            _wrapper = new VisualElement();
            _wrapper.AddToClassList(WrapperClassName);

            _flex = new VisualElement();
            _flex.AddToClassList(FlexClassName);

            // Label sits above the infix content
            _label = new Label();
            _label.AddToClassList(LabelClassName);
            _label.style.display = DisplayStyle.None;
            _flex.Add(_label);

            // Infix: host controls place their trigger / input field here
            Infix = new VisualElement();
            Infix.AddToClassList(InfixClassName);
            _flex.Add(Infix);

            _wrapper.Add(_flex);

            // Fill underline
            _underline = new VisualElement();
            _underline.AddToClassList(UnderlineClassName);
            var ripple = new VisualElement();
            ripple.AddToClassList(UnderlineRippleClassName);
            _underline.Add(ripple);
            _wrapper.Add(_underline);

            // Outline border container (visible only in Outline mode)
            _outlineContainer = new VisualElement();
            _outlineContainer.AddToClassList(OutlineContainerClassName);
            _wrapper.Add(_outlineContainer);

            Add(_wrapper);

            // Subscript — hint / error text below the field
            _subscript = new Label();
            _subscript.AddToClassList(SubscriptClassName);
            _subscript.style.display = DisplayStyle.None;
            Add(_subscript);

            // Apply default appearance
            Appearance = MatFormFieldAppearance.Fill;
        }

        // ── State helpers (called by the hosting control) ──────────────────────

        /// <summary>Toggles the focused appearance (underline/outline colour change).</summary>
        public void SetFocused(bool focused)   => EnableInClassList(FocusedClassName, focused);

        /// <summary>Toggles the disabled appearance (muted colours).</summary>
        public void SetDisabled(bool disabled) => EnableInClassList(DisabledClassName, disabled);

        /// <summary>Toggles the has-value state (label stays floating above the field).</summary>
        public void SetHasValue(bool hasValue) => EnableInClassList(HasValueClassName, hasValue);

        /// <summary>Shows or hides the subscript hint text beneath the field.</summary>
        public void SetHintText(string hint)
        {
            _subscript.text = hint ?? string.Empty;
            _subscript.style.display = string.IsNullOrEmpty(hint)
                ? DisplayStyle.None
                : DisplayStyle.Flex;
        }
    }
}

