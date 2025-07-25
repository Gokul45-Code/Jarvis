// <copyright file="CaseUserProfile.cs" company="Microsoft">
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
    /// Case User Profile.
    /// </summary>
    public static class CaseUserProfile
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "jarvis_caseuser";

        /// <summary>
        /// Class for Attribute.
        /// </summary>
        public static class Atrributes
        {
            /// <summary>
            /// Case Attribute.
            /// </summary>
            public const string Case = "jarvis_case";

            /// <summary>
            /// Business Partner.
            /// </summary>
            public const string BusinessPartner = "jarvis_businesspartner";

            /// <summary>
            /// Profile Attribute.
            /// </summary>
            public const string Profile = "jarvis_profile";

            /// <summary>
            /// User Attribute.
            /// </summary>
            public const string User = "jarvis_user";

            /// <summary>
            /// Status Attribute.
            /// </summary>
            public const string Status = "statecode";
        }
    }
}
