using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Metadata;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Connector.Common.Reflection.Data;

using ScribeLabs.Connector.SystemIO.Objects;

namespace ScribeLabs.Connector.SystemIO
{
    class ConnectorService
    {
        #region Instantiation 
        private Reflector reflector;
        public bool IsConnected { get; set; }
        public Guid ConnectorTypeId { get; }
        #endregion

        #region Connection
        public enum SupportedActions
        {
            Query,
            Create,
            Execute
        }

        public void Connect(ConnectionHelper.ConnectionProperties connectionProps)
        {
            reflector = new Reflector(Assembly.GetExecutingAssembly());
            IsConnected = true;
        }

        public void Disconnect()
        {
            IsConnected = false;
        }
        #endregion

        #region Operations
        public OperationResult Execute(DataEntity dataEntity)
        {
            var entityName = dataEntity.ObjectDefinitionFullName;
            var operationResult = new OperationResult();

            switch (entityName)
            {
                case EntityNames.MoveOrRename:
                    return MoveOrRename(dataEntity, entityName);
                case EntityNames.CreateFolder:
                case EntityNames.DeleteFolder:

                case EntityNames.AppendText:
                case EntityNames.AppendText2:
                    return WriteAll(dataEntity, entityName);
                default:
                    throw new ArgumentException($"{entityName} is not supported for Execute.");
            }
        }

        private OperationResult FolderSpecial(DataEntity dataEntity, string entityName)
        {
            var operationResult = new OperationResult();
            var output = new DataEntity(entityName);
            dataEntity.Properties.TryGetValue("SearchPath", out var SearchPath);
            try
            {
                switch (entityName)
                {
                    case EntityNames.CreateFolder:
                        Directory.CreateDirectory(SearchPath.ToString());
                        break;
                }
                return new OperationResult
                {
                    ErrorInfo = new ErrorResult[] { null },
                    ObjectsAffected = new[] { 1 },
                    Output = new[] { output },
                    Success = new[] { true }
                };
            }
            catch (IOException ioEx)
            {
                var errorResult = new ErrorResult();
                errorResult.Description = ioEx.Message;

                return new OperationResult
                {
                    ErrorInfo = new ErrorResult[] { errorResult },
                    ObjectsAffected = new[] { 0 },
                    Success = new[] { false }
                };
            }
        }

        private OperationResult MoveOrRename(DataEntity dataEntity, string entityName)
        {
            var operationResult = new OperationResult();
            var output = new DataEntity(entityName);
            dataEntity.Properties.TryGetValue("SearchFullPath", out var SearchFullPath);
            try
            {
                switch (entityName)
                {
                    case EntityNames.MoveOrRename:
                        dataEntity.Properties.TryGetValue("TargetFullPath", out var TargetFullPath);
                        File.Move(SearchFullPath.ToString(), TargetFullPath.ToString());
                        break;
                }
                return new OperationResult
                {
                    ErrorInfo = new ErrorResult[] { null },
                    ObjectsAffected = new[] { 1 },
                    Output = new[] { output },
                    Success = new[] { true }
                };
            }
            catch (IOException ioEx)
            {
                var errorResult = new ErrorResult();
                errorResult.Description = ioEx.Message;

                return new OperationResult
                {
                    ErrorInfo = new ErrorResult[] { errorResult },
                    ObjectsAffected = new[] { 0 },
                    Success = new[] { false }
                };
            }
        }

        public OperationResult Create(DataEntity dataEntity)
        {
            var entityName = dataEntity.ObjectDefinitionFullName;
            var operationResult = new OperationResult();

            switch (entityName)
            {
                case EntityNames.WriteBytes:
                case EntityNames.WriteText:
                    return WriteAll(dataEntity, entityName);
                default:
                    throw new ArgumentException($"{entityName} is not supported for Create.");
            }
        }

        private OperationResult WriteAll(DataEntity dataEntity, string entityName)
        {
            var operationResult = new OperationResult();
            var output = new DataEntity(entityName);

            dataEntity.Properties.TryGetValue("TargetFullPath", out var TargetFullPath);

            try
            {
                switch (entityName)
                {
                    case EntityNames.WriteBytes:
                        var Bytes = dataEntity.Properties["Bytes"] as Byte[];
                        File.WriteAllBytes(TargetFullPath.ToString(), Bytes);
                        break;
                    case EntityNames.WriteText:
                        dataEntity.Properties.TryGetValue("Text", out var Text);
                        File.WriteAllText(TargetFullPath.ToString(), Text.ToString());
                        break;
                    case EntityNames.AppendText:
                    case EntityNames.AppendText2:
                        dataEntity.Properties.TryGetValue("Text", out var appText);
                        File.AppendAllText(TargetFullPath.ToString(), appText.ToString());
                        break;
                }

                return new OperationResult
                {
                    ErrorInfo = new ErrorResult[] { null },
                    ObjectsAffected = new[] { 1 },
                    Output = new[] { output },
                    Success = new[] { true }
                };
            }
            catch (IOException ioEx)
            {
                var errorResult = new ErrorResult();
                errorResult.Description = ioEx.Message;

                return new OperationResult
                {
                    ErrorInfo = new ErrorResult[] { errorResult },
                    ObjectsAffected = new[] { 0 },
                    Success = new[] { false }
                };
            }
        }

