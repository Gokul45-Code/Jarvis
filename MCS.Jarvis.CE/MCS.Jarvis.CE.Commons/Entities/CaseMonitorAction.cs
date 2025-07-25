// <copyright file="CaseMonitorAction.cs" company="Microsoft">
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
    /// Case Monitor Action.
    /// </summary>
    public static class CaseMonitorAction
    {
        /// <summary>
        /// Entity Logical Name.
        /// </summary>
        public const string EntityLogicalName = "jarvis_casemonitoraction";

        /// <summary>
        /// Class for Attributes.
        /// </summary>
        public static class Attributes
        {
            /// <summary>
            /// Action Type.
            /// </summary>
            public const string ActionType = "jarvis_actiontype";

            /// <summary>
            /// Priority Code.
            /// </summary>
            public const string PriorityCode = "prioritycode";

            /// <summary>
            /// Subject Attribute.
            /// </summary>
            public const string Subject = "subject";

            /// <summary>
            /// Follow up time.
            /// </summary>
            public const string Followuptime = "jarvis_followuptime";

            /// <summary>
            /// Follow up Date.
            /// </summary>
            public const string FollowupDate = "actualstart";
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
