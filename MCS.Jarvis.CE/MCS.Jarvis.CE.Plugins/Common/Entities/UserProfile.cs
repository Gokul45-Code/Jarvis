// <copyright file="UserProfile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.Plugins
{
    /// <summary>
    /// User Profile.
    /// </summary>
    public static class UserProfile
    {
        /// <summary>
        /// Entity Logical name.
        /// </summary>
        public const string EntityLogicalName = "jarvis_teamprofiledetails";

        /// <summary>
        /// Status enum.
        /// </summary>
        public enum Status : int
        {
            /// <summary>
            /// active enum.
            /// </summary>
            Active = 0,

            /// <summary>
            /// inactive enum.
            /// </summary>
            InActive = 1,
        }

        /// <summary>
        /// Status Reason.
        /// </summary>
        public enum StatuReasom : int
        {
            /// <summary>
            /// active enum.
            /// </summary>
            Active = 1,

            /// <summary>
            /// inactive enum.
            /// </summary>
            InActive = 2,
        }

        /// <summary>
        /// Attribute class.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            /// Name attribute.
            /// </summary>
            public const string Name = "jarvis_name";

            /// <summary>
            /// User attribute.
            /// </summary>
            public const string User = "jarvis_user";

            /// <summary>
            /// Profile attribute.
            /// </summary>
            public const string Profile = "jarvis_teamprofile";

            /// <summary>
            /// case location.
            /// </summary>
            public const string CaseLocation = "jarvis_caselocation";

            /// <summary>
            /// case type.
            /// </summary>
            public const string CaseType = "jarvis_casetype";

            /// <summary>
            /// country attribute.
            /// </summary>
            public const string Country = "jarvis_country";

            /// <summary>
            /// Service line.
            /// </summary>
            public const string ServiceLine = "jarvis_serviceline";

            /// <summary>
            /// status attribute.
            /// </summary>
            public const string Status = "statecode";

            /// <summary>
            /// Status Reason.
            /// </summary>
            public const string StatusReason = "statuscode";
        }
    }
}
