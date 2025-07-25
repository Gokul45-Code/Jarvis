// <copyright file="AccountProcess.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.Account
{
    using System.Linq;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using static MCS.Jarvis.CE.BusinessProcessesShared.Helpers.Constants;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountProcess"/> class.
    /// </summary>
    public class AccountProcess
    {
        /// <summary>
        /// SetSupportedCurrency.
        /// </summary>
        /// <param name="externalCurrency">External Currency.</param>
        /// <param name="adminService">Organization Service.</param>
        /// <returns>Entity Reference.</returns>
        public EntityReference SetSupportedCurrency(EntityReference externalCurrency, IOrganizationService adminService)
        {
            EntityCollection getCurrencyMappings = adminService.RetrieveMultiple(new FetchExpression(string.Format(FetchXmls.CurrencyMapping, externalCurrency.Id)));
            EntityCollection getDefaultCurrency = adminService.RetrieveMultiple(new FetchExpression(FetchXmls.DefaultEuroCurrency));
            if (getCurrencyMappings != null && getCurrencyMappings.Entities.Count > 0)
            {
                Entity currencyMapping = getCurrencyMappings.Entities.First();
                if (currencyMapping != null && currencyMapping.Attributes.Contains("jarvis_vassupportedcurrency") && currencyMapping.Attributes["jarvis_vassupportedcurrency"] != null)
                {
                    return (EntityReference)currencyMapping.Attributes["jarvis_vassupportedcurrency"];
                }
                else
                {
                    if (getDefaultCurrency != null && getDefaultCurrency.Entities.Count > 0)
                    {
                        return getDefaultCurrency.Entities.First().ToEntityReference();
                    }
                }
            }

            if (getDefaultCurrency != null && getDefaultCurrency.Entities.Count > 0)
            {
                return getDefaultCurrency.Entities.First().ToEntityReference();
            }

            return null;
        }
    }
}