namespace UAFEP
{
    /// <summary>
    /// Represents a team.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// Team name.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
