using System.Collections;
using System.Collections.Generic;
using Sim.Faciem;
using Unity.Properties;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>
    /// ViewModel for the Material grid-list demo page.
    /// Demonstrates template-driven square and rectangular tiles in both directions.
    /// </summary>
    public class GridListDemoViewModel : ViewModel<GridListDemoViewModel>, IGridListDemoDataContext
    {
        private readonly IList _regions;
        private readonly IList _gyms;
        private readonly IList _routes;
        private readonly IList _leagueMembers;
        private readonly IList _largeCatalog;

        [CreateProperty]
        public IList Regions => _regions;

        [CreateProperty]
        public IList Gyms => _gyms;

        [CreateProperty]
        public IList Routes => _routes;

        [CreateProperty]
        public IList LeagueMembers => _leagueMembers;

        [CreateProperty]
        public IList LargeCatalog => _largeCatalog;

        public GridListDemoViewModel()
        {
            _regions = new List<MaterialGridDemoItem>
            {
                new() { Title = "Twinleaf", SupportingText = "Quiet lakeside hometown" },
                new() { Title = "Jubilife", SupportingText = "City of apps and trainers" },
                new() { Title = "Eterna", SupportingText = "Ancient city by the forest" },
                new() { Title = "Canalave", SupportingText = "Harbor city and library" },
                new() { Title = "Sunyshore", SupportingText = "Solar-powered seaside city" },
                new() { Title = "Snowpoint", SupportingText = "Northern city of snow" },
            };

            _gyms = new List<MaterialGridDemoItem>
            {
                new() { Title = "Roark", SupportingText = "Oreburgh Gym" },
                new() { Title = "Gardenia", SupportingText = "Eterna Gym" },
                new() { Title = "Maylene", SupportingText = "Veilstone Gym" },
                new() { Title = "Wake", SupportingText = "Pastoria Gym" },
            };

            _routes = new List<MaterialGridDemoItem>
            {
                new() { Title = "Route 201", SupportingText = "Twinleaf to Sandgem" },
                new() { Title = "Route 202", SupportingText = "Trainer-filled opening path" },
                new() { Title = "Route 203", SupportingText = "Road to Oreburgh Gate" },
                new() { Title = "Route 204", SupportingText = "Northbound valley route" },
                new() { Title = "Route 205", SupportingText = "Windworks and forest split" },
                new() { Title = "Route 206", SupportingText = "Cycling Road overlook" },
            };

            _leagueMembers = new List<MaterialGridDemoItem>
            {
                new() { Title = "Aaron", SupportingText = "Bug-type specialist" },
                new() { Title = "Bertha", SupportingText = "Ground-type specialist" },
                new() { Title = "Flint", SupportingText = "Fire-type specialist" },
                new() { Title = "Lucian", SupportingText = "Psychic-type specialist" },
                new() { Title = "Cynthia", SupportingText = "Champion of Sinnoh" },
            };

            var largeCatalog = new List<MaterialGridDemoItem>();
            for (var i = 1; i <= 480; i++)
            {
                largeCatalog.Add(new MaterialGridDemoItem
                {
                    Title = $"Entry {i:000}",
                    SupportingText = $"Virtualized tile #{i} for long scrolling validation"
                });
            }

            _largeCatalog = largeCatalog;
        }
    }

    /// <summary>Simple bindable tile model used by the Material grid-list demo templates.</summary>
    public class MaterialGridDemoItem : Bindable<MaterialGridDemoItem>
    {
        private string _title;
        private string _supportingText;

        [CreateProperty]
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        [CreateProperty]
        public string SupportingText
        {
            get => _supportingText;
            set => SetProperty(ref _supportingText, value);
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}
