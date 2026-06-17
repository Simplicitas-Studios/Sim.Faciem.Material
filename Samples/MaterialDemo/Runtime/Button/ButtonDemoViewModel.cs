using Sim.Faciem.Commands;
using Unity.Properties;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>
    /// ViewModel for the Button demo page.
    /// Showcases all MatButton variants using Sinnoh move categories as context.
    /// Provides a toggle to demonstrate the :disabled pseudo-class styling.
    /// </summary>
    public class ButtonDemoViewModel : ViewModel<ButtonDemoViewModel>, IButtonDemoDataContext
    {
        private bool _buttonsEnabled = true;
        private bool _disableRipple;

        [CreateProperty]
        public bool ButtonsEnabled
        {
            get => _buttonsEnabled;
            private set => SetProperty(ref _buttonsEnabled, value);
        }

        [CreateProperty]
        public bool DisableRipple
        {
            get => _disableRipple;
            private set => SetProperty(ref _disableRipple, value);
        }

        [CreateProperty]
        public Command ToggleEnabled { get; private set; }

        [CreateProperty]
        public Command ToggleRipple { get; private set; }

        public ButtonDemoViewModel()
        {
            ToggleEnabled = Command.Execute(() => ButtonsEnabled = !ButtonsEnabled);
            ToggleRipple = Command.Execute(() => DisableRipple = !DisableRipple);
        }
    }
}

