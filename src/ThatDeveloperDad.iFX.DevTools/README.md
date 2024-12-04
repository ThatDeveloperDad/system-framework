# README.md for ThatDeveloperDad.iFX.DevTools

I'll be putting some helper classes in here that are intended to smooth the Developer Experience while building complex, composable applications.

These are not intended to be used within a Production Deployment, and so calls to these classes should be wrapped in the `#if DEBUG` compiler directive.


## LocalSettingsReader
Configuring your application can be a Real Pain<sup>TM</sup> when you have settings that need to vary from one environment to the next.

We all know we shouldn't store sensitive configuration values in any files that end up in Source Control (for obvious reasons,) and wrangling all that can be a Total Drag.

So what I've come around to doing is to use configuration sources in order of corresponding Volatility.

One Caveat to this is for sensitive settings.  Connection Strings, API Keys, really any "Secret" values need to go into some kind of secure store that gets loaded into your app's Configuration at spin-up time.

### Non-Volatile, Non-Sensitive Configuration
I use `appsettings.json` for this kind of thing.  The "Architecture" node that feeds the app Build-Up is perfectly fine to go here.  Static Values that change with the same (or slower) cadence of the components that use them are fine here as well.

Just remember that appSettings.json can only be modified at deployment time, in most cloud-based or container-based deployment scenarios, and that altering this file's contents will likely cause a restart of your application.

### Environmental Settings (whether sensitive or not)
For settings that need to be set specifically based on WHERE the code executes, I've gotten in the habit of using Environment Variables.  

These things can be tricky, and in corporate environments we don't often have the required level of access to set those even on our "own" development machines.  The NodeJs and NPM folks have used a ".env" file to carry the Environment Variables that apply to their apps on their local development machines, and we can do something similar in .Net.  

(We have to take a little longer way 'round for it, but this strategy works well.)

***IMPORTANT!!!***  If you go this route, MAKE SURE you add whatever filename you choose to your .gitignore file BEFORE you add it.  I've left mine in place in testConsole for demo purposes.  It's not got anything real in it anyway, so I don't feel *^too^* guilty. :P

### Sensitive Settings
While you can use this `localsettings.env` technique for passwords, API keys, etc... be REALLY careful about making sure this file stays out of your source control.

In your application's configuration build out method, add whatever Secret Store you're planning to use in production, and make sure you test it thoroughly.  This is one of those things that *^might^* work on your machine, and *^might^* work elsewhere, so don't take it for granted. ;)

## How To format and consume this file is described... NEXT!

This class reads a file formatted like a .env file, and adds the Key-Value Pair settings to your application's runtime environment.

I've added this because the package I had been using (DotNetEnv) for this isn't working correctly in .Net 9.  If you're not using .Net 9, or can get that package to work, I encourage you to use it instead of this ersatz hack-around. ;)

### Setting it up
1. Add a new text file to the Project Folder that needs the Environmental settings.  Call it something that makes sense.  I prefer to use the ".env" extension to make it easy to see what it means at a glance.  
2. Edit the .gitignore file for your current repository, and add this filename to the list of ignored files and folders.
3. Open the app's `[ProjectName].csproj` file and add the following:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  ... other Project file goop, omitted for brevity ...
  <ItemGroup> 
  <!-- Configration Files and other stuff that needs to get
  to the execution directory can go in here...-->
    <None Update="[Whatever_You_Called_Your_File]">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <!-- You've likely already got this next node in there if you're using an appsettings.json file.  -->
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
  ... there might be more goop here too ...
</Project>
```


### Format
Here's the format I've put in place for these "localsettings" files.

**Comments:  ** use a single at the beginning of a line to add a comment.

**Settings:  ** Everything in here is strings, formatted as Key-Value Pairs.  
**Key**=**Value**  

Where Key is the formatted name for the Setting and Value is what will be configured.  Note that I'm just doing a string.Split("="), so if your value contains an equals sign, it's not going to get added here.

You ^can^ factor or subdivide these settings into "sections" by using a double underscore as the Section Delimiter.  When you do that, you'll have to alter the way the "external" setting value is referenced in your appSettings.json.

**Example:**  
```
In localsettings.env, I've added a "fake" connection string as a local setting.:

--
SQLDb__ConnectionString=SomeConnectionString
--

```
In appSettings, we reference that "externalized" setting like so:
```json

"Modules":[
            {
                "LogicalName": "ThingUseCases",
                "Contract":"ISvc1", 
                "ContractAssembly":"Apps.Managers.Svc1.Abstractions",
                "Lifetime":"Singleton", 
                "Implementation":{
                    "Source":"Module",
                    "Assembly":"Managers.Svc1",
                    "ServiceOptions":{
                        "StringOption":"SomeString",
                        "IntOption": 5,
                        "SomeSecret":"EXT:SQLDb:ConnectionString"
                    }
                },
    ... rest of appsettings omitted ...

```
I use "EXT:" in the ServiceOptions nodes to identify that that Option value should be loaded from the ambient configuration, using the Key that comes after the "EXT:" token.

When we separate the settings into sections using the "__" in the environment file, we replace the double underscore with a ":" to plumb through the Configuration Path.