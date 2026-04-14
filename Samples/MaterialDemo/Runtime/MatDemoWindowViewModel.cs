using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Sim.Faciem.Commands;
using Sim.Faciem.Controls;
using Unity.Properties;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>
    /// Shell ViewModel for the Material Demo window.
    /// Manages which demo page is shown in the content Region and drives
    /// the active-state colour of the left-side navigation buttons.
    /// </summary>
    public class MatDemoWindowViewModel : ViewModel<MatDemoWindowViewModel>, IMatDemoWindowDataContext
    {
        private readonly ReactiveProperty<int> _selectedNavIndex = new(0);

        // ── Nav colour backing fields ──────────────────────────────────────────
        private MatButtonColor _gettingStartedNavColor = MatButtonColor.Primary;
        private MatButtonColor _themingNavColor        = MatButtonColor.Default;
        private MatButtonColor _buttonNavColor         = MatButtonColor.Default;

        // ── Nav colour properties (Primary = active, Default = inactive) ───────

        [CreateProperty]
        public MatButtonColor GettingStartedNavColor
        {
            get => _gettingStartedNavColor;
            private set => SetProperty(ref _gettingStartedNavColor, value);
        }

        [CreateProperty]
        public MatButtonColor ThemingNavColor
        {
            get => _themingNavColor;
            private set => SetProperty(ref _themingNavColor, value);
        }

        [CreateProperty]
        public MatButtonColor ButtonNavColor
        {
            get => _buttonNavColor;
            private set => SetProperty(ref _buttonNavColor, value);
        }

        // ── Navigation commands ────────────────────────────────────────────────

        [CreateProperty]
        public Command NavigateToGettingStarted { get; private set; }

        [CreateProperty]
        public Command NavigateToTheming { get; private set; }

        [CreateProperty]
        public Command NavigateToButton { get; private set; }

        // ── Constructor ────────────────────────────────────────────────────────

        public MatDemoWindowViewModel()
        {
            NavigateToGettingStarted = Command.ExecuteAsync(ct => NavigateToPage(0, ct));
            NavigateToTheming        = Command.ExecuteAsync(ct => NavigateToPage(1, ct));
            NavigateToButton         = Command.ExecuteAsync(ct => NavigateToPage(2, ct));

            Disposables.Add(_selectedNavIndex.Subscribe(UpdateNavColors));
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        protected override async UniTask NavigateTo()
        {
            await NavigateToPage(0);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private async UniTask NavigateToPage(int index, CancellationToken ct = default)
        {
            _selectedNavIndex.Value = index;

            var viewId = index switch
            {
                1 => WellKnownMatDemoViewIds.Sim_Faciem_Material_ThemingDemo,
                2 => WellKnownMatDemoViewIds.Sim_Faciem_Material_ButtonDemo,
                _ => WellKnownMatDemoViewIds.Sim_Faciem_Material_GettingStarted,
            };

            await Navigation.Navigate(viewId, WellKnownMatDemoRegions.Sim_Faciem_Material_DemoContent);
        }

        private void UpdateNavColors(int index)
        {
            GettingStartedNavColor = index == 0 ? MatButtonColor.Primary : MatButtonColor.Default;
            ThemingNavColor        = index == 1 ? MatButtonColor.Primary : MatButtonColor.Default;
            ButtonNavColor         = index == 2 ? MatButtonColor.Primary : MatButtonColor.Default;
        }
    }
    
}

