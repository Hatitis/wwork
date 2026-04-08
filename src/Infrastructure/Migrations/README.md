Run the following to create the initial migration once .NET SDK is available:

```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```
