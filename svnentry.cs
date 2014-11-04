using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ssl
{
    class svnentry
    {
        /// <summary>
        /// Return enumeration
        /// </summary>
        public enum EntryResult { Ok, LastEntryAdded, Failed };

        /// <summary>
        /// Static variable holding the length of the longest revision number found
        /// </summary>
        public static int RevisionMaxLength = 0;

        /// <summary>
        /// Static variable holding the length of the longest username found
        /// </summary>
        public static int UsernameMaxLength = 0;

        /// <summary>
        /// Class level variable which holds the largest header size
        /// </summary>
        public static int HeaderMaxSize = 0;

        #region private members
        /// <summary>
        /// Collection of lines which comprise the current entry
        /// </summary>
        private List<string> lines;

        /// <summary>
        /// The revision number of this entry.
        /// </summary>
        private string revision = string.Empty;

        /// <summary>
        /// The user who committed this revision
        /// </summary>
        private string user = string.Empty;

        /// <summary>
        /// Filter on username.
        /// </summary>
        private static string userFilter = String.Empty;

        /// <summary>
        /// Date and time that this entry was committed to the repository.
        /// </summary>
        private DateTime commitDate = new DateTime();

        //Regular Expressions for parsing the output from the svn log command
        private Regex rxSeparator = new Regex(@"^-----");
        private Regex rxBlankLine = new Regex(@"(^\s*$)");
        private Regex rxRevision = new Regex(@"^r(\d+)");
        private Regex rxUser = new Regex(@"^r(\d+)\s\|\s([a-zA-Z0-9]*)\s\|\s(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})");

        #endregion

        #region public members

        /// <summary>
        /// The revision number of this entry.
        /// </summary>
        public string Revision
        {
            get { return this.revision; }
        }

        #endregion

        /// <summary>
        /// Construct a new svnentry.
        /// </summary>

        public svnentry()
        {
            lines = new List<string>();
        }

        /// <summary>
        /// Examines the line passed in to see whether it belongs in this entry and adds it if it does.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public EntryResult AddToEntry(string line)
        {
            //Is this a separator line? If so, and this is not the first line found then this entry is complete
            Match match;
            match = rxSeparator.Match(line);
            if (match.Success)
            {
                if (lines.Count == 0)
                {
                    return EntryResult.Ok;
                }
                else
                {
                    return EntryResult.LastEntryAdded;
                }
            }

            //Is this a blank line? If so, ignore it
            match = this.rxBlankLine.Match(line);
            if (match.Success)
            {
                return EntryResult.Ok;
            }

            //If it wasn't a separator line or a blank line then extract the revision number and user (if present)
            match = this.rxUser.Match(line);
            if (match.Success)
            {
                if (match.Groups.Count >= 3)
                {
                    this.revision = match.Groups[1].Value;
                    if (this.revision.Length > svnentry.RevisionMaxLength)
                    {
                        svnentry.RevisionMaxLength = this.revision.Length;  //Store the length of the longest revision number
                    }
                    this.user = match.Groups[2].Value;
                    if (this.user.Length > svnentry.UsernameMaxLength)
                    {
                        svnentry.UsernameMaxLength = this.user.Length;  //Store the length of the longest username
                    }

                    try
                    {
                        this.commitDate = System.Convert.ToDateTime(match.Groups[3].Value);
                    }
                    #pragma warning disable 0168
                    catch (ArgumentException exception)
                    {
                        //If the conversion fails then abort
                        return EntryResult.Failed;
                    }
                    #pragma warning restore
                }
                return EntryResult.Ok;
            }

            //If we get here it should just be a regular line
            lines.Add(line);
            return EntryResult.Ok;
        }

        /// <summary>
        /// Returns the Header line from this entry
        /// </summary>
        public string Header
        {
            get
            {
                string thisHeader = this.revision.PadLeft(svnentry.RevisionMaxLength) + " " + this.user.PadRight(svnentry.UsernameMaxLength) + " " + this.commitDate;
                if (thisHeader.Length > svnentry.HeaderMaxSize)
                {
                    svnentry.HeaderMaxSize = thisHeader.Length;
                }

                return thisHeader;
            }
        }

        /// <summary>
        /// Returns the detail line(s) from this entry
        /// </summary>
        public string Detail
        {
            get
            {
                if (lines.Count == 0)
                {
                    return "Unknown";
                }
                else
                {
                    int lineTracker = 1;
                    StringBuilder returnSb = new StringBuilder();
                    lines.ForEach(delegate(string line)
                        {
                            if (lineTracker != 1)
                            {
                                line.PadLeft(line.Length + svnentry.HeaderMaxSize);
                            }
                            lineTracker++;
                            returnSb.Append(line);
                        });

                    return returnSb.ToString();
                }
            }
        }

        public bool Match(string userFilter, string search, DateTime dateFilter)
        {
            bool matched = true;

            //Check the user filter
            if (userFilter.Length > 0 && userFilter != user)
            {
                return false;
            }

            if (search.Length > 0)
            {
                matched = searchEntry(search);
            }

            if (!matched)
            {
                return false;
            }

            if (dateFilter != DateTime.MinValue && dateFilter.Date != this.commitDate.Date)
            {
                matched = false;
            }

            return matched;
        }

        private bool searchEntry(string search)
        {
            bool found = false;

            lines.ForEach(delegate(string line)
            {
                if (line.ToUpper().Contains(search.ToUpper()))
                {
                    found = true;
                }
            });

            return found;
        }
    }
}
