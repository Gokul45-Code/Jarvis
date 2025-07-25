// <copyright file="GOP.cs" company="Microsoft">
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
    /// GOP Entity.
    /// </summary>
    public static class GOPEntity
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "jarvis_gop";

        /// <summary>
        /// Enum for Status.
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
        /// Payment Type.
        /// </summary>
        public enum PaymentType : int
        {
            /// <summary>
            /// Cash Value.
            /// </summary>
            Cash = 334030000,

            /// <summary>
            /// Contract Value.
            /// </summary>
            Contract = 334030001,

            /// <summary>
            /// Credit_Card Value.
            /// </summary>
            Credit_Card = 334030002,

            /// <summary>
            /// GOP Value.
            /// </summary>
            GOP = 334030003,

            /// <summary>
            /// Warranty Value.
            /// </summary>
            Warranty = 334030005,

            /// <summary>
            /// Whitelist Value.
            /// </summary>
            Whitelist = 334030006,

            /// <summary>
            /// RD_HD Value.
            /// </summary>
            RD_HD = 334030007,
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
