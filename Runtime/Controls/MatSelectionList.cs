using UnityEngine.UIElements;

namespace Sim.Faciem.Controls
{
    /// <summary>
    /// Material-styled multi-selection list built on <see cref="MatList"/>.
    /// The first version exposes selection through <see cref="MatList.SelectedIndices"/>.
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
