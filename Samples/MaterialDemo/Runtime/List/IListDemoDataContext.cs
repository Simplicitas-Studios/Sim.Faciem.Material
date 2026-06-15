using System.Collections;
using System.Collections.Generic;
using Sim.Faciem;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>Data context contract for the Material list demo page.</summary>
    public interface IListDemoDataContext : IDataContext
    {
        /// <summary>Data source for the plain Material list example.</summary>
        IList RouteMilestones { get; }

        /// <summary>Data source for the Material selection-list example.</summary>
        IList GymBattles { get; }

        /// <summary>Selected row indices for the selection-list demo.</summary>
        List<int> SelectedGymBattleIndices { get; }

        /// <summary>Human-readable reflection of the selected row indices.</summary>
        string SelectedGymBattleIndicesText { get; }
    }
}
