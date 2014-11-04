using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;

namespace ssl
{
    class Program
    {
        private enum NodeFound {    unknown, executable, parm, limit, 
                                    key, url, 
                                    mainColourFG, mainColourBG, 
                                    secondaryColourFG, secondaryColourBG,
                                    currentColourFG, currentColourBG };

        /// <summary>
        /// Holds the Subversion command line to execute.
        /// </summary>
        private static string svnCommandLine;

        /// <summary>
        /// Holds the parameters to be passed to the subversion command
        /// </summary>
        private static string svnParms = " ";

        /// <summary>
        /// Revision to work with
        /// </summary>
        private static string revision = String.Empty;

        /// <summary>
        /// Search argument
        /// </summary>
        private static string search = string.Empty;

        /// <summary>
        /// Search argument
        /// </summary>
        private static string userFilter = string.Empty;

        /// <summary>
        /// Search argument
        /// </summary>
        private static DateTime dateFilter;

        /// <summary>
        /// The repositories which the client knows about
        /// </summary>
        private static Hashtable repositories;

        /// <summary>
        /// Main Colour (foreground) to show files in
        /// </summary>
        private static System.ConsoleColor mainColourFG = System.ConsoleColor.White;

        /// <summary>
        /// Main Colour (background) to show files in
        /// </summary>
        private static ConsoleColor mainColourBG = ConsoleColor.Black;

        /// <summary>
        /// Secondary Colour (foreground) to show files in
        /// </summary>
        private static ConsoleColor secondaryColourFG = ConsoleColor.Yellow;

        /// <summary>
        /// Secondary Colour (background) to show files in
        /// </summary>
        private static ConsoleColor secondaryColourBG = ConsoleColor.Black;

        /// <summary>
        /// Current foreground colour of console
        /// </summary>
        private static ConsoleColor currentColourFG = ConsoleColor.White;

        /// <summary>
        /// Current background colour of console
        /// </summary>
        private static ConsoleColor currentColourBG = ConsoleColor.Black;

        ///// <summary>
        ///// The number of entries searched
        ///// </summary>
        //private static int totalEntries = 0;

        ///// <summary>
        ///// The number of entries matched
        ///// </summary>
        //private static int matchedEntries = 0;

        /// <summary>
        /// Indicates whether the user has asked for the help text to be displayed
        /// </summary>
        private static bool helpRequested = false;

        /// <summary>
        /// Limit the number of entries returned by subversion
        /// </summary>
        private static int limit = 0;

        /// <summary>
        /// The subversion list entries found
        /// </summary>
        private static svnentries<svnentry> entries;


        static void Main(string[] args)
        {
            Console.WriteLine("Subversion log viewer v 0.1");
            if (args.Length == 0)
            {
                Console.WriteLine("Usage ssl [repository-letter] <search>");
                return;
            }

            repositories = new Hashtable(4);        //Create a hashtable which will initially expect 4 entries
            entries = new svnentries<svnentry>();

            //Load up the ini file
            if (!readInifile())
            {
                Console.WriteLine("Cannot read from settings file ssl.xml");
                return;
            }

            if (!parseArgs(args))
            {
                return;
            }

            //If the user has asked for help then display it and quit
            if (helpRequested)
            {
                Console.WriteLine("Usage: ssl REPOS [options] [search]");
                Console.WriteLine("       The following options are available:");
                Console.WriteLine("       -uuser filter results by user (eg -umike)");
                Console.WriteLine("       -lnn limits the result to the first nn (eg -l32)");
                Console.WriteLine("       [search] is the search term to use (eg code or \"fixed bug\"");
                Console.WriteLine(" ");
                Console.WriteLine("       where REPOS is the code as defined in the ssl.xml file");

                return;
            }

            //Now, see if the repository letter entered is available
            string key = args[0];

            if (!repositories.ContainsKey(key))
            {
                Console.WriteLine("Repository " + key + " is not defined");
                Console.WriteLine("Available repositories are:");
                foreach (string thisKey in repositories.Keys)
                {
                    Console.WriteLine(thisKey + " = " + repositories[thisKey]);
                }
                return;
            }


            subversion svn = new subversion((string)repositories[key]);
            string output = string.Empty;
            string parms = svnParms;

            if (revision.Length > 0)      //If a specific revision has been requested then use it
            {
                parms += "-r " + revision + " ";
            }

            if (limit > 0)              //If a limit is required then use it
            {
                parms += "--limit " + limit.ToString() + " ";
            }

            output = svn.RunSyncAndGetResults(svnCommandLine, parms);

            Console.WriteLine("Executing command: " + svnCommandLine + parms + repositories[key]);
            Console.WriteLine("");

            entries.UserFilter = userFilter;
            entries.SearchFilter = search;
            entries.dateFilter = dateFilter;

            if (!entries.Populate(output))
            {
                Console.WriteLine("Error reading data from subversion.");
                return;
            }

            writeOutput();

            Console.WriteLine("");
        }

