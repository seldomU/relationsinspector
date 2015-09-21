
namespace Stuff
{
    class Util
    {
        public static void RunSystemCmd(string cmdText)
        {
            UnityEngine.Debug.Log("running system cmd: " + cmdText);
            /*
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = @"/C " + cmdText;  // use /K to not close the window after the command was executed

            // don't put the standard output on the shell, give it to us
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            process.StartInfo = startInfo;            
            process.Start();
            //Debug.Log("result: " + process.StandardOutput.ReadToEnd());
            */
            System.Diagnostics.Process.Start("CMD.exe", @"/C " + cmdText);
        }
    }
}
