using System.Collections;

namespace Sim.Faciem.Material.Samples.Icon
{
    /// <summary>Data context contract for the Material icon demo page.</summary>
    public interface IIconDemoDataContext : IDataContext
    {
        /// <summary>Case-insensitive file-name filter applied to the icon catalog.</summary>
        string SearchQuery { get; set; }

        /// <summary>Filtered icon catalog shown in the grid.</summary>
        IList FilteredIcons { get; }
    }
}
