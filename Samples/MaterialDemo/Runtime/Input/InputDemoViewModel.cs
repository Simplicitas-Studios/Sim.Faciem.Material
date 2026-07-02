using Unity.Properties;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>
    /// ViewModel for the Input demo page.
    /// Demonstrates Material text and numeric inputs with shared form-field chrome and icons.
    /// </summary>
    public class InputDemoViewModel : ViewModel<InputDemoViewModel>, IInputDemoDataContext
    {
        private string _trainerName = string.Empty;
        private string _searchQuery = string.Empty;
        private string _pokedexNumber = string.Empty;
        private string _catchRate = string.Empty;

        [CreateProperty]
        public string TrainerName
        {
            get => _trainerName;
            set => SetProperty(ref _trainerName, value);
        }

        [CreateProperty]
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        [CreateProperty]
        public string PokedexNumber
        {
            get => _pokedexNumber;
            set => SetProperty(ref _pokedexNumber, value);
        }

        [CreateProperty]
        public string CatchRate
        {
            get => _catchRate;
            set => SetProperty(ref _catchRate, value);
        }
    }
}
