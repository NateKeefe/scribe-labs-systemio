using Scribe.Connector.Common.Reflection;
using Scribe.Connector.Common.Reflection.Actions;
using System;

namespace ScribeLabs.Connector.SystemIO.Entities
{
    [ObjectDefinition(Hidden = false)]
    [Create(Name = "Execute")]
    public class MoveOrRename
    {
        [PropertyDefinition(RequiredInActionInput = true, Description = "The source directory path and filename, including extension.")]
        public string SearchFullPath { get; set; }
        [PropertyDefinition(RequiredInActionInput = true, Description = "The target directory path and filename, including extension.")]
        public string TargetFullPath { get; set; }
    }

    [ObjectDefinition(Hidden = false)]
    [Create(Name = "Execute")]
    public class CreateFolder
    {
        [PropertyDefinition(RequiredInActionInput = true, Description = "The source directory path and filename, including extension.")]
        public string SearcPath { get; set; }
    }

    [ObjectDefinition(Hidden = false)]
    [Create(Name = "Execute")]
    public class DeleteFolder
    {
        [PropertyDefinition(RequiredInActionInput = true, Description = "The source directory path and filename, including extension.")]
        public string SearchPath { get; set; }
        [PropertyDefinition]
        public bool Recursive { get; set; }
    }

    [ObjectDefinition(Hidden = false)]
    [Create(Name = "Execute")]
    public class AppendText
    {
        [PropertyDefinition(Description = "The target directory path and filename, including extension.", RequiredInActionInput = true)]
        public string TargetFullPath { get; set; }
        [PropertyDefinition(Description = "Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.", RequiredInActionInput = true)]
        public string Text { get; set; }
    }

    [ObjectDefinition(Hidden = false)]
    [Create(Name = "Execute")]
    public class AppendText2
    {
        [PropertyDefinition(Description = "The target directory path and filename, including extension.", RequiredInActionInput = true)]
        public string TargetFullPath { get; set; }
        [PropertyDefinition(Description = "Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.", RequiredInActionInput = true)]
        public string Text { get; set; }
    }

    [ObjectDefinition(Hidden = false)]
    [Create]
    public class WriteText
    {
        [PropertyDefinition(Description = "The target directory path and filename, including extension.", RequiredInActionInput = true)]
        public string TargetFullPath { get; set; }
        [PropertyDefinition(Description = "Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.", RequiredInActionInput = true)]
        public string Text { get; set; }
    }

    [ObjectDefinition(Hidden = false)]
    [Create]
    public class WriteBytes
    {
        [PropertyDefinition(Description = "The target directory path and filename, including extension.", RequiredInActionInput = true)]
        public string TargetFullPath { get; set; }
        [PropertyDefinition(Description = "Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.", RequiredInActionInput = true)]
        public Byte[] Bytes { get; set; }
    }



}