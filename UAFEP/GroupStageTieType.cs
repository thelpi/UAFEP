namespace UAFEP
{
    /// <summary>
    /// Options available to untie qualified teams from a group stage.
    /// </summary>
    public enum GroupStageTieType
    {
        /// <summary>
        /// Overall ranking comparison.
        /// </summary>
        Ranking,
        /// <summary>
        /// Single turn knockout stage between tied teams;
        /// teams count is a power of 2 (eg no exemption);
        /// teams beyond the 2-power are eliminated, based on an overall ranking comparison.
        /// </summary>
        Mixed,
        /// <summary>
        /// Single or two turn(s) knockout stage between tied teams;
        /// every team qualified for the knockout stage;
        /// exemptions possible on first turn, based on an overall ranking comparison;
        /// </summary>
        KnockOut
    }
}
