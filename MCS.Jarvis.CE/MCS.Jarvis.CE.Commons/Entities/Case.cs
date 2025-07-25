// <copyright file="Case.cs" company="Microsoft">
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
    /// Class for Case.
    /// </summary>
    public static class Case
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "incident";

        /// <summary>
        /// Class for Attribute.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            /// Customer Attribute.
            /// </summary>
            public const string Customer = "customerid";

            /// <summary>
            /// Home Dealer.
            /// </summary>
            public const string HomeDealer = "jarvis_homedealer";

            /// <summary>
            /// Breakdown Country.
            /// </summary>
            public const string BreakdownCountry = "jarvis_country";

            /// <summary>
            /// Case Type.
            /// </summary>
            public const string CaseType = "casetypecode";

            /// <summary>
            /// Service Line.
            /// </summary>
            public const string ServiceLine = "jarvis_caseserviceline";

            /// <summary>
            /// Location Attribute.
            /// </summary>
            public const string Location = "jarvis_caselocation";

            /// <summary>
            /// Case Status Update.
            /// </summary>
            public const string CaseStatusUpdate = "jarvis_casestatusupdate";

            /// <summary>
            /// First Closed.
            /// </summary>
            public const string FirstClosed = "jarvis_casestatusupdate";
        }

        /// <summary>
        /// Case Location.
        /// </summary>
        public enum CaseLocation : int
        {
            /// <summary>
            /// National Value.
            /// </summary>
            National = 334030001,

            /// <summary>
            /// International Value.
            /// </summary>
            International = 334030000,

            /// <summary>
            /// Regional Value.
            /// </summary>
            Regional = 334030002,
        }
    }
}
