using Sim.Faciem.Commands;
using Sim.Faciem.Material.Controls;
using Unity.Properties;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>ViewModel for the MatSlideToggle demo page.</summary>
    public class SlideToggleDemoViewModel : ViewModel<SlideToggleDemoViewModel>, ISlideToggleDemoDataContext
    {
        private bool _togglesEnabled = true;
        private bool _disableRipple;
        private bool _boundValue = true;
        private MatSlideToggleColor _boundColor = MatSlideToggleColor.Primary;

        [CreateProperty]
        public bool TogglesEnabled
        {
            get => _togglesEnabled;
            private set => SetProperty(ref _togglesEnabled, value);
        }

        [CreateProperty]
        public bool DisableRipple
        {
            get => _disableRipple;
            private set => SetProperty(ref _disableRipple, value);
        }

        [CreateProperty]
        public bool DisabledTogglesEnabled => false;

        [CreateProperty]
        public bool BoundValue
        {
            get => _boundValue;
            set => SetProperty(ref _boundValue, value);
        }

        [CreateProperty]
        public MatSlideToggleColor BoundColor
        {
            get => _boundColor;
            private set => SetProperty(ref _boundColor, value);
        }

        [CreateProperty]
        public Command ToggleEnabled { get; private set; }

        [CreateProperty]
        public Command ToggleRipple { get; private set; }

        [CreateProperty]
        public Command CycleBoundColor { get; private set; }

        public SlideToggleDemoViewModel()
        {
            ToggleEnabled = Command.Execute(() => TogglesEnabled = !TogglesEnabled);
            ToggleRipple = Command.Execute(() => DisableRipple = !DisableRipple);
            CycleBoundColor = Command.Execute(CycleColor);
        }

        private void CycleColor()
        {
            BoundColor = BoundColor switch
            {
                MatSlideToggleColor.Default => MatSlideToggleColor.Primary,
                MatSlideToggleColor.Primary => MatSlideToggleColor.Accent,
                MatSlideToggleColor.Accent => MatSlideToggleColor.Warn,
                _ => MatSlideToggleColor.Default,
            };
        }
    }
}
