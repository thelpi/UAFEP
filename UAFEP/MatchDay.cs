using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    /// <summary>
    /// Represents a match day.
    /// </summary>
    public class MatchDay
    {
        /// <summary>
        /// Collection of matches.
        /// </summary>
        public IReadOnlyCollection<Match> Matches { get; }
        /// <summary>
        /// Status; <see cref="MatchDayStatus"/>.
        /// </summary>
        public MatchDayStatus Status { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="matches">Array of <see cref="Match"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="matches"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="matches"/> is empty.</exception>
        /// <exception cref="ArgumentException">Matches list contains null.</exception>
        public MatchDay(params Match[] matches)
        {
            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }

            if (matches.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(matches), 0, "Matches list is empty.");
            }

            if (matches.Contains(null))
            {
                throw new ArgumentException("Matches list contains null.", nameof(matches));
            }

            Matches = new List<Match>(matches);
            Status = MatchDayStatus.Pending;
        }

        /// <summary>
        /// Plays every non-played match of the instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">Already played.</exception>
        public void Play()
        {
            if (Status == MatchDayStatus.Complete)
            {
                throw new InvalidOperationException("Already played.");
            }

            foreach (var match in Matches.Where(m => !m.Played))
            {
                match.Play();
            }
            Status = MatchDayStatus.Complete;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" || ", Matches);
        }

        /// <summary>
        /// Creates a new instance by reversing every <see cref="Match"/> of the current instance.
        /// </summary>
        /// <returns>Instance of <see cref="MatchDay"/>.</returns>
        public MatchDay Reverse()
        {
            return new MatchDay(
                Enumerable.Range(0, Matches.Count).Select(i =>
                    Matches.ElementAt(i).IsExempt
                        ? new Match(Matches.ElementAt(i).Team1)
                        : new Match(Matches.ElementAt(i).Team2, Matches.ElementAt(i).Team1, false)
                ).ToArray());
        }
    }
}