        #endregion

        #region Query
        public IEnumerable<DataEntity> ExecuteQuery(Query query)
        {
            var entityName = query.RootEntity.ObjectDefinitionFullName;

            switch (entityName)
            {
                case EntityNames.Files:
                    var folderFiles = FiltersToFolderFiles(query);
                    return FileValuesToDE(folderFiles, entityName);
                case EntityNames.FileText:
                case EntityNames.FileBytes:
                    return ReadAnyFile(query);
                case EntityNames.FileLines:
                    return ReadAllRows(query);
                default:
                    throw new InvalidExecuteQueryException(
                        $"The {entityName} entity is not supported for query.");
            }
        }

        private static Dictionary<string, object> BuildConstraintDictionary(Expression queryExpression)
        {
            var constraints = new Dictionary<string, object>();

            if (queryExpression == null)
                return constraints;

            if (queryExpression.ExpressionType == ExpressionType.Comparison)
            {
                // only 1 filter
                addCompEprToConstraints(queryExpression as ComparisonExpression, ref constraints);
            }
            else if (queryExpression.ExpressionType == ExpressionType.Logical)
            {
                // Multiple filters
                addLogicalEprToConstraints(queryExpression as LogicalExpression, ref constraints);
            }
            else
                throw new InvalidExecuteQueryException("Unsupported filter type: " + queryExpression.ExpressionType.ToString());

            return constraints;
        }

        private static void addLogicalEprToConstraints(LogicalExpression exp, ref Dictionary<string, object> constraints)
        {
            if (exp.Operator != LogicalOperator.And)
                throw new InvalidExecuteQueryException("Unsupported operator in filter: " + exp.Operator.ToString());

            if (exp.LeftExpression.ExpressionType == ExpressionType.Comparison)
                addCompEprToConstraints(exp.LeftExpression as ComparisonExpression, ref constraints);
            else if (exp.LeftExpression.ExpressionType == ExpressionType.Logical)
                addLogicalEprToConstraints(exp.LeftExpression as LogicalExpression, ref constraints);
            else
                throw new InvalidExecuteQueryException("Unsupported filter type: " + exp.LeftExpression.ExpressionType.ToString());

            if (exp.RightExpression.ExpressionType == ExpressionType.Comparison)
                addCompEprToConstraints(exp.RightExpression as ComparisonExpression, ref constraints);
            else if (exp.RightExpression.ExpressionType == ExpressionType.Logical)
                addLogicalEprToConstraints(exp.RightExpression as LogicalExpression, ref constraints);
            else
                throw new InvalidExecuteQueryException("Unsupported filter type: " + exp.RightExpression.ExpressionType.ToString());
        }

        private static void addCompEprToConstraints(ComparisonExpression exp, ref Dictionary<string, object> constraints)
        {
            if (exp.Operator != ComparisonOperator.Equal)
                throw new InvalidExecuteQueryException($"The {exp.Operator} operator is not supported for the {exp.LeftValue.Value} field. Only the Equals operator is supported.");

            var constraintKey = exp.LeftValue.Value.ToString();
            if (constraintKey.LastIndexOf(".") > -1)
            {
                // need to remove "objectname." if present
                constraintKey = constraintKey.Substring(constraintKey.LastIndexOf(".") + 1);
            }
            constraints.Add(constraintKey, exp.RightValue.Value.ToString());
        }

        public static string[] FiltersToFolderFiles(Query query)
        {
            //Get Filters
            var constraints = BuildConstraintDictionary(query.Constraints);

            constraints.TryGetValue("SearchPath", out var folder);
            if (folder == null) { throw new InvalidExecuteQueryException("Missing Folder filter."); }
            if (folder.ToString().EndsWith("\\") == false) { folder = folder + "\\".ToString(); }

            constraints.TryGetValue("SearchPattern", out var SearchPattern);
            if (SearchPattern == null) { SearchPattern = "*"; }

            constraints.TryGetValue("SearchOption", out var SearchOption);
            if (SearchOption == null || !Enum.TryParse<SearchOption>(SearchOption.ToString(), out var searchOptionResult)) { searchOptionResult = System.IO.SearchOption.TopDirectoryOnly; }
            
            //Get File List
            string[] dirs = null;
            try { dirs = Directory.GetFiles(folder.ToString(), SearchPattern.ToString(), searchOptionResult) ?? null; }
            catch { }
            return dirs;
        }

