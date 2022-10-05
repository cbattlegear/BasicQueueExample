# Basic Azure Queueing Example

This is an example application showing the use of Azure Functions and 
bindings to create an end to end pipeline for getting REST API data in 
parallel while tracking last access time. 

## Prerequisites
You will need:
 - [Azure Functions development environment](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-vs?tabs=in-process)
 - [Azure Subscription](https://learn.microsoft.com/en-us/azure/guides/developer/azure-developer-guide#understanding-accounts-subscriptions-and-billing)
 - [Azure Function App](https://learn.microsoft.com/en-us/azure/azure-functions/functions-get-started?pivots=programming-language-csharp)
 - [Azure Storage Account](https://learn.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal)

## Test and Deploy
You will be able to quickly test and deploy this function following the 
instructions laid out as part of the [Create your first C# function in Azure using Visual Studio quickstart](https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio?tabs=in-process#run-the-function-locally)

The most important items you will have to change is setting the STORAGE_ACCOUNT, 
DATA_LAKE, and SQL_AZURE_CONNECTION_STRING application settings to your respective
resources.

If you are testing locally and would like to fire the timers without waiting 
for the triggers, you can make a post call to the admin endpoint. 
As an example you could use PowerShell to call the endpoint to run the 
GetAllPokemon function:

`Invoke-WebRequest -Uri "http://localhost:7071/admin/functions/GetAllPokemon" -Method Post -Body "{}" -ContentType "application/json"`

## Resources
These are also outlined in the comments for the specific functions that 
use them but just to make them more easily available. Here are the resources
I utilized to create this example: 
 - [Azure Storage Queue Output Bindings](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-output?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp)
 - [Azure SQL Input Bindings](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-azure-sql-input?tabs=in-process&pivots=programming-language-csharp)
 - [Azure Storage Queue Trigger](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp)
 - [Azure Functions Declarative Bindings](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library?tabs=v2%2Ccmd#binding-at-runtime)
 - [Azure SQL Output Bindings](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-azure-sql-output?tabs=in-process&pivots=programming-language-csharp)