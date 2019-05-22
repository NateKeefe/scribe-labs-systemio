# Scribe Labs - System.IO
On-Premise community connector for Scribe using .NET System.IO namespace

### Use-cases
Use this connector with Text or XML connectors to easily process multiple files in the same map. 

You can also use this connector with Salesforce, Microsoft Dynamics, SQL Server, or Tools (Base64 Encode/Decode Binary) to move attachments.

| Entity | Description | Query | Create | Move/Rename | Append |
| --- | --- | --- | --- | --- |  --- |
| `Folder` | Lists all files, included hidden files, in a folder *or subfolder* based on a filename filter| **X** | **X** | **X** |  - | 
| `File` | Create and read a file by *text, line, or byte(s)* |**X**| **X** | **X** |  **X** | 

### Folder Filtering
**Search Path** is the full directory to your folder, such as: **C:\Users\nkeefe\Desktop\\**

**Search Pattern** is the file name, such as **X12.txt**. You may also use the  * *wildcard*, to return multiple files, such as **X&ast;.txt** for all files starting with *X* and ending in *.txt*.

**Search Option** supports returning files in the **TopDirectoryOnly** or **AllDirectories**. When NULL, connector will default to *TopDirectoryOnly*.

Filtering is case-insensitive.
