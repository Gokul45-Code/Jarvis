// <copyright file="UserProfile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Commons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// User Profile.
    /// </summary>
    public static class UserProfile
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "jarvis_teamprofiledetails";

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
        public enum StatuReasom : int
        {
            /// <summary>
            /// Active Value.
            /// </summary>
            Active = 1,

            /// <summary>
            /// InActive Value.
            /// </summary>
            InActive = 2,
        }

        /// <summary>
        /// Class for Attribute.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            /// Name Attribute.
            /// </summary>
            public const string Name = "jarvis_name";

            /// <summary>
            /// User Attribute.
            /// </summary>
            public const string User = "jarvis_user";

            /// <summary>
            /// Profile Attribute.
            /// </summary>
            public const string Profile = "jarvis_teamprofile";

            /// <summary>
            /// CaseLocation Attribute.
            /// </summary>
            public const string CaseLocation = "jarvis_caselocation";

            /// <summary>
            /// CaseType Attribute.
            /// </summary>
            public const string CaseType = "jarvis_casetype";

            /// <summary>
            /// Country Attribute.
            /// </summary>
            public const string Country = "jarvis_country";

            /// <summary>
            /// ServiceLine Attribute.
            /// </summary>
            public const string ServiceLine = "jarvis_serviceline";

            /// <summary>
            /// Status Attribute.
            /// </summary>
            public const string Status = "statecode";

            /// <summary>
            /// StatusReason Attribute.
            /// </summary>
            public const string StatusReason = "statuscode";
        }
    }
}
