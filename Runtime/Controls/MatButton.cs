using UnityEngine.UIElements;

namespace Sim.Faciem.Controls
{
    /// <summary>
    /// A button control that mirrors Angular Material's button system.
    /// Inherits all binding and command capabilities from <see cref="BindableButton"/>.
    /// Variant and colour are applied exclusively through USS classes; no stylesheet
    /// is loaded programmatically — add a MatButton theme TSS to your PanelSettings.
    /// </summary>
    [UxmlElement]
    public partial class MatButton : BindableButton
    {
        // ── Shared base ────────────────────────────────────────────────────────
        public const string BaseClassName = "mat-button-base";

        // ── Variant classes ────────────────────────────────────────────────────
        public const string BasicClassName    = "mat-button";
        public const string RaisedClassName   = "mat-raised-button";
        public const string StrokedClassName  = "mat-stroked-button";
        public const string FlatClassName     = "mat-flat-button";
        public const string IconClassName     = "mat-icon-button";
        public const string FabClassName      = "mat-fab";
        public const string MiniFabClassName  = "mat-mini-fab";

        // ── Colour classes ─────────────────────────────────────────────────────
        public const string PrimaryClassName  = "mat-primary";
        public const string AccentClassName   = "mat-accent";
        public const string WarnClassName     = "mat-warn";

        // ── Internal lookup arrays ─────────────────────────────────────────────
        private static readonly string[] AllVariantClasses =
        {
            BasicClassName, RaisedClassName, StrokedClassName, FlatClassName,
            IconClassName, FabClassName, MiniFabClassName,
        };

        private static readonly string[] AllColorClasses =
        {
            PrimaryClassName, AccentClassName, WarnClassName,
        };

        // ── Backing fields ─────────────────────────────────────────────────────
        private MatButtonVariant _variant    = MatButtonVariant.Basic;
        private MatButtonColor   _themeColor = MatButtonColor.Default;

        // ── UXML attributes ────────────────────────────────────────────────────

        /// <summary>Controls which Angular Material button appearance is applied.</summary>
        [UxmlAttribute]
        public MatButtonVariant Variant
        {
            get => _variant;
            set
            {
                _variant = value;
                foreach (var cls in AllVariantClasses)
                    RemoveFromClassList(cls);
                AddToClassList(GetVariantClassName(value));
            }
        }

        /// <summary>Applies a theme palette colour class to the button.</summary>
        [UxmlAttribute]
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

        // ── Constructor ────────────────────────────────────────────────────────

        public MatButton()
        {
            AddToClassList(BaseClassName);
            Variant    = MatButtonVariant.Basic;
            ThemeColor = MatButtonColor.Default;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string GetVariantClassName(MatButtonVariant variant) => variant switch
        {
            MatButtonVariant.Basic    => BasicClassName,
            MatButtonVariant.Raised   => RaisedClassName,
            MatButtonVariant.Stroked  => StrokedClassName,
            MatButtonVariant.Flat     => FlatClassName,
            MatButtonVariant.Icon     => IconClassName,
            MatButtonVariant.Fab      => FabClassName,
            MatButtonVariant.MiniFab  => MiniFabClassName,
            _                         => BasicClassName,
        };

        private static string GetColorClassName(MatButtonColor color) => color switch
        {
            MatButtonColor.Primary => PrimaryClassName,
            MatButtonColor.Accent  => AccentClassName,
            MatButtonColor.Warn    => WarnClassName,
            _                      => string.Empty,
        };
    }
}

