using System;
using System.Collections.Generic;
using System.Text;
using Zetetic.Chain;
using System.Diagnostics;
using NLog;

namespace Zetetic.Chain.Commands
{
    public class BatchCommand : Command
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [ChainRequired]
        public string Image { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public bool UseShell { get; set; }
        public bool ThrowError { get; set; }
        public bool KillAfterWait { get; set; }
        public int MaxWaitTime { get; set; }

        protected virtual ProcessStartInfo CreateStartInfo()
        {
            ProcessStartInfo pi = new ProcessStartInfo(this.Image, this.Arguments);
            pi.CreateNoWindow = true;
            pi.WindowStyle = ProcessWindowStyle.Hidden;

            if (!string.IsNullOrEmpty(this.WorkingDirectory))
                pi.WorkingDirectory = this.WorkingDirectory;

            pi.UseShellExecute = this.UseShell;

            logger.Debug("Proc image {0}, args {1}, wd {2}, shell {3}",
                this.Image, this.Arguments, pi.WorkingDirectory, pi.UseShellExecute);

            return pi;
        }

        public override CommandResult Execute(IContext ctx)
        {
            ProcessStartInfo pi = this.CreateStartInfo();

            using (Process p = new Process())
            {
                p.StartInfo = pi;

                bool started = p.Start();
                logger.Info("Started {0}: {1}", pi.FileName, started);

                int wtim = 30;
                long waited = 0;

                while (!p.HasExited)
                {
                    System.Threading.Thread.Sleep(wtim);

                    if ((waited += wtim) > this.MaxWaitTime && this.MaxWaitTime > 0)
                    {
                        if (this.KillAfterWait)
                            p.Kill();

                        throw new ApplicationException("Process exceeded the maximum wait time of "
                            + this.MaxWaitTime + " ms");
                    }
                }

                int rc = p.ExitCode;

                logger.Info("Exit code {0}", rc);

                ctx["LastExitCode"] = p.ExitCode;

                if (rc != 0 && this.ThrowError)
                    throw new ApplicationException("Nonzero return code");
            }

            return CommandResult.Continue;
        }
    }
}
