using System;
using System.Collections.Generic;
using System.Text;

namespace ssl
{
    class subversion
    {
        public string Repository;

        public subversion(string repository)
        {
            Repository = @"""" + repository + @"""";
        }

        public string RunSyncAndGetResults(string command, string parms)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(command);
            psi.Arguments = parms + " " + Repository;
            psi.RedirectStandardOutput = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            System.Diagnostics.Process proc;
            proc = System.Diagnostics.Process.Start(psi);
            /*
            System.IO.StreamReader myOutput = proc.StandardOutput;
            proc.WaitForExit();
            string output = String.Empty;
            if (proc.HasExited)
            {
                output = myOutput.ReadToEnd();
            }
            */
            string output = proc.StandardOutput.ReadToEnd();

            return output;
        }
    }
}
