﻿using System;
using System.Configuration;
using System.Reflection;
using Funbit.Ets.Telemetry.Server.Helpers;

namespace Funbit.Ets.Telemetry.Server.Setup
{
    public class UrlReservationSetup : ISetup
    {
        static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        SetupStatus _status = SetupStatus.Uninstalled;
        
        public UrlReservationSetup()
        {
            try
            {
                string port = ConfigurationManager.AppSettings["Port"];
                string arguments = string.Format(@"http show urlacl url=http://+:{0}/", port);
                Log.Info("Adding ACL rule status...");
                string output = ProcessHelper.RunNetShell(arguments, "Failed to check URL ACL status");
                _status = output.Contains(port) ? SetupStatus.Installed : SetupStatus.Uninstalled;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                _status = SetupStatus.Failed;
            }
        }

        public SetupStatus Status
        {
            get { return _status; }
        }

        public SetupStatus Install()
        {
            if (_status == SetupStatus.Installed)
                return _status;

            try
            {
                // get Everyone token for the current locale
                string everyone = new System.Security.Principal.SecurityIdentifier(
                    "S-1-1-0").Translate(typeof(System.Security.Principal.NTAccount)).ToString();
                string port = ConfigurationManager.AppSettings["Port"];
                string arguments = string.Format(@"http add urlacl url=http://+:{0}/ user=\{1}", port, everyone);
                Log.Info("Adding ACL rule...");
                ProcessHelper.RunNetShell(arguments, "Failed to add URL ACL");
                _status = SetupStatus.Installed;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                _status = SetupStatus.Failed;
                throw;
            }
            return _status;
        }

        public SetupStatus Uninstall()
        {
            SetupStatus status;
            try
            {
                string port = ConfigurationManager.AppSettings["Port"];
                string arguments = string.Format(@"http delete urlacl url=http://+:{0}/", port);
                Log.Info("Deleting ACL rule...");
                ProcessHelper.RunNetShell(arguments, "Failed to delete URL ACL");
                status = SetupStatus.Uninstalled;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                status = SetupStatus.Failed;
            }
            return status;
        }
    }
}