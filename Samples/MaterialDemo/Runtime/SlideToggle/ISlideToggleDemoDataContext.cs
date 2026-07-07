using Sim.Faciem.Commands;
using Sim.Faciem.Material.Controls;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>Data context contract for the Slide Toggle demo page.</summary>
    public interface ISlideToggleDemoDataContext : IDataContext
    {
        bool TogglesEnabled { get; }
        bool DisableRipple { get; }
        bool DisabledTogglesEnabled { get; }
        bool BoundValue { get; set; }
        MatSlideToggleColor BoundColor { get; }
        Command ToggleEnabled { get; }
        Command ToggleRipple { get; }
        Command CycleBoundColor { get; }
    }
}
