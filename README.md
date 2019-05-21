# Scribe Labs - System.IO
On-Premise community connector for Scribe using .NET System.IO namespace

| Entity | Description | Query | Create | Move/Rename | Append |
| --- | --- | --- | --- | --- |  --- |
| `Folder` | Lists all files in a folder *or subfolder* based on a filename filter (which supports *wildcards*) | **X** | **X** | **X** |  - | 
| `File` | Create and read a file by *text, line, or byte(s)* |**X**| **X** | **X** |  **X** | 

Use this connector with Text or XML connectors to easily process multiple files in the same map. 

You can also use this connector with Salesforce or Microsoft Dynamics to move attachments.
