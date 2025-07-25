// <copyright file="BusinessPartner.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Commons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Business Partner.
    /// </summary>
    public static class BusinessPartner
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "account";

        /// <summary>
        /// Class for attribute.
        /// </summary>
        public static class Atrributes
        {
            /// <summary>
            /// Country Value.
            /// </summary>
            public const string Country = "jarvis_country";

            /// <summary>
            /// Business Partner Country.
            /// </summary>
            public const string BusinessPartnerCountry = "jarvis_address1_country";
        }
    }
}
