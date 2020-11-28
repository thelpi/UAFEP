namespace UAFEP
{
    /// <summary>
    /// Enumeration of status for <see cref="MatchDay"/>.
    /// </summary>
    public enum MatchDayStatus
    {
        /// <summary>
        /// Complete; all matches have been played.
        /// </summary>
        Complete,
        /// <summary>
        /// In progress; some matches have been played but not all.
        /// </summary>
        InProgress,
        /// <summary>
        /// Pending; no matches have been played yet.
        /// </summary>
        Pending
    }
}
