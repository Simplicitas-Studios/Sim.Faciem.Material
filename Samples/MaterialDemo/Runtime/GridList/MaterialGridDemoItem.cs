using Unity.Properties;

namespace Sim.Faciem.Material.Samples.GridList
{
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