// <copyright file="BusinessPartner.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
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
        /// Attributes class.
        /// </summary>
        public static class Atrributes
        {
            /// <summary>
            /// Country attribute.
            /// </summary>
            public const string Country = "jarvis_country";

            /// <summary>
            /// Business Partner Country.
            /// </summary>
            public const string BusinessPartnerCountry = "jarvis_address1_country";
        }
    }
}
