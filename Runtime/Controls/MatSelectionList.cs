using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// Material-styled multi-selection list built on <see cref="Material.Controls.MatList"/>.
    /// The first version exposes selection through <see cref="Material.Controls.MatList.SelectedIndices"/>.
    /// </summary>
    [UxmlElement]
    public partial class MatSelectionList : MatList
    {
        /// <summary>
        /// Initializes a new Material selection list with multi-selection enabled.
        /// </summary>
        public MatSelectionList()
        {
            SetSelectionListMode(true);
        }
    }
}
