// <copyright file="InAppNotification.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MCS.Jarvis.CE.BusinessProcessShared.AppNotification
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Newtonsoft.Json;

    /// <summary>
    /// In App Notification.
    /// </summary>
    public class InAppNotification
    {
        /// <summary>
        /// Create In App notification.
        /// </summary>
        /// <param name="service">Organization Service.</param>
        /// <param name="owners">Owners Id.</param>
        /// <param name="linkdata">Link data.</param>
        /// <param name="message">Message data.</param>
        /// <param name="body">body data.</param>
        public void CreateInAppnotification(IOrganizationService service, List<Guid> owners, List<LinkData> linkdata, string message, string body)
        {
            var actions = new List<Action>();
            foreach (var item in linkdata)
            {
                actions.Add(new Action() { Title = item.Title, Data = new Data() { Url = item.Url, NavigationTarget = "inline" } });
            }

            List<Entity> entities = new List<Entity>();
            foreach (Guid item in owners)
            {
                if (item != Guid.Empty)
                {
                    Entity appNotification = new Entity("appnotification");
                    appNotification.Attributes["title"] = string.Format("{0}", message);
                    appNotification.Attributes["body"] = string.Format("{0}", body);
                    appNotification.Attributes["ownerid"] = new EntityReference("systemuser", item);
                    appNotification.Attributes["data"] = "{\"actions\":" + JsonConvert.SerializeObject(actions) + "}";
                    entities.Add(appNotification);
                }
            }

            var multipleRequest = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = false,
                    ReturnResponses = true,
                },

                // Create an empty organization request collection.
                Requests = new OrganizationRequestCollection(),
            };

            foreach (var entity in entities)
            {
                CreateRequest createRequest = new CreateRequest { Target = entity };
                multipleRequest.Requests.Add(createRequest);
            }

            // Execute all the requests in the request collection using a single web method call.
            service.Execute(multipleRequest);
        }

        /// <summary>
        /// Action class.
        /// </summary>
        internal class Action
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Action"/> class.
            /// </summary>
            public Action()
            {
                this.Data = new Data();
            }

            /// <summary>
            /// Gets or sets title property.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets data property.
            /// </summary>
            public Data Data { get; set; }
        }

        /// <summary>
        /// Data class.
        /// </summary>
        internal class Data
        {
            /// <summary>
            /// Gets or sets url property.
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets navigation Target.
            /// </summary>
            public string NavigationTarget { get; internal set; }
        }
    }
}
