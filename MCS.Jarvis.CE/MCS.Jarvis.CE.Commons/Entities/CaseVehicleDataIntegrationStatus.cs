// <copyright file="CaseVehicleDataIntegrationStatus.cs" company="Microsoft">
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
    /// Case Vehicle Data Integration Status.
    /// </summary>
    public static class CaseVehicleDataIntegrationStatus
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "jarvis_casevehicledataintegrationstatus";

        /// <summary>
        /// Vehicle Record Type.
        /// </summary>
        public enum VehicleRecordType : int
        {
            /// <summary>
            /// Soft Offer.
            /// </summary>
            SoftOffer = 334030000,

            /// <summary>
            /// Warranty Attribute.
            /// </summary>
            Warranty = 334030001,

            /// <summary>
            /// Contract Attribute.
            /// </summary>
            Contract = 334030002,

            /// <summary>
            /// WhiteList Attribute.
            /// </summary>
            WhiteList = 334030003,
        }

        /// <summary>
        /// Progress Status.
        /// </summary>
        public enum ProgressStatus : int
        {
            /// <summary>
            /// Not Started.
            /// </summary>
            NotStarted = 334030000,

            /// <summary>
            /// In progress.
            /// </summary>
            Inprogress = 334030001,

            /// <summary>
            /// Completed Value.
            /// </summary>
            Completed = 334030002,

            /// <summary>
            /// Failed Value.
            /// </summary>
            Failed = 334030003,
        }
    }
}
