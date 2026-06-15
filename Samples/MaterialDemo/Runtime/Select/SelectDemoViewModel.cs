using System.Collections.Generic;
using Unity.Properties;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>
    /// ViewModel for the Select demo page.
    /// Demonstrates single-select, multiple-select, fill and outline appearances
    /// using Sinnoh-region Pokémon data as example content.
    /// </summary>
    public class SelectDemoViewModel : ViewModel<SelectDemoViewModel>, ISelectDemoDataContext
    {
        private string _selectedRegion = string.Empty;
        private List<string> _selectedTypes = new();
        private string _selectedStarter = string.Empty;

        [CreateProperty]
        public string SelectedRegion
        {
            get => _selectedRegion;
            set => SetProperty(ref _selectedRegion, value);
        }

        [CreateProperty]
        public List<string> SelectedTypes
        {
            get => _selectedTypes;
            set => SetProperty(ref _selectedTypes, value);
        }

        [CreateProperty]
        public string SelectedStarter
        {
            get => _selectedStarter;
            set => SetProperty(ref _selectedStarter, value);
        }

    }
}
