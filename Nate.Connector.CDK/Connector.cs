using System;
using System.Collections.Generic;

using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Logger;
using MethodInfo = Scribe.Connector.Common.MethodInfo;

namespace ScribeLabs.Connector.SystemIO
{
    #region Connector Attributes
    [ScribeConnector(
    ConnectorTypeIdAsString,
    ConnectorTypeName,
    ConnectorTypeDescription,
    typeof(Connector),
    "", // SettingsUITypeName (obsolete)
    "", // SettingsUIVersion (obsolete)
    ConnectionUITypeName,
    ConnectionUIVersion,
    "", // XapFileName (obsolete)
    new[] { "Scribe.IS2.Target", "Scribe.MS2.Target", "Scribe.IS2.Source", "Scribe.MS2.Source" },
    SupportsCloud,
    ConnectorVersion)]
    #endregion
    
    public class Connector : IConnector
    {
        #region Constants

        internal const string ConnectorTypeName = "Scribe Labs - System.IO";

        internal const string ConnectorTypeDescription = "Scribe Labs connector that works with Folders and Files using the .NET System.IO namespace.";

        internal const string ConnectorVersion = "1.0.0";

        internal const string ConnectorTypeIdAsString = "{E5EB8923-0820-40E7-8CA8-E4FFDEF36CCD}";

        internal const string CryptoKey = "{13BFA366-C188-5GN4-9C2D-ECBDFB1892C7}";

        internal const string CompanyName = "Scribe Labs";

        internal const bool SupportsCloud = false;

        internal const string ConnectionUITypeName = "ScribeOnline.GenericConnectionUI";

        internal const string ConnectionUIVersion = "1.0";

        private MethodInfo methodInfo;
        private ConnectorService service;
        private readonly Guid connectionId;

        public Connector()
        {
            clearLocals();
            connectionId = Guid.NewGuid();
            methodInfo = new MethodInfo(GetType().Name);
        }
        private void clearLocals()
        {
            if (service != null)
            {
                service.Disconnect();
                service = null;
            }
        }

        internal static void unhandledExecptionHandler(string methodName, Exception exception)
        {
            var msg = string.Format("Unhandled exception caught in {0}: {1}\n\n", methodName, exception.Message);
            var details = string.Format("Details: {0}", exception.ToString());
            Logger.Write(Logger.Severity.Error, ConnectorTypeDescription, msg + details);
            throw new ApplicationException(msg, exception);
        }
        #endregion

        #region IConnector
        public Guid ConnectorTypeId => Guid.Parse(ConnectorTypeIdAsString);

        public bool IsConnected
        {
            get
            {
                if (service == null) return false;
                return service.IsConnected;
            }
        }

        public string PreConnect(IDictionary<string, string> properties)
        {
            using (new LogMethodExecution(ConnectorTypeDescription, methodInfo.GetCurrentMethodName()))
            {
                try
                {
                    var uiDef = ConnectionHelper.GetConnectionFormDefintion();
                    return uiDef.Serialize();
                }
                catch (Exception exception)
                {
                    unhandledExecptionHandler(methodInfo.GetCurrentMethodName(), exception);
                }
            }
            return "";
        }

        public void Connect(IDictionary<string, string> properties)
        {
            using (new LogMethodExecution(ConnectorTypeDescription + connectionId.ToString(), methodInfo.GetCurrentMethodName()))
            {
                try
                {
                    // validate & get connection properties
                    var connectionProps = ConnectionHelper.GetConnectionProperties(properties);

                    if (service == null)
                        service = new ConnectorService();

                    service.Connect(connectionProps);
                }
                catch (InvalidConnectionException)
                {
                    clearLocals();
                    throw;
                }
                catch (Exception exception)
                {
                    clearLocals();
                    unhandledExecptionHandler(methodInfo.GetCurrentMethodName(), exception);
                }
            }
        }

        public void Disconnect()
        {
            using (new LogMethodExecution(ConnectorTypeDescription, methodInfo.GetCurrentMethodName()))
            {
                try
                {
                    clearLocals();
                }
                catch (Exception exception)
                {
                    unhandledExecptionHandler(methodInfo.GetCurrentMethodName(), exception);
                }
            }
        }

        public IMetadataProvider GetMetadataProvider()
        {
            using (new LogMethodExecution(ConnectorTypeDescription, methodInfo.GetCurrentMethodName()))
            {
                if (service.GetMetadataProvider() == null)
                    throw new ApplicationException("Must connect before calling " + methodInfo.GetCurrentMethodName());

                return service.GetMetadataProvider();
            }
        }

        public IEnumerable<DataEntity> ExecuteQuery(Query query)
        {
            using (new LogMethodExecution(ConnectorTypeDescription, methodInfo.GetCurrentMethodName()))
            {
                try
                {
                    if (service == null || service.IsConnected == false)
                        throw new ApplicationException("Must connect before calling " + methodInfo.GetCurrentMethodName());

                    if (query == null)
                        throw new ArgumentNullException(nameof(query));

                    return service.ExecuteQuery(query);
                }
                catch (InvalidExecuteQueryException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    unhandledExecptionHandler(methodInfo.GetCurrentMethodName(), exception);
                }
            }
            return null;
        }

        public OperationResult ExecuteOperation(OperationInput input)
        {
            using (new LogMethodExecution(ConnectorTypeDescription, methodInfo.GetCurrentMethodName()))
            {
                try
                {
                    if (service == null || service.IsConnected == false)
                        throw new ApplicationException("Must connect before calling " +
                                                       methodInfo.GetCurrentMethodName());

                    if (input == null)
                        throw new ArgumentNullException(nameof(input));

                    if (input.Input == null)
                        throw new ArgumentException("The 'Input' property of the 'input' parameter cannot be null.", nameof(input));

                    if (!Enum.TryParse(input.Name, out ConnectorService.SupportedActions action))
                        throw new InvalidExecuteOperationException("Unsupported operation: " + input.Name);

                    if (input.Input.Length < 1)
                        throw new ArgumentException("Input must have at least one data entity.", nameof(input));

                    if (input.Input.Length > 1)
                        throw new ArgumentException("Input must cannot have more than one data entity.", nameof(input));

                    switch (action)
                    {
                        case ConnectorService.SupportedActions.Create:
                            return service.Create(input.Input[0]);
                        case ConnectorService.SupportedActions.Execute:
                            return service.Execute(input.Input[0]);
                        default:
                            throw new InvalidExecuteOperationException("Unsupported operation: " + input.Name);
                    }
                }
                catch (InvalidExecuteOperationException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    unhandledExecptionHandler(methodInfo.GetCurrentMethodName(), exception);
                }
            }
            return null;
        }

        public IEnumerable<DataEntity> ProcessMessage(string entityName, string message)
        {
            throw new NotImplementedException();
        }

        public MethodResult ExecuteMethod(MethodInput input)
        {
            throw new NotImplementedException();
        }
    }
        #endregion 
}
