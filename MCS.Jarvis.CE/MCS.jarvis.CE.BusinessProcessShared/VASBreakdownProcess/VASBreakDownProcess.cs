// <copyright file="VASBreakDownProcess.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.jarvis.CE.BusinessProcessShared.VASBreakdownProcess
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MCS.Jarvis.CE.Plugins;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// VAS Breakdown Process.
    /// </summary>
    public class VASBreakDownProcess
    {
        /// <summary>
        /// set State BPF.
        /// </summary>
        /// <param name="target">target param.</param>
        /// <param name="service">service param.</param>
        /// <param name="statuscode">status code param.</param>
        /// <param name="statecode">state code param.</param>
        public void setStateBPF(Entity target, IOrganizationService service, int statuscode, int statecode)
        {
            EntityCollection getcaseBPFProcess = service.RetrieveMultiple(new FetchExpression(string.Format(Constants.FetchXmls.getCaseBPFInstance, target.Id)));
            if (getcaseBPFProcess != null && getcaseBPFProcess.Entities.Count == 1)
            {
#pragma warning disable SA1123 // Do not place regions within elements
                #region Activate/Deactivate BPF

                Entity vasBreakDownProcess = new Entity(getcaseBPFProcess.Entities[0].LogicalName);
#pragma warning restore SA1123 // Do not place regions within elements
                vasBreakDownProcess.Id = getcaseBPFProcess.Entities[0].Id;
                vasBreakDownProcess["statecode"] = new OptionSetValue(statecode);
                vasBreakDownProcess["statuscode"] = new OptionSetValue(statuscode);

                // UpdateRequest updateBPFRequest = new UpdateRequest()
                // {
                //    Target = vasBreakDownProcess,
                // };
                // service.Execute(updateBPFRequest);
                service.Update(vasBreakDownProcess);

                #endregion
            }
        }
    }
}
