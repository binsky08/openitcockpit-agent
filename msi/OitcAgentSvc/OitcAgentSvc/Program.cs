﻿using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace OitcAgentSvc
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new OitcAgentSvc()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