        private static bool readInifile()
        {
            string iniFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\ssl.xml";
            if (!File.Exists(iniFile))
            {
                return false;
            }

            XmlTextReader reader = new XmlTextReader(iniFile);

            NodeFound nf = NodeFound.unknown;

            try
            {
                string key = string.Empty;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "executable")
                        {
                            nf = NodeFound.executable;
                        }

                        if (reader.Name == "parm")
                        {
                            nf = NodeFound.parm;
                        }

                        if (reader.Name == "limit")
                        {
                            nf = NodeFound.limit;
                        }

                        if (reader.Name == "key")
                        {
                            nf = NodeFound.key;
                        }

                        if (reader.Name == "url")
                        {
                            nf = NodeFound.url;
                        }

                        if (reader.Name == "mainColourFG")
                        {
                            nf = NodeFound.mainColourFG;
                        }

                        if (reader.Name == "mainColourBG")
                        {
                            nf = NodeFound.mainColourBG;
                        }

                        if (reader.Name == "secondaryColourFG")
                        {
                            nf = NodeFound.secondaryColourFG;
                        }

                        if (reader.Name == "secondaryColourBG")
                        {
                            nf = NodeFound.secondaryColourBG;
                        }

                        if (reader.Name == "currentColourFG")
                        {
                            nf = NodeFound.currentColourFG;
                        }

                        if (reader.Name == "currentColourBG")
                        {
                            nf = NodeFound.currentColourBG;
                        }
                    }
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        switch (nf)
                        {
                            case NodeFound.executable:
                                svnCommandLine = reader.Value;
                                break;

                            case NodeFound.parm:
                                svnParms += reader.Value + " ";
                                break;

                            case NodeFound.limit:
                                try
                                {
                                    int parmLimit = System.Convert.ToInt32(reader.Value);

                                    limit = parmLimit;
                                }
                                #pragma warning disable 0168
                                catch (Exception excp)
                                {
                                    //Do nothing, leave the limit at it's default
                                }
                                #pragma warning restore
                                break;

                            case NodeFound.key:
                                key = reader.Value;
                                break;

                            case NodeFound.url:
                                repositories.Add(key, reader.Value);
                                break;

                            case NodeFound.mainColourFG:
                                try
                                {
                                    mainColourFG = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), reader.Value);
                                }
                                catch (ArgumentException) { };  //Do nothing, we will use the default

                                break;

