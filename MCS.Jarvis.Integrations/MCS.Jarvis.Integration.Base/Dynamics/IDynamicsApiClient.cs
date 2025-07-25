namespace MCS.Jarvis.Integration.Base.Dynamics
{
    using MCS.Jarvis.Integration.Base.Logging;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;

    public interface IDynamicsApiClient
    {
        /// <summary>
        /// Retrieve Result Set By Fetch Xml.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="fetchXml">fetch Xml.</param>
        /// <param name="userId">user Id.</param>
        /// <param name="fetchCount">fetch count.</param>
        /// <returns>Result set.</returns>
        public ICollection<JObject> RetrieveResultSetByFetchXml(string entitySetName, string fetchXml, string userId, int fetchCount = 5000);

        /// <summary>
        /// Retrieve Limited Result Set By Fetch XML.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="fetchXml">Fetch XML.</param>
        /// <param name="pageNumber">page Number.</param>
        /// <param name="pagingCookie">paging Cookie.</param>
        /// <param name="userId">user Id.</param>
        /// <returns></returns>
        public (ICollection<JObject> fetchResults, string? pagingCookie) RetrieveLimitedResultSetByFetchXml(string entitySetName, string fetchXml, int pageNumber, string pagingCookie, string userId);

        /// <summary>
        /// Set Logging Reference.
        /// </summary>
        /// <param name="logger"></param>
        public void SetLoggingReference(ILoggerService logger);

        /// <summary>
        /// Execute single NN associate request.
        /// </summary>
        /// <param name="firstEntitySetName">first Entity Set Name.</param>
        /// <param name="firstEntityId">first Entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="secondEntitySetName">second Entity Set Name.</param>
        /// <param name="secondEntityId">second Entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>associate request.</returns>
        public Task<bool> ExecuteNNAssociateAsync(string firstEntitySetName, string firstEntityId, string relationshipName, string secondEntitySetName, string secondEntityId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute single NN associate request.
        /// </summary>
        /// <param name="firstEntitySetName">first Entity Set Name.</param>
        /// <param name="firstEntityId">first Entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="secondEntitySetName">second Entity Set Name.</param>
        /// <param name="secondEntityId">second Entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>associate request.</returns>
        public bool ExecuteNNAssociate(string firstEntitySetName, string firstEntityId, string relationshipName, string secondEntitySetName, string secondEntityId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute single NN disassociate request.
        /// </summary>
        /// <param name="firstEntitySetName">first Entity Set Name.</param>
        /// <param name="firstEntityId">first Entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="secondEntitySetName">second Entity Set Name.</param>
        /// <param name="secondEntityId">second Entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>associate request.</returns>
        public Task<bool> ExecuteNNDisassociateAsync(string firstEntitySetName, string firstEntityId, string relationshipName, string secondEntitySetName, string secondEntityId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute single NN disassociate request.
        /// </summary>
        /// <param name="firstEntitySetName">first Entity Set Name.</param>
        /// <param name="firstEntityId">first Entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="secondEntitySetName">second Entity Set Name.</param>
        /// <param name="secondEntityId">second Entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>associate request.</returns>
        public bool ExecuteNNDisassociate(string firstEntitySetName, string firstEntityId, string relationshipName, string secondEntitySetName, string secondEntityId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Get NN associated records.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="selectQuery">select Query.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>NN associated records.</returns>
        public Task<ICollection<JObject>> GetNNAssociatedAsync(string entitySetName, string entityId, string relationshipName, string selectQuery, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Get NN associated records.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="relationshipName">relationship Name.</param>
        /// <param name="selectQuery">select Query.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>NN associated records.</returns>
        public ICollection<JObject> GetNNAssociated(string entitySetName, string entityId, string relationshipName, string selectQuery, bool parseErrorAndThrowException = true);

        /// <summary>
        /// execute batch request.
        /// </summary>
        /// <param name="httpMessageContents">HTTP Message contents.</param>
        /// <returns>return HTTP Response Message.</returns>
        public Task<HttpResponseMessage?> ExecuteBatchRequest(ICollection<HttpMessageContent> httpMessageContents);

        /// <summary>
        /// execute batch request.
        /// </summary>
        /// <param name="httpMessageContents">HTTP Message contents.</param>
        /// <returns>return HTTP Response Message.</returns>
        public Task<ICollection<HttpResponseMessage>?> ExecuteGetBatchRequest(ICollection<HttpMessageContent> httpMessageContents);

        /// <summary>
        /// Execute Fetch XML Batch.
        /// </summary>
        /// <param name="fetchXML">fetch XML.</param>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="batchName">batch Name.</param>
        /// <param name="fetchCount">fetch Count.</param>
        /// <returns></returns>
        public ICollection<JObject> ExecuteFetchXmlBatch(string fetchXML, string entitySetName, string batchName, int fetchCount = 5000);

        /// <summary>
        /// Share Record To Access Team.
        /// </summary>
        /// <param name="requestName">request Name.</param>
        /// <param name="content">content message.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>if record is successfully shared or not.</returns>
        public Task<bool> ShareRecordToAccessTeamAsync(string requestName, string content, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Share Record To Access Team.
        /// </summary>
        /// <param name="requestName">request Name.</param>
        /// <param name="content">content message.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>if record is successfully shared or not.</returns>
        public bool ShareRecordToAccessTeam(string requestName, string content, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute single create request.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>created Id.</returns>
        public Task<string?> CreateEntityAsync(string entitySetName, JObject jsonObject, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute single create request.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>created Id.</returns>
        public string? CreateEntity(string entitySetName, JObject jsonObject, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute single update request.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>return updated record ID.</returns>
        public Task<string> UpdateEntityAsync(string entitySetName, string entityId, JObject jsonObject, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute single update request.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="jsonObject">jObject value.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>return updated record ID.</returns>
        public string UpdateEntity(string entitySetName, string entityId, JObject jsonObject, bool parseErrorAndThrowException = true);

        /// <summary>
        /// create or update entity object.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="compositeKey">composite Key.</param>
        /// <param name="jsonObject">JSON Object.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns></returns>
        public Task<JObject?> UpsertEntityAsync(string entitySetName, string compositeKey, JObject jsonObject, bool parseErrorAndThrowException = true);

        /// <summary>
        /// create or update entity object.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="compositeKey">composite Key.</param>
        /// <param name="jsonObject">JSON Object.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns></returns>
        public JObject? UpsertEntity(string entitySetName, string compositeKey, JObject jsonObject, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute single delete request in Async.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>success boolean value.</returns>
        public Task<bool> DeleteEntityAsync(string entitySetName, string entityId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute single delete request in Async.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>success boolean value.</returns>
        public bool DeleteEntity(string entitySetName, string entityId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Retrieve record using Guid.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="columns">columns value.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>record Guid.</returns>
        public Task<JObject?> RetrieveEntityByIdAsync(string entitySetName, string entityId, string[] columns, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Retrieve record using Guid.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="entityId">entity Id.</param>
        /// <param name="columns">columns value.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>record Guid.</returns>
        public JObject RetrieveEntityById(string entitySetName, string entityId, string[] columns = null, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Retrieve Result Set By API Filter Query.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="selectQuery">select query.</param>
        /// <param name="filterQuery">filter query.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Result set by API.</returns>
        public Task<List<JObject>> RetrieveResultSetByFilterAsync(string entitySetName, string selectQuery, string filterQuery, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Retrieve Result Set By API Filter Query.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="selectQuery">select query.</param>
        /// <param name="filterQuery">filter query.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Result set by API.</returns>
        public ICollection<JObject> RetrieveResultSetByFilter(string entitySetName, string selectQuery, string filterQuery, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute global action asynchronous.
        /// </summary>
        /// <param name="requestParameter">action request input parameter.</param>
        /// <param name="actionName">action name.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>action response output parameter.</returns>
        public Task<JObject?> ExecuteGlobalActionAsync(JObject requestParameter, string actionName, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute global action asynchronous.
        /// </summary>
        /// <param name="requestParameter">action request input parameter.</param>
        /// <param name="actionName">action name.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>action response output parameter.</returns>
        public JObject? ExecuteGlobalAction(JObject requestParameter, string actionName, bool parseErrorAndThrowException = true);

        /// <summary>
        /// executes an entity bound action.
        /// </summary>
        /// <param name="requestParameter">request parameters object.</param>
        /// <param name="actionName">action name.</param>
        /// <param name="recordId">record id.</param>
        /// <param name="entitySchemaName">entity schema name.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>action response.</returns>
        public Task<JObject?> ExecuteEntityBoundActionAsync(JObject requestParameter, string actionName, string recordId, string entitySchemaName, bool parseErrorAndThrowException = true);

        /// <summary>
        /// executes an entity bound action.
        /// </summary>
        /// <param name="requestParameter">request parameters object.</param>
        /// <param name="actionName">action name.</param>
        /// <param name="recordId">record id.</param>
        /// <param name="entitySchemaName">entity schema name.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>action response.</returns>
        public JObject? ExecuteEntityBoundAction(JObject requestParameter, string actionName, string recordId, string entitySchemaName, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute Send Email.
        /// </summary>
        /// <param name="requestParameter">Request Parameter.</param>
        /// <param name="emailId">Email Id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Returns the object.</returns>
        public Task<JObject?> ExecuteSendEmailActionAsync(JObject requestParameter, string emailId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute Send Email.
        /// </summary>
        /// <param name="requestParameter">Request Parameter.</param>
        /// <param name="emailId">Email Id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Returns the object.</returns>
        public JObject? ExecuteSendEmailAction(JObject requestParameter, string emailId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute add members to team.
        /// </summary>
        /// <param name="userIdList">user list which need to be added to team.</param>
        /// <param name="teamId">team id.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>if task is successfull or not.</returns>
        public Task<bool> ExecuteAddTeamMembersToTeam(ICollection<JToken> userIdList, string teamId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute remove members from team.
        /// </summary>
        /// <param name="userIdList"></param>
        /// <param name="teamId">ser list which need to be removed to team.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>if task is successfull or not.</returns>
        public Task<bool> ExecuteRemoveTeamMembersToTeam(ICollection<JToken> userIdList, string teamId, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute Send Email.
        /// </summary>
        /// <param name="requestParameter">Request Parameter.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Returns the object.</returns>
        public Task<JObject?> ExecuteSendEmailFromTemplateActionAsync(JObject requestParameter, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Execute Send Email.
        /// </summary>
        /// <param name="requestParameter">Request Parameter.</param>
        /// <param name="parseErrorAndThrowException">parse Error And Throw Exception.</param>
        /// <returns>Returns the object.</returns>
        public JObject? ExecuteSendEmailFromTemplateAction(JObject requestParameter, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Retrieve Fetch Xml Result First Set.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="fetchXml">fetch Xml.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>fetch xml value.</returns>
        public Task<ICollection<JObject>> RetrieveTopRecordsFromFetchXmlAsync(string entitySetName, string fetchXml, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Retrieve Fetch Xml Result First Set.
        /// </summary>
        /// <param name="entitySetName">entity Set Name.</param>
        /// <param name="fetchXml">fetch Xml.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns>fetch xml value.</returns>
        public ICollection<JObject> RetrieveTopRecordsFromFetchXml(string entitySetName, string fetchXml, bool parseErrorAndThrowException = true);

        /// <summary>
        /// Retrieve limited result set by change tracking.
        /// </summary>
        /// <param name="entitySetName">entity set name.</param>
        /// <param name="selectQuery">select query.</param>
        /// <param name="deltaToken">existing paging token if any.</param>
        /// <param name="pagingCookie">paging cookie if any.</param>
        /// <param name="parseErrorAndThrowException">parse and throw exception.</param>
        /// <returns></returns>
        public (ICollection<JObject> results, string? pagingCookieVal, string? deltaTokenVal) RetrieveLimitedResultSetByChangeTracking(string entitySetName, string selectQuery, string deltaToken, string pagingCookie, bool parseErrorAndThrowException = true);
    }
}
