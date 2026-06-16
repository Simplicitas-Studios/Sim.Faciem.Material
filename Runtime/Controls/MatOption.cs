using Sim.Faciem.Controls;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// Represents a single selectable option inside a <see cref="MatSelect"/>.
    /// Declare these as UXML children of <c>&lt;Sim.Faciem.Controls.MatSelect&gt;</c>,
    /// exactly as you would use <c>&lt;mat-option&gt;</c> in Angular Material.
    /// </summary>
    [UxmlElement]
    public partial class MatOption : VisualElement
    {
        private string _value   = string.Empty;
        private string _label   = string.Empty;
        private bool   _disabled;

        /// <summary>The value submitted when this option is selected.</summary>
        [UxmlAttribute]
        public string Value
        {
            get => _value;
            set => _value = value ?? string.Empty;
        }

        /// <summary>
        /// The human-readable text shown in the dropdown list and in the trigger
        /// after selection. Falls back to <see cref="Value"/> when empty.
        /// </summary>
        [UxmlAttribute]
        public string Label
        {
            get => _label;
            set => _label = value ?? string.Empty;
        }

        /// <summary>When true the option is rendered as un-selectable.</summary>
        [UxmlAttribute]
        public bool Disabled
        {
            get => _disabled;
            set => _disabled = value;
        }
    }
}

