using Sim.Faciem.Commands;
using Sim.Faciem.Controls;
using Sim.Faciem.Material.Controls;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>Data context contract for the Material Demo shell window.</summary>
    public interface IMatDemoWindowDataContext : IDataContext
    {
        Command NavigateToGettingStarted { get; }
        Command NavigateToTheming { get; }
        Command NavigateToIcon { get; }
        Command NavigateToButton { get; }
        Command NavigateToMenu { get; }
        Command NavigateToSelect { get; }
        Command NavigateToInput { get; }
        Command NavigateToList { get; }
        Command NavigateToGridList { get; }
        Command NavigateToSlideToggle { get; }

        MatButtonColor GettingStartedNavColor { get; }
        MatButtonColor ThemingNavColor { get; }
        MatButtonColor IconNavColor { get; }
        MatButtonColor ButtonNavColor { get; }
        MatButtonColor MenuNavColor { get; }
        MatButtonColor SelectNavColor { get; }
        MatButtonColor InputNavColor { get; }
        MatButtonColor ListNavColor { get; }
        MatButtonColor GridListNavColor { get; }
        MatButtonColor SlideToggleNavColor { get; }
    }
}

