using System.Collections;
using Sim.Faciem;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>Data context contract for the Material grid-list demo page.</summary>
    public interface IGridListDemoDataContext : IDataContext
    {
        /// <summary>Data source for vertical square tiles.</summary>
        IList Regions { get; }

        /// <summary>Data source for vertical fixed-size tiles.</summary>
        IList Gyms { get; }

        /// <summary>Data source for horizontal square tiles.</summary>
        IList Routes { get; }

        /// <summary>Data source for horizontal fixed-size tiles.</summary>
        IList LeagueMembers { get; }

        /// <summary>Large data source used to demonstrate virtualization.</summary>
        IList LargeCatalog { get; }
    }
}
