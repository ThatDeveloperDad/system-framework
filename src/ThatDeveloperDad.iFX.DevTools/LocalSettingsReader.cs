using Microsoft.Extensions.Logging;
using System.IO;

namespace ThatDeveloperDad.iFX.DevTools;

/// <summary>
/// Adds a way to store and incorporate local development settings 
/// into the application.
/// 
/// I was using DotNetEnv, but it doesn't seem to be working anymore.
/// </summary>
public class LocalSettingsReader
{

    private static string _CommentToken = "#";

    /// <summary>
    /// Loads the indicated file, and adds any settings found to
    /// the current runtime Environment.
    /// </summary>
    /// <param name="settingsFileName">The name of the file that contains the Environment settings.</param>
    /// <param name="clobber">(Optional, default is true) A boolean value stating that settings loaded from this file should overwrite the values of existing settings if they're already present.</param>
    /// <param name="throwOnError">(Optional, default is false) A boolean flag that determines whether an error is to be simply logged without shutting down the process, or if any errors should be propageted back to the caller.</param>
    /// <param name="logger">(Optional, default is null) An ILogger object that is used to log the progress of this method, if supplied.</param>
    public static void LoadLocalSettings(
        string settingsFileName, 
        bool? clobber = true,
        bool? throwOnError = false,
        ILogger? logger = null)
    {
        // Access the file.  If it doesn't exist, Log a warning and return.
        var execPath = AppDomain.CurrentDomain.BaseDirectory;
        var settingsPath = Path.Combine(execPath, settingsFileName);

        try
        {
            if(File.Exists(settingsPath) == false)
            {
                logger?.LogWarning($"The file {settingsFileName} was not found at the current execution path.");
                return;
            }

            var fileLines = File.ReadAllLines(settingsPath);
            foreach(string line in fileLines)
            {
                // Read the file, and parse each line.
                // If the line is comment, move next.
                // Each line will hold one Setting in the format of [Key]=[Value]
                // If the Key already exists, and "clobber" is false, we move on.
                // Otherwise, we'll add the described setting to the Environment owned by the ambient process.
                if(line.StartsWith(_CommentToken) == true)
                {
                    continue;
                }
                var settingParts = line.Split("=");
                if(settingParts.Length != 2)
                {
                    logger?.LogWarning($"The line '{line}' in the settings file {settingsFileName} is not in the correct format.");
                    continue;
                }
                string key = settingParts[0];
                string value = settingParts[1];
                if(Environment.GetEnvironmentVariable(key) != null && clobber == false)
                {
                    logger?.LogInformation($"The setting {key} already exists in the Environment, and the clobber flag is set to false.  Skipping.");
                    continue;
                }
                Environment.SetEnvironmentVariable(key, value);
            }
        
        }
        catch(Exception e)
        {
            logger?.LogError($"An error occurred while trying to load the settings file {settingsFileName}.", e);
            logger?.LogWarning($"Because the {settingsFileName} file could not be loaded, some system configuration may be missing.");
            if(throwOnError == true)
            {
                throw;
            }
        }
    }
}
