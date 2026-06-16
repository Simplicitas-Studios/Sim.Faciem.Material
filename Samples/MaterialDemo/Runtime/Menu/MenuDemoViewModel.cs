using System.Threading;
using Cysharp.Threading.Tasks;
using Sim.Faciem.Commands;
using Unity.Properties;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>
    /// ViewModel for the Menu demo page.
    /// Demonstrates flat popup actions, disabled entries and navigation-oriented menu items.
    /// </summary>
    public class MenuDemoViewModel : ViewModel<MenuDemoViewModel>, IMenuDemoDataContext
    {
        private string _lastAction = "No menu action selected yet.";

        [CreateProperty]
        public Command CatchPokemon { get; }

        [CreateProperty]
        public Command OpenBag { get; }

        [CreateProperty]
        public Command SaveGame { get; }

        [CreateProperty]
        public Command NavigateToButtons { get; }

        [CreateProperty]
        public Command NavigateToSelect { get; }

        [CreateProperty]
        public Command NavigateToList { get; }

        [CreateProperty]
        public string LastAction
        {
            get => _lastAction;
            set => SetProperty(ref _lastAction, value);
        }

        public MenuDemoViewModel()
        {
            CatchPokemon = Command.Execute(() => LastAction = "Catch Pokémon selected from the menu." );
            OpenBag = Command.Execute(() => LastAction = "Open Bag selected from the menu." );
            SaveGame = Command.Execute(() => LastAction = "Save Game selected from the menu." );

            NavigateToButtons = Command.ExecuteAsync(ct => NavigateToDemo(WellKnownMatDemoViewIds.Sim_Faciem_Material_ButtonDemo, "Buttons demo", ct));
            NavigateToSelect = Command.ExecuteAsync(ct => NavigateToDemo(WellKnownMatDemoViewIds.Sim_Faciem_Material_SelectDemo, "Select demo", ct));
            NavigateToList = Command.ExecuteAsync(ct => NavigateToDemo(WellKnownMatDemoViewIds.Sim_Faciem_Material_ListDemo, "List demo", ct));
        }

        private async UniTask NavigateToDemo(ViewId viewId, string destination, CancellationToken ct = default)
        {
            LastAction = $"Navigating to the {destination}.";
            await Navigation.Navigate(viewId, WellKnownMatDemoRegions.Sim_Faciem_Material_DemoContent);
        }
    }
}
