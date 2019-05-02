using Scribe.Connector.Common.Reflection;
using Scribe.Connector.Common.Reflection.Actions;
using System;
using System.IO;

namespace ScribeLabs.Connector.SystemIO.Entities
{
    [ObjectDefinition(Hidden = false)]
    [Query]
    public class FileText
    {
        [PropertyDefinition(UsedInActionOutput = false, UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string SearchPath { get; set; }
        [PropertyDefinition(UsedInActionOutput = false, UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string SearchFileName { get; set; }
        //return field
        [PropertyDefinition(Description = "Opens a file, reads all lines of the file as a string, and then closes the file.")]
        public string Text { get; set; }
    }

    [ObjectDefinition(Hidden = false)]
    [Query]
    public class FileLines
    {
        [PropertyDefinition(UsedInActionOutput = false, UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string SearchPath { get; set; }
        [PropertyDefinition(UsedInActionOutput = false, UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string SearchFileName { get; set; }
        //return field
        [PropertyDefinition(Description = "Opens a file, reads all lines of the file as a row, and then closes the file.")]
        public string Lines { get; set; }
    }

    [ObjectDefinition(Hidden = false)]
    [Query]
    public class FileBytes
    {
        [PropertyDefinition(UsedInActionOutput = false, UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string SearchPath { get; set; }
        [PropertyDefinition(UsedInActionOutput = false, UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string SearchFileName { get; set; }
        //return field
        [PropertyDefinition(Description = "Opens a file, reads all lines of the file as a byte array, and then closes the file.")]
        public Byte[] Bytes { get; set; }
    }

    [ObjectDefinition(Hidden = false)]
    [Query]
    public class Files
    {
        //Filter Fields
        [PropertyDefinition(UsedInActionOutput = false, UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string SearchPath { get; set; }
        [PropertyDefinition(UsedInActionOutput = false, UsedInQueryConstraint = true, UsedInQuerySelect = false)]
        public string SearchPattern { get; set; }
        [PropertyDefinition(UsedInActionOutput = false, UsedInQueryConstraint = true, UsedInQuerySelect = false,
            Description = "Specifies whether to search the current directory only (TopDirectoryOnly), or the current directory and all subdirectories (AllDirectories). Default option is TopDirectoryOnly.")]
        public SearchOption SearchOption { get; set; }
        //Return Fields
        [PropertyDefinition(Description = "The Path to the file (i.e. C:\\Users\\admin\\Desktop).")]
        public string Path { get; set; }
        [PropertyDefinition(Description = "The name of the file, including its extension. (i.e. ImportLeads.txt).")]
        public string FileName { get; set; }
        [PropertyDefinition(Description = "The Path to the file (i.e. C:\\Users\\admin\\Desktop\\ImportLeads.txt).")]
        public string FullPath { get; set; }
        [PropertyDefinition(Description = "The current Folder name (i.e. C:\\Users\\admin\\Desktop would return Desktop).")]
        public string FolderName { get; set; }
        [PropertyDefinition(Description = "The file's extension.")]
        public string FileExtension { get; set; }
        [PropertyDefinition(Description = "Indicates where a file exists.")]
        public bool Exists { get; set; }
        [PropertyDefinition(Description = "The file size in bytes.")]
        public long FileSize { get; set; }
        [PropertyDefinition(Description = "The creation time in UTC.")]
        public DateTime CreatedOn { get; set; }
        [PropertyDefinition(Description = "The last time the file was accessed in UTC.")]
        public DateTime AccessedOn { get; set; }
        [PropertyDefinition(Description = "The file is hidden, and thus is not included in an ordinary directory listing.")]
        public bool Hidden { get; set; }
        [PropertyDefinition(Description = "The file is a candidate for backup or removal.")]
        public bool Archive { get; set; }
        [PropertyDefinition(Description = "The file is temporary. A temporary file contains data that is needed while an application is executing but is not needed after the application is finished. File systems try to keep all the data in memory for quicker access rather than flushing the data back to mass storage. A temporary file should be deleted by the application as soon as it is no longer needed.")]
        public bool Temporary { get; set; }
        [PropertyDefinition(Description = "The file is a system file. That is, the file is part of the operating system or is used exclusively by the operating system.")]
        public bool System { get; set; }
    }
}
