﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Protocols.TestManager.Detector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Microsoft.Protocols.TestManager.SMBDPlugin.Detector
{
    partial class SmbdDetector
    {
        public bool PingSut()
        {
            logWriter.AddLog(DetectLogLevel.Information, "Ping Target SUT...");

            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default TtL value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "0123456789ABCDEF0123456789ABCDEF";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 5000;
            bool result = false;
            List<PingReply> replys = new List<PingReply>();
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    replys.Add(pingSender.Send(DetectionInfo.SUTName, timeout, buffer, options));
                }
            }
            catch (Exception ex)
            {
                logWriter.AddLog(DetectLogLevel.Warning, "Failed", false, LogStyle.StepFailed);
                logWriter.AddLog(DetectLogLevel.Information, string.Format("Error: {0}", ex));
                return false;
            }
            foreach (var reply in replys)
            {

                result |= (reply.Status == IPStatus.Success);
            }
            if (result)
            {
                logWriter.AddLog(DetectLogLevel.Warning, "Passed", false, LogStyle.StepPassed);
                return true;
            }
            else
            {
                logWriter.AddLog(DetectLogLevel.Warning, "Failed", false, LogStyle.StepFailed);
                logWriter.AddLog(DetectLogLevel.Information, "Target SUT doesn't respond.");
                return false;
            }
        }

        private IEnumerable<IPAddress> GetIpAddressListOfSut()
        {
            try
            {
                var result = Dns.GetHostAddresses(DetectionInfo.SUTName).Where(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork);
                return result;
            }
            catch (Exception ex)
            {
                logWriter.AddLog(DetectLogLevel.Information, string.Format("Cannot get SUT IP addresses: {0}.", ex));
                return new IPAddress[0];
            }
        }
    }
}
