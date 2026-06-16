using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Sim.Faciem.Commands;
using Sim.Faciem.Controls;
using Sim.Faciem.Material.Controls;
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
        private MatButtonColor _iconNavColor           = MatButtonColor.Default;
        private MatButtonColor _buttonNavColor         = MatButtonColor.Default;
        private MatButtonColor _menuNavColor           = MatButtonColor.Default;
        private MatButtonColor _selectNavColor         = MatButtonColor.Default;
        private MatButtonColor _listNavColor           = MatButtonColor.Default;
        private MatButtonColor _gridListNavColor       = MatButtonColor.Default;

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
        public MatButtonColor IconNavColor
        {
            get => _iconNavColor;
            private set => SetProperty(ref _iconNavColor, value);
        }

        [CreateProperty]
        public MatButtonColor ButtonNavColor
        {
            get => _buttonNavColor;
            private set => SetProperty(ref _buttonNavColor, value);
        }

        [CreateProperty]
        public MatButtonColor MenuNavColor
        {
            get => _menuNavColor;
            private set => SetProperty(ref _menuNavColor, value);
        }

        [CreateProperty]
        public MatButtonColor SelectNavColor
        {
            get => _selectNavColor;
            private set => SetProperty(ref _selectNavColor, value);
        }

        [CreateProperty]
        public MatButtonColor ListNavColor
        {
            get => _listNavColor;
            private set => SetProperty(ref _listNavColor, value);
        }

        [CreateProperty]
        public MatButtonColor GridListNavColor
        {
            get => _gridListNavColor;
            private set => SetProperty(ref _gridListNavColor, value);
        }

        // ── Navigation commands ────────────────────────────────────────────────

        [CreateProperty]
        public Command NavigateToGettingStarted { get; private set; }

        [CreateProperty]
        public Command NavigateToTheming { get; private set; }

        [CreateProperty]
        public Command NavigateToIcon { get; private set; }

        [CreateProperty]
        public Command NavigateToButton { get; private set; }

        [CreateProperty]
        public Command NavigateToMenu { get; private set; }

        [CreateProperty]
        public Command NavigateToSelect { get; private set; }

        [CreateProperty]
        public Command NavigateToList { get; private set; }

        [CreateProperty]
        public Command NavigateToGridList { get; private set; }

        // ── Constructor ────────────────────────────────────────────────────────

        public MatDemoWindowViewModel()
        {
            NavigateToGettingStarted = Command.ExecuteAsync(ct => NavigateToPage(0, ct));
            NavigateToTheming        = Command.ExecuteAsync(ct => NavigateToPage(1, ct));
            NavigateToIcon           = Command.ExecuteAsync(ct => NavigateToPage(2, ct));
            NavigateToButton         = Command.ExecuteAsync(ct => NavigateToPage(3, ct));
            NavigateToMenu           = Command.ExecuteAsync(ct => NavigateToPage(4, ct));
            NavigateToSelect         = Command.ExecuteAsync(ct => NavigateToPage(5, ct));
            NavigateToList           = Command.ExecuteAsync(ct => NavigateToPage(6, ct));
            NavigateToGridList       = Command.ExecuteAsync(ct => NavigateToPage(7, ct));

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
                2 => WellKnownMatDemoViewIds.Sim_Faciem_Material_IconDemo,
                3 => WellKnownMatDemoViewIds.Sim_Faciem_Material_ButtonDemo,
                4 => WellKnownMatDemoViewIds.Sim_Faciem_Material_MenuDemo,
                5 => WellKnownMatDemoViewIds.Sim_Faciem_Material_SelectDemo,
                6 => WellKnownMatDemoViewIds.Sim_Faciem_Material_ListDemo,
                7 => WellKnownMatDemoViewIds.Sim_Faciem_Material_GridListDemo,
                _ => WellKnownMatDemoViewIds.Sim_Faciem_Material_GettingStarted,
            };

            await Navigation.Navigate(viewId, WellKnownMatDemoRegions.Sim_Faciem_Material_DemoContent);
        }

        private void UpdateNavColors(int index)
        {
            GettingStartedNavColor = index == 0 ? MatButtonColor.Primary : MatButtonColor.Default;
            ThemingNavColor        = index == 1 ? MatButtonColor.Primary : MatButtonColor.Default;
            IconNavColor           = index == 2 ? MatButtonColor.Primary : MatButtonColor.Default;
            ButtonNavColor         = index == 3 ? MatButtonColor.Primary : MatButtonColor.Default;
            MenuNavColor           = index == 4 ? MatButtonColor.Primary : MatButtonColor.Default;
            SelectNavColor         = index == 5 ? MatButtonColor.Primary : MatButtonColor.Default;
            ListNavColor           = index == 6 ? MatButtonColor.Primary : MatButtonColor.Default;
            GridListNavColor       = index == 7 ? MatButtonColor.Primary : MatButtonColor.Default;
        }
    }
    
    
}

