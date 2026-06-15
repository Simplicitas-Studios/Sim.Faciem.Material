using System.Collections;
using System.Collections.Generic;
using Sim.Faciem;
using Unity.Properties;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>
    /// ViewModel for the Material list demo page.
    /// Demonstrates a template-driven Material list and multi-selection list.
    /// </summary>
    public class ListDemoViewModel : ViewModel<ListDemoViewModel>, IListDemoDataContext
    {
        private readonly IList _routeMilestones;
        private readonly IList _gymBattles;

        private List<int> _selectedGymBattleIndices = new();
        private string _selectedGymBattleIndicesText = "[]";

        [CreateProperty]
        public IList RouteMilestones => _routeMilestones;

        [CreateProperty]
        public IList GymBattles => _gymBattles;

        [CreateProperty]
        public List<int> SelectedGymBattleIndices
        {
            get => _selectedGymBattleIndices;
            set
            {
                SetProperty(ref _selectedGymBattleIndices, value ?? new List<int>());
                SelectedGymBattleIndicesText = $"[{string.Join(", ", _selectedGymBattleIndices)}]";
            }
        }

        [CreateProperty]
        public string SelectedGymBattleIndicesText
        {
            get => _selectedGymBattleIndicesText;
            private set => SetProperty(ref _selectedGymBattleIndicesText, value);
        }

        public ListDemoViewModel()
        {
            _routeMilestones = new List<MaterialListDemoItem>
            {
                new() { Title = "Twinleaf Town", SupportingText = "Your journey begins with a quiet lakeside hometown." },
                new() { Title = "Jubilife City", SupportingText = "A bustling hub where trainers pick up Pokétch apps and news." },
                new() { Title = "Eterna Forest", SupportingText = "A misty forest route with bug Pokémon and hidden corners." },
                new() { Title = "Mt. Coronet", SupportingText = "The mountain spine of Sinnoh, connecting much of the region." },
            };

            _gymBattles = new List<MaterialListDemoItem>
            {
                new() { Title = "Oreburgh Gym", SupportingText = "Roark — Rock type specialist." },
                new() { Title = "Eterna Gym", SupportingText = "Gardenia — Grass type specialist." },
                new() { Title = "Veilstone Gym", SupportingText = "Maylene — Fighting type specialist." },
                new() { Title = "Pastoria Gym", SupportingText = "Wake — Water type specialist." },
                new() { Title = "Snowpoint Gym", SupportingText = "Candice — Ice type specialist." },
            };
        }
    }
}
