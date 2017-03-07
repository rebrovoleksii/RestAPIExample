# Users REST API

## Setup

1. Clone repository to your working directory (e.g. C:\dev\RestAPIExample)
2. Add environment variable SOLUTION\_DIR with the value of repo filepath 
(this variable is used for building porjects).
Following command could be used in command line:  
  
    `SETX SOLUTION_DIR C:\dev\RestAPIExample`
3. Register URL that will be used Users WCF service. 
So that you don't need to run VS or console application that hosts WSF service as administrator
(More info [here](https://msdn.microsoft.com/library/ms733768(v=vs.110).aspx)) 
Following command could be used in command line: 

    `netsh http add urlacl url=http://+:8000/ user=MYDOMAIN\a.rebrov`

4. Creat folder where data file ofr SQL Server Compact database will be created (e.g. C:\dev\RestAPIExample\DataFolder)

5. Open solution in Visual Studio

6. If not configured enable automatic nuget restore in your VS as described
 [here](https://docs.microsoft.com/en-us/nuget/consume-packages/package-restore#migrating-to-automatic-restore)

7. Open __App.config__ file of WebServicesHostApplication project.
Find __connectionStrings__ tag and set value of attribute __connectionString__ 
to "Data Source=C:\dev\RestAPIExample\DataFolder\WebServicesDB.sdf"
where path to file is folder you have created on step 4. and WebServicesDB.sdf - name of Db file.
It should look similar to following:
    `<connectionStrings>
        <add name="WebServicesDBConnectionString" connectionString="Data Source=C:\dev\RestAPIExample\DataFolder\WebServicesDB.sdf" providerName="System.Data.SqlServerCe.4.0" />
    </connectionStrings>`
8. Repear setp 7 for __App.config__ file of WebServicesIntergrationTests project

9. Build solution

## Run tests using NUnit console

1. Install NUnit 3 Console - download and install it from [here](https://github.com/nunit/nunit-console/releases/tag/3.6.1)

2. Make sure NUnit Console is in your PATH

3. You can run RunTests.bat from comman lind with paratemer Debug or Release 
depending on configuration you used to build solution

## List of automated test cases

### Add user
CreateUser\_ReturnsBadRequestCode\_WhenRequestContainsIllegalCharacter(&quot;NickName}|&quot;)
CreateUser\_ReturnsBadRequestCode\_WhenRequestContainsIllegalCharacter(&quot;Nick()Name&quot;) 
CreateUser\_ReturnsBadRequestCode\_WhenRequestContainsIllegalCharacter(&quot;&amp;*NickName&quot;) 
CreateUser\_ReturnsConflictCode\_WhenSuchUserAlreadyExistInDB 
CreateUser\_ReturnsOkCodeAndUserSavedInDB\_WhenAddingNewUniqueUser 
CreateUser\_ReturnsOkCodeAndUserSavedInDB\_WhenAddingNewUniqueUserWithoutName 

### Get all users
GetUsers\_ReturnsAllUsers\_WhenMoreThenOneUserInDB 
GetUsers\_ReturnsEmptyResponse\_WhenNoUserInDB 
GetUsers\_ReturnsOneUser\_WhenOnlyOneUserInDB 

### Get user information
GetUserByNickName\_ReturnsBadRequestCode\_WhenNickNameIsTooLong 
GetUserByNickName\_ReturnsBadRequestCode\_WhenNickNametContainsIllegalCharacter(&quot;NickName}|&quot;) 
GetUserByNickName\_ReturnsBadRequestCode\_WhenNickNametContainsIllegalCharacter(&quot;Nick()Name&quot;) 
GetUserByNickName\_ReturnsBadRequestCode\_WhenNickNametContainsIllegalCharacter(&quot;&amp;*NickName&quot;) 
GetUserByNickName\_ReturnsCorrectUser\_WithMaxNickNameLength 
GetUserByNickName\_ReturnsCorrectUser\_WithMinNickNameLength 
GetUserByNickName\_ReturnsNotFoundCode_WhenNoUserFoundInDB 

### Update user
UpdateUser\_ReturnsBadRequestCode\_WhenRequestContainsIllegalCharacter(&quot;&amp;*NickName&quot;) 
UpdateUser\_ReturnsBadRequestCode\_WhenRequestContainsIllegalCharacter(&quot;NickName}|&quot;) 
UpdateUser\_ReturnsBadRequestCode\_WhenRequestContainsIllegalCharacter(&quot;Nick()Name&quot;) 
UpdateUserByNickName\_ReturnsNotFoundCode\_WhenNoSuchFoundInDB 
UpdateUserByNickName\_ReturnsOkCodeAndUser\_WhenUserDeletedInDB 

### Delete user
DeleteUserByNickName_ReturnsBadRequestCode_WhenRequestContainsIllegalCharacter(&quot;NickName}|&quot;) 
DeleteUserByNickName_ReturnsBadRequestCode_WhenRequestContainsIllegalCharacter(&quot;Nick()Name&quot;) 
DeleteUserByNickName_ReturnsBadRequestCode_WhenRequestContainsIllegalCharacter(&quot;&amp;*NickName&quot;) 
DeleteUserByNickName_ReturnsNotFoundCode_WhenNoSuchFoundInDB 
DeleteUserByNickName_ReturnsOkCodeAndUser_WhenUserDeletedInDB 

