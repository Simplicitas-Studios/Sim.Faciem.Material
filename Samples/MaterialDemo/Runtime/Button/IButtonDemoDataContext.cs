using Sim.Faciem;
using Sim.Faciem.Commands;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>Data context contract for the Button demo page.</summary>
    public interface IButtonDemoDataContext : IDataContext
    {
        /// <summary>Toggles whether the demo buttons are enabled or disabled.</summary>
        Command ToggleEnabled { get; }

        bool ButtonsEnabled { get; }
    }
}

