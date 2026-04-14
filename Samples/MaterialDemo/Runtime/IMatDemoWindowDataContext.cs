using Sim.Faciem;
using Sim.Faciem.Commands;
using Sim.Faciem.Controls;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>Data context contract for the Material Demo shell window.</summary>
    public interface IMatDemoWindowDataContext : IDataContext
    {
        Command NavigateToGettingStarted { get; }
        Command NavigateToTheming { get; }
        Command NavigateToButton { get; }

        MatButtonColor GettingStartedNavColor { get; }
        MatButtonColor ThemingNavColor { get; }
        MatButtonColor ButtonNavColor { get; }
    }
}

