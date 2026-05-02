using System.Collections.Generic;
using Sim.Faciem;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>Data context contract for the Select demo page.</summary>
    public interface ISelectDemoDataContext : IDataContext
    {
        /// <summary>Selected Pokémon region for the single-select example.</summary>
        string SelectedRegion { get; }

        /// <summary>Selected Pokémon types for the multiple-select example.</summary>
        List<string> SelectedTypes { get; }

        /// <summary>Selected starter for the outline appearance example.</summary>
        string SelectedStarter { get; }
    }
}