        public static IEnumerable<DataEntity> FileValuesToDE(string[] dirs, string entityName)
        {
            if (dirs != null)
            {
                foreach (string dir in dirs)
                {
                    var getFileName = Path.GetFileName(dir);
                    var fileExt = Path.GetExtension(dir);
                    var info = new FileInfo(dir);
                    var fileLength = info.Length;
                    var createTime = info.CreationTimeUtc;
                    var lastAccessTime = info.LastAccessTimeUtc;
                    var folder = Path.GetFileName(Path.GetDirectoryName(dir));
                    var exists = info.Exists;
                    var directoryName = info.DirectoryName;
                    var hidden = info.Attributes.HasFlag(FileAttributes.Hidden);
                    var archive = info.Attributes.HasFlag(FileAttributes.Archive);
                    var temporary = info.Attributes.HasFlag(FileAttributes.Temporary);
                    var system = info.Attributes.HasFlag(FileAttributes.System);

                    var dataEntityFolder = new DataEntity();
                    dataEntityFolder.ObjectDefinitionFullName = entityName.ToString();

                    var folderProps = new EntityProperties();
                    folderProps.Add("FullPath", dir);
                    folderProps.Add("FolderName", folder);
                    folderProps.Add("FileName", getFileName);
                    folderProps.Add("FileExtension", fileExt);
                    folderProps.Add("Exists", exists);
                    folderProps.Add("FileSize", fileLength);
                    folderProps.Add("CreatedOn", createTime);
                    folderProps.Add("AccessedOn", lastAccessTime);
                    folderProps.Add("Path", directoryName);
                    folderProps.Add("Hidden", hidden);
                    folderProps.Add("Archive", archive);
                    folderProps.Add("Temporary", temporary);
                    folderProps.Add("System", system);

                    dataEntityFolder.Properties = folderProps;

                    yield return dataEntityFolder;
                }
            }
            else
            {
                yield return new DataEntity(entityName);
            }            
        }

        public static IEnumerable<DataEntity> ReadAllRows(Query query)
        {
            var entityName = query.RootEntity.ObjectDefinitionFullName;
            var constraints = BuildConstraintDictionary(query.Constraints);
            constraints.TryGetValue("SearchFileName", out var filename);
            if (filename == null) { throw new InvalidExecuteQueryException("Missing SearchFileName filter."); }
            constraints.TryGetValue("SearchPath", out var folder);
            if (folder == null) { throw new InvalidExecuteQueryException("Missing SearchPath filter."); }
            if (folder.ToString().EndsWith("\\") == false) { folder = folder + "\\".ToString(); }
            
            switch(entityName)
            {
                case EntityNames.FileLines:
                    string[] Lines = null;
                    Lines = File.ReadAllLines(folder.ToString() + filename.ToString(),System.Text.Encoding.UTF8);
                    foreach (var l in Lines)
                    {
                        var linesDE = new DataEntity(entityName);
                        var linesProps = new EntityProperties();
                        linesProps.Add("Lines", l);
                        linesDE.Properties = linesProps;
                        yield return linesDE;
                    }
                    break;
            }

        }

        public static IEnumerable<DataEntity> ReadAnyFile(Query query)
        {
            var entityName = query.RootEntity.ObjectDefinitionFullName;
            var constraints = BuildConstraintDictionary(query.Constraints);

            constraints.TryGetValue("SearchFileName", out var filename);
            if (filename == null) { throw new InvalidExecuteQueryException("Missing SearchFileName filter."); }
            constraints.TryGetValue("SearchPath", out var folder);
            if (folder == null) { throw new InvalidExecuteQueryException("Missing SearchPath filter."); }
            if (folder.ToString().EndsWith("\\") == false) { folder = folder + "\\".ToString(); }

            var dataEntity = new DataEntity(entityName);
            var entityProperties = new EntityProperties();

            try
            {
                switch (entityName)
                {
                    case EntityNames.FileText:
                        string Text = "";
                        Text = File.ReadAllText(folder.ToString() + filename.ToString()); //add encoding option here
                        entityProperties.Add("Text", Text);
                        dataEntity.Properties = entityProperties;
                        break;
                    case EntityNames.FileBytes:
                        Byte[] Bytes = null;
                        Bytes = File.ReadAllBytes(folder.ToString() + filename.ToString());
                        entityProperties.Add("Bytes", Bytes);
                        dataEntity.Properties = entityProperties;
                        break;
                }
            }
            catch (Exception exp)
            {
                Logger.Write(Logger.Severity.Error,
                    $"Cannot find Folder or File when querying entity: {entityName}.",
                    exp.Message + exp.InnerException);
                throw new InvalidExecuteQueryException("Cannot find Folder or File: " + exp.Message);
            }

            yield return dataEntity;
        }

        #endregion

        #region Metadata
        public IMetadataProvider GetMetadataProvider()
        {
            return reflector.GetMetadataProvider();
        }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            throw new NotImplementedException();
        }

        public IObjectDefinition RetrieveObjectDefinition(string objectName, bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public void ResetMetadata()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}