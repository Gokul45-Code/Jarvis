// <copyright file="PassOut.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Commons.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Pass Out.
    /// </summary>
    public static class PassOut
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "jarvis_passout";

        /// <summary>
        /// Status enum.
        /// </summary>
        public enum Status : int
        {
            /// <summary>
            /// Active Value.
            /// </summary>
            Active = 0,

            /// <summary>
            /// InActive Value.
            /// </summary>
            InActive = 1,
        }

        /// <summary>
        /// Status Reason.
        /// </summary>
        public enum StatusReason : int
        {
            /// <summary>
            /// Draft Value.
            /// </summary>
            Draft = 1,

            /// <summary>
            /// To Be Sent Value.
            /// </summary>
            ToBeSent = 334030001,

            /// <summary>
            /// Sent Value.
            /// </summary>
            Sent = 334030002,

            /// <summary>
            /// Cancelled Value.
            /// </summary>
            Cancelled = 2,
        }

        /// <summary>
        /// Class for Attribute.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            /// Case Attribute.
            /// </summary>
            public const string Case = "jarvis_incident";

            /// <summary>
            /// Business Partner.
            /// </summary>
            public const string BusinessPartner = "jarvis_repairingdealer";

            /// <summary>
            /// Status Attribute.
            /// </summary>
            public const string Status = "statecode";
        }
    }
}
