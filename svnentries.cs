using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ssl
{
    /// <summary>
    /// Holds a collection of svnentry object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class svnentries<T> : List<svnentry>
    {
        #region private members
        /// <summary>
        /// Number of entries considered for inclusion in this collection
        /// </summary>
        private int totalEntries = 0;

        /// <summary>
        /// Number of entries added to this collection once filters were taken into account
        /// </summary>
        private int matchedEntries = 0;

        #endregion

        #region properties

        /// <summary>
        /// Gets the number of entries considered for inclusion in this collection before filters were applied
        /// </summary>
        public int TotalEntries
        {
            get
            {
                return this.totalEntries;
            }
        }

        /// <summary>
        /// Gets the number of entries added to this collection once filters were taken into account
        /// </summary>
        public int MatchedEntries
        {
            get
            {
                return this.matchedEntries;
            }
        }

        #endregion

        #region public members

        /// <summary>
        /// Gets/Sets the user name for filtering entries
        /// </summary>
        public string UserFilter = string.Empty;

        /// <summary>
        /// Gets/Sets the search string for filtering entries
        /// </summary>
        public string SearchFilter = string.Empty;

        /// <summary>
        /// Gets/Sets the date filter for filtering entries
        /// </summary>
        public DateTime dateFilter = DateTime.MinValue;

        #endregion
        /// <summary>
        /// Populate this collection from the string of lines passed in.
        /// </summary>
        /// <param name="lines">String containing the lines to use to initialise this collection</param>
        public bool Populate(string lines)
        {
            StringReader reader = new StringReader(lines);

            svnentry entry = new svnentry();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                svnentry.EntryResult result = entry.AddToEntry(line);

                if (result == svnentry.EntryResult.LastEntryAdded)
                {
                    totalEntries++;

                    //Add this svnentry to the list, taking into account the search and filters
                    if (entry.Match(this.UserFilter, this.SearchFilter, this.dateFilter))
                    {
                        this.Add(entry);
                        matchedEntries++;
                    }

                    entry = new svnentry();
                }
                else
                {
                    if (result == svnentry.EntryResult.Failed)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
