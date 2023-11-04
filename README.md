# MS Power Platform Dataverse Archive Service
This repo is my experiment to see if I could create a service to copy the structure of the table in Dataverse in an Azure SQL Database, copy the data from the Dataverse to SQL on a schedule, and mimic the Dataverse Audit Log functionality by tracking changes to a table. Then I thought I would create a web site using MVC, Blazor, and EntraId authentication.

For the most part my experiment succeded. I was able to copy the structure of the table, copy data on a scheduled basis, and capture changes from a table. The management app lists all the tables and when a tables is added to the archive or the change log the appropriate plugin steps are added in Dataverse.

## A couple of pending issues:
1. The columns returned by the metadata info for a table and the columns returned by a retrieve are slightly different. There are some virtual columns that included with a retrieve.
2. Some of the Dataverse datatypes do not map inherintly to .Net and SQL datatypes. 
3. To really make this useful it would need the ability to restore a record at a specific state. That functionality is not in this code.

## Current Status
These issues can be worked around but I simple lost the time to be able to put toward this effort, and since the orginal experiment proved to be fairly successful I have not continued with further development of the project. Also I simply do not have the time to keep up with the pace of change on the Power Platform as a lone side project. I  consider this work to be a decent example of some pretty fun .Net code and use of the Dataverse SDK. I hope it can be of use to someone.

This code is stricly sample code I do not consider it production ready by any means. So, there is no implied warranty and any use of this code is at your own risk.

# Solution Structure
## Projects
There are four projects in the solution:
1. ArchiveService.Management.App - MVC Blazor web application.
2. ArchiveService.Shared - Obviously shared code between the project.
3. ArchiveService.Publish - Azure Function app that recieves the 'Publish', and 'PublishAll' events of Dataverse and scheduled function to trigger the daily archive.
4. ArchiveService.Dataverse.Api - Azure Function app that receives recieves tables updates and schedule archive.

### Azure Functions
The Azure Function code is very light, all logic is in the ArchiveService.Shared assembly.

#### Startup Classes
Comparing the two Azure Function Startup classes you will notice the class that is registered for the shared interfaces are different. In the [ArchiveService.Publish.Startup](/src/Archive%20Service/ArchiveService.Publish/Startup.cs) the class is the 'SendToQueue' class, but in the [ArchiveService.Dataverse.Api.Startup](/src/Archive%20Service/ArchiveService.Api/Startup.cs) class the registered classes are the 'Manager' classes with all the business logic. 

#### Use of System Managed Identity
One thing you will not find in the code or the App.settings files is a passwordword or a app secret. Both of the Azure Function Apps use a system managed identity. There are many advantages of using the managed identity, highest on my list is the lack of app secrets to store or expire.

#### Dataverse Authentication
The ArchiveService.Dataverse.Api uses the managed identity to authenticate to Dataverse. The SDK ServiceClient class is created in the startup with a method to manage the token. I can't take all the credit for this code, that goes to [Dreaming in CRM](https://dreamingincrm.com/2021/11/16/connecting-to-dataverse-from-function-app-using-managed-identity/). 



### ArchiveService.Management.App
This website is an MVC, Blazor appliction that manages which tables are tracked on the daily archive and the change log functionality. Authentication is using Entra Id. at the moment it simply validates the user is in the Entra Id domain, I had thought to eventually make it a security group but didn't get there.

### Archive Service Shared
Most of the business logic for ArchiveService.Publish and ArchiveService.Dataverse.Api are in this assembly. 
#### Use of Interfaces and Dependency Injection
The use of Interfaces and Dependency Injection allows the PublishManager and the TableDailyArchiveManager to either push messages to a queue for further processing or handle all the processing synchronously. Using the approach it would possible to execute the PublishManager and TableDailyArchiveManager in a console app if it was necessary to write to an on premise version of SQL for instance.

#### Publish Manager
The [Publish Manager](/src/Archive%20Service/ArchiveService.Shared/Managers/PublishManager/PublishManager.cs) receives the 'Publish' message from Dataverse and then dependening on if the tables already exists in the archive uses the IAlterTableHelper or the ICreateTableHelper that is injected.  

#### DailyArchiveManager
The [DailyArchiveManager](/src/Archive%20Service/ArchiveService.Shared/Managers/DailyArchive/DailyArchiveManager.cs) queries the SQL management table to determine which tables should be archived on a daily bases and uses the ITableDailyArchiveManager that is injected which either drops a message into a queue or processes the table synchronously.

#### TableDailyArchiveManager 
The [TableDailyArchiveManager](/src/Archive%20Service/ArchiveService.Shared/Managers/TableDailyArchive/TableDailyArchiveManager.cs) uses the 'RetrieveEntityChangesRequest' of Dataverse to get the changes since the last token or does an initial sync. For each record a paramterized SQL update or insert statment is generated based on the columns returned.

[TableDailyArchiveSendToQueue](/src/Archive%20Service/ArchiveService.Shared/Managers/SendToQueueBase.cs) simply sends a message to a queue.

#### CreateTable
The [CreateTableProcessor](/src/Archive%20Service/ArchiveService.Shared/Managers/CreateTable/CreateTableProcessor.cs) parses the metadata returned from Dataverse and attempts to recreate a similiar table in the SQL database. 

The [CreateTableSendToQueue](/src/Archive%20Service/ArchiveService.Shared/Managers/CreateTable/CreateTableSendToQueue.cs) simply sends a message to a queue.

#### AlterTable
The [AlterTableProcessor](/src/Archive%20Service/ArchiveService.Shared/Managers/AlterTable/AlterTableProcessor.cs) attempts to compare the structure of the table in SQL to the metadata from Dataverse and generate an 'alter table' statment to add columns. I even added logic to handle a scenario where someone deletes a column in Dataverse and recreates it with a different datatype.

The [AlterTableSendToQueue](/src/Archive%20Service/ArchiveService.Shared/Managers/AlterTable/AlterTableSendToQueue.cs) simple sends a message into a queue.

#### ChangeLogManager
The [ChangeLogManager](/src/Archive%20Service/ArchiveService.Shared/Managers/ChangeLog/ChangeLogManager.cs) receives an update message from Dataverse, maps the changes made to a JObject, and then writes the changes to a JSON column in SQL.

#### ServiceEndPointStepManager
The [ServiceEndPointStepManager](/src/Archive%20Service/ArchiveService.Shared/Managers/ServiceEndpointSteps/ServiceEndPointStepManager.cs) handles the creating and deleting of SDK Steps on the service endpoint that sends messages to the ChangeLogManager.


