using Sim.Faciem.Commands;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>Data context contract for the Material menu demo page.</summary>
    public interface IMenuDemoDataContext : IDataContext
    {
        Command CatchPokemon { get; }
        Command OpenBag { get; }
        Command SaveGame { get; }
        Command NavigateToButtons { get; }
        Command NavigateToSelect { get; }
        Command NavigateToList { get; }

        string LastAction { get; }
    }
}
