namespace Sim.Faciem.Controls
{
    /// <summary>
    /// Controls whether <see cref="MatGridList"/> flows vertically or horizontally.
    /// </summary>
    public enum MatGridListDirection
    {
        /// <summary>
        /// Tiles flow top-to-bottom. <c>TrackCount</c> means columns and <c>TileSize</c> means row height.
        /// </summary>
        Vertical,

        /// <summary>
        /// Tiles flow left-to-right. <c>TrackCount</c> means rows and <c>TileSize</c> means column width.
        /// </summary>
        Horizontal,
    }
}