                            case NodeFound.mainColourBG:
                                try
                                {
                                    mainColourBG = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), reader.Value);
                                }
                                catch (ArgumentException) { };  //Do nothing, we will use the default

                                break;

                            case NodeFound.secondaryColourFG:
                                try
                                {
                                    secondaryColourFG = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), reader.Value);
                                }
                                catch (ArgumentException) { };  //Do nothing, we will use the default

                                break;

                            case NodeFound.secondaryColourBG:
                                try
                                {
                                    secondaryColourBG = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), reader.Value);
                                }
                                catch (ArgumentException) { };  //Do nothing, we will use the default

                                break;

                            case NodeFound.currentColourFG:
                                try
                                {
                                    currentColourFG = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), reader.Value);
                                }
                                catch (ArgumentException) { };  //Do nothing, we will use the default

                                break;

                            case NodeFound.currentColourBG:
                                try
                                {
                                    currentColourBG = (System.ConsoleColor)Enum.Parse(typeof(System.ConsoleColor), reader.Value);
                                }
                                catch (ArgumentException) { };  //Do nothing, we will use the default

                                break;
                        }
                    }
                }
            }
            catch (XmlException excp)
            {
                Console.WriteLine("Error reading ini file: " + excp.Message);
                return false;
            }
            finally
            {
                reader.Close();
            }

            reader = null;

            //If we are missing some key data then drop out
            if (svnCommandLine.Length < 3 || svnParms.Length < 3 || repositories.Count == 0)
            {
                return false;
            }

            //If we get to here then everything is initialised correctly so return true.
            return true;
        }

        /// <summary>
        /// Parse the arguments passed in from the command line
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool parseArgs(string[] args)
        {
            Regex rxRepos = new Regex(@"^-r(\d*)([:, ]?\d*)?");
            Regex rxUser = new Regex(@"\s*-u([a-zA-Z0-9]*)\s*");
            Regex rxDate = new Regex(@"\s*-d(\d{2,4}[-\\]\d{1,2}[-\\]\d{1,2})\s*");
            Regex rxLimit = new Regex(@"\s*-l(\d*)\s*");

            foreach (string arg in args)
            {
                string ARG = arg.ToUpper();

                //If user has requested help then set this option and drop out
                if (ARG == "-H" || ARG == "--H" || ARG == "-HELP" || ARG == "--HELP")
                {
                    helpRequested = true;
                    return true;
                }

                if (arg == args[0])
                {
                    continue;       //Drop out for the first argument which is the repository (if it wasn't a help request).
                }

                //Check if the argument is a revision
                Match match = rxRepos.Match(arg);
                if (match.Success)
                {
                    if (match.Groups.Count == 1)
                    {
                        revision = match.Groups[1].Value;
                    }
                    else if (match.Groups.Count == 2)
                    {
                        revision = match.Groups[1].Value + match.Groups[2].Value;
                    }
                    else if (match.Groups.Count == 3)
                    {
                        revision = match.Groups[1].Value + match.Groups[2].Value + match.Groups[3].Value;
                    }
                    continue;
                }

                //Check if the argument is a user filter
                match = rxUser.Match(arg);
                if (match.Success && match.Groups.Count >= 1)
                {
                    userFilter = match.Groups[1].Value;
                    continue;
                }

                //Check if the argument is a limit filter
                match = rxLimit.Match(arg);
                if (match.Success && match.Groups.Count >= 1)
                {
                    try
                    {
                        int parmLimit = System.Convert.ToInt32(match.Groups[1].Value);
                        limit = parmLimit;
                    }
                    #pragma warning disable 0168
                    catch (Exception excp)
                    {
                        //Do nothing, leave the limit at it's default
                    }
                    #pragma warning restore
                    
                    continue;
                }

                //Check if the argument is a date filter
                match = rxDate.Match(arg);
                if (match.Success && match.Groups.Count >= 1)
                {
                    try
                    {
                        dateFilter = System.Convert.ToDateTime(match.Groups[1].Value + " 00:00:00");
                    }
                    #pragma warning disable 0168
                    catch (FormatException excp)
                    {
                        Console.WriteLine("Invalid date specified in date filter");
                        return false;
                    }
                    #pragma warning restore
                    continue;
                }

                search = arg;
            }

            return true;
        }

        /// <summary>
        /// Write the output to the console
        /// </summary>
        private static void writeOutput()
        {
            int rows = Console.WindowHeight;
            int columns = Console.WindowWidth;
            
            bool useMainColour = true;
            foreach (svnentry entry in entries)
            {
                if (useMainColour)
                {
                    Console.ForegroundColor = mainColourFG;
                    Console.BackgroundColor = mainColourBG;
                }
                else
                {
                    Console.ForegroundColor = secondaryColourFG;
                    Console.BackgroundColor = secondaryColourBG;
                }
                useMainColour = !useMainColour;

                //Get the string to output and pad it with spaces as necessary
                string outputLine = entry.Header + " " + entry.Detail;

                int totalLength = 0;
                while (totalLength < outputLine.Length)
                {
                    totalLength += columns;
                }

                outputLine = outputLine.PadRight(totalLength - 1, ' ');

                Console.WriteLine(outputLine);
            }

            Console.ForegroundColor = currentColourFG;
            Console.BackgroundColor = currentColourBG;

            Console.WriteLine("");
            Console.WriteLine("Searched {0} entries, found {1} matches", entries.TotalEntries, entries.MatchedEntries);
        }
    }
}
