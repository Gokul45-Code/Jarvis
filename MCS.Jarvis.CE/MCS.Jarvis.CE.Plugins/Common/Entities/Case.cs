// <copyright file="Case.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    /// <summary>
    /// Case Class.
    /// </summary>
    public static class Case
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "incident";

        /// <summary>
        /// Case Location.
        /// </summary>
        public enum CaseLocation : int
        {
            /// <summary>
            /// National enum.
            /// </summary>
            National = 334030001,

            /// <summary>
            /// International enum.
            /// </summary>
            International = 334030000,

            /// <summary>
            /// Regional enum.
            /// </summary>
            Regional = 334030002,
        }

        /// <summary>
        /// Attributes class.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            /// Customer attribute.
            /// </summary>
            public const string Customer = "customerid";

            /// <summary>
            /// Home dealer.
            /// </summary>
            public const string HomeDealer = "jarvis_homedealer";

            /// <summary>
            /// Break down country.
            /// </summary>
            public const string BreakdownCountry = "jarvis_country";

            /// <summary>
            /// Case type.
            /// </summary>
            public const string CaseType = "casetypecode";

            /// <summary>
            /// Service Line.
            /// </summary>
            public const string ServiceLine = "jarvis_caseserviceline";

            /// <summary>
            /// Location attribute.
            /// </summary>
            public const string Location = "jarvis_caselocation";

            /// <summary>
            /// case status update.
            /// </summary>
            public const string CaseStatusUpdate = "jarvis_casestatusupdate";

            /// <summary>
            /// First closed.
            /// </summary>
            public const string FirstClosed = "jarvis_casestatusupdate";
        }
    }
}
