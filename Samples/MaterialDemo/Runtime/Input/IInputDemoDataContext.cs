namespace Sim.Faciem.Material.Samples
{
    /// <summary>Data context contract for the Input demo page.</summary>
    public interface IInputDemoDataContext : IDataContext
    {
        string TrainerName { get; }
        string SearchQuery { get; }
        string PokedexNumber { get; }
        string CatchRate { get; }
    }
}
