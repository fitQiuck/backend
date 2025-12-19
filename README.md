# How to Run the Project

1. ## Clone the repository
```bash
git clone https://github.com/YourUsername/YourProjectName.git
cd YourProjectName
```

--------------------------------------------------------------

2. ## Install dependencies
```
dotnet restore
```

--------------------------------------------------------------

3. ## Configure database

  ### Edit appsettings.json connection string for PostgreSQL

  ### Run migrations
```
dotnet ef database update --project YourProject.Infrastructure --startup-project YourProject.Api
```

--------------------------------------------------------------

4. ## Prepare image folder
```
YourProject.Api/wwwroot/images/overallImages
```

--------------------------------------------------------------

5. ## Run the project
```
dotnet run --project YourProject.Api
```
