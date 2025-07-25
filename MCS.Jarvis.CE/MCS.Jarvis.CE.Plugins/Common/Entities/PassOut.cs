// <copyright file="PassOut.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    /// <summary>
    /// Pass out.
    /// </summary>
    public static class PassOut
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "jarvis_passout";

        /// <summary>
        /// Status Enum.
        /// </summary>
        public enum Status : int
        {
            /// <summary>
            /// Active enum.
            /// </summary>
            Active = 0,

            /// <summary>
            /// Inactive enum.
            /// </summary>
            InActive = 1,
        }

        /// <summary>
        /// Status Reason.
        /// </summary>
        public enum StatusReason : int
        {
            /// <summary>
            /// Draft enum.
            /// </summary>
            Draft = 1,

            /// <summary>
            /// To be sent.
            /// </summary>
            ToBeSent = 334030001,

            /// <summary>
            /// sent enum.
            /// </summary>
            Sent = 334030002,

            /// <summary>
            /// Cancelled enum.
            /// </summary>
            Cancelled = 2,
        }

        /// <summary>
        /// Attribute class.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            /// case attribute.
            /// </summary>
            public const string Case = "jarvis_incident";

            /// <summary>
            /// Business Partner.
            /// </summary>
            public const string BusinessPartner = "jarvis_repairingdealer";

            /// <summary>
            /// Status attribute.
            /// </summary>
            public const string Status = "statecode";
        }
    }
}
