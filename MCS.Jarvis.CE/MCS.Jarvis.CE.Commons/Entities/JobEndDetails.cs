// <copyright file="JobEndDetails.cs" company="Microsoft">
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
    /// Job End Details.
    /// </summary>
    public static class JobEndDetails
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "jarvis_jobenddetails";

        /// <summary>
        /// Class for Attribute.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            /// Repairing Dealer.
            /// </summary>
            public const string RepairingDealer = "jarvis_repairingdealerpassout";

            /// <summary>
            /// Actual Cause Fault.
            /// </summary>
            public const string ActualCauseFault = "jarvis_actualcausefault";

            /// <summary>
            /// Mileage Attribute.
            /// </summary>
            public const string Mileage = "jarvis_mileage";

            /// <summary>
            /// Mileage Unit.
            /// </summary>
            public const string MileageUnit = "jarvis_mileageunit";

            /// <summary>
            /// JED Update Time Stamp.
            /// </summary>
            public const string JEDUpdateTimeStamp = "jarvis_jobendupdatetimestamp";

            /// <summary>
            /// Related Case.
            /// </summary>
            public const string RelatedCase = "jarvis_incident";

            /// <summary>
            /// status code.
            /// </summary>
            public const string Statuscode = "statuscode";
        }
    }
}
