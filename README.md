# Users REST service

## Setup

1. Clone repository to your working directory (e.g. C:\dev\RestAPIExample)
2. Add environment variable SOLUTION\_DIR with the value of repo filepath 
(this variable is used for building porjects).
Following command could be used in command line:  
  
    `SETX SOLUTION_DIR C:\dev\RestAPIExample`
3. Register URL that will be used by Users WCF service. 
So that you don't need to run VS or console application that hosts WSF service as administrator
(More info [here](https://msdn.microsoft.com/library/ms733768(v=vs.110).aspx)) 
Following command could be used in command line if deploy at localhost port 8000 (URL : [http://localhost:8000]()): 

    `netsh http add urlacl url=http://+:8000/ user=MYDOMAIN\a.rebrov`

4. Creat folder where data file ofr SQL Server Compact database will be created (e.g. C:\dev\RestAPIExample\DataFolder)

5. Open solution in Visual Studio

6. If not configured enable automatic nuget restore in your VS as described
 [here](https://docs.microsoft.com/en-us/nuget/consume-packages/package-restore#migrating-to-automatic-restore)

7. Open __App.config__ file of WebServicesHostApplication project.
* Find __connectionStrings__ tag and set value of attribute __connectionString__ 
to "Data Source=C:\dev\RestAPIExample\DataFolder\WebServicesDB.sdf"
where path to file is folder you have created on step 4. and WebServicesDB.sdf - name of Db file.
It should look similar to following:
    `<connectionStrings>
        <add name="WebServicesDBConnectionString" connectionString="Data Source=C:\dev\RestAPIExample\DataFolder\WebServicesDB.sdf" providerName="System.Data.SqlServerCe.4.0" />
    </connectionStrings>`
* Make sure __endpoint__ tag under __system.serviceModel__ has attribute __address__ with same value as configured on step 3. 
(If not application will work but only if you launch it with administator rights)
8. Repeat step 7 for __App.config__ file of WebServicesIntergrationTests project - tag __connectionStrings__
* Additionally make sure that __testSettings__ has correct UserServiceURL value 
(if nothing manually changed after repo clone it should be correct)

9. Build solution

10. To start Users REST service launch ServiceApplication.exe from the Debug(Release)/bin folder 
(depending under whihc configuration solution was built)

## Runing tests using NUnit console

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
CreateUser\_ReturnsCreatedCodeAndSavesUserInDB\_WhenAddingNewUniqueUser 
CreateUser\_ReturnsCreatedCodeAndSavesUserInDB\_WhenAddingNewUniqueUserWithoutName 

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
UpdateUserByNickName\_ReturnsOkCode\_WhenUserUpdatedInDB 

### Delete user
DeleteUserByNickName\_ReturnsBadRequestCode\_WhenRequestContainsIllegalCharacter(&quot;NickName}|&quot;) 
DeleteUserByNickName\_ReturnsBadRequestCode\_WhenRequestContainsIllegalCharacter(&quot;Nick()Name&quot;) 
DeleteUserByNickName\_ReturnsBadRequestCode\_WhenRequestContainsIllegalCharacter(&quot;&amp;*NickName&quot;) 
DeleteUserByNickName\_ReturnsNotFoundCode\_WhenNoSuchFoundInDB 
DeleteUserByNickName\_ReturnsOkCodeAndUser\_WhenUserDeletedInDB 

## Tech solution analysis

### Pro's
1. Lightweight and easy to deploy - no need of IIS to host web service.
No needto use heavy DB applicatin like MS SQL Server.
2. REST service is easily could be used from any HTTP based client software
3. Tests and REST API specification present exhaustive functional documentation of app

### Con's
1. Update in following form PUT /Services/TestService/Users/{NickName}
does not make much sense. It's not good idea to update NickNAme since it's PK in DB.
From the other hand if pass NickName in URL we still need to send in body new value of
UserName. So I implemented it;s differently:
PUT /Services/TestService /Users (Body contains user information - NickName of user to update
and new value for UserName)
2. NickName isn't good candidate for PK in DB. Better to have separate EntityID field
that is autogenerated on DB level.
3. WCF does not support Dependency Injection out-of-the-box. Hence it's hard to unit test
logic inside the service. (Not saying that if need to use another storage e.g. NoSQL we need
rewrite service completely with possibly code duplication)
4. Console app isn't perfect candidate to be host of the web service - e.g. it's hard to manage it
(gracefully start and stop). Windows Service might be better candidate or even web app with possibility
to be deployed at cloud (e.g.Azure)

### Extra notes & futher improvements
1. Logging wasn't part of the taks but it's nice to have feature.
2. There was not requiremetns about what client to use so I took advantage of RestSharp library
since I've used it before and it seemed good option for me as REST client for API testing.
3. Add DI for the WCF service
4. Validation if parameters of incoming request could be implemented via Attributes - this facilitate code reuse if we need e.g. add more services to Users API that also pass some input
5. "One-click" run of tests is possible. The flow in the script will be following :
* Setup environment : add env variables, register url of the service, create folder for DB file
* Restore packages using nuget restore (need to dowload nuget.exe of thee version that compatible with MSBuild installed on PC)
* Build solution with MSBuild
* Run tests with NUnit Console
