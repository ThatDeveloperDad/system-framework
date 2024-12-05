using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ThatDeveloperDad.iFX.Serialization;

/// <summary>
/// Provides methods that make working with JSON easier.
/// </summary>
public static class JsonUtilities
{
    /// <summary>
    /// Sometimes, when we request that a Language Model return JSON instead of text,
    /// the model will return that json surrounded by a markdown code fence, despite our instructions.
    /// This method strips that markdown Codefence if it's present.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string StripMarkdown(this string json)
    {
        string jsonFenceStart = "```json";
        string codeFenceEnd = "```";

        json = json.Replace(jsonFenceStart, string.Empty)
                   .Replace(codeFenceEnd, string.Empty);

        return json;
    }

    /// <summary>
    /// Returns a JSON String from the provided object.
    /// Any properties with null, empty string, or default values
    /// are omitted.
    /// </summary>
    /// <typeparam name="T">The Type of objecy to be serialized.</typeparam>
    /// <param name="instance">The instance of T to be serialized.</param>
    /// <returns>A Write-Indented JSON String</returns>
    public static string GetCleanJson<T>(T instance)
    {
        var jsonOptions = CreateJsonOptions()
                            .OmitEmptyStrings()
                            .OmitDefaults();

        string rawJson = JsonSerializer.Serialize(instance, jsonOptions);
       
        string? cleanJson = rawJson.PurgeNullElements();
        // We don't want to return null.
        // if, for some reason, the PurgeNullElements method
        // returns a null string, we'll return the rawJson
        if(cleanJson != null)
        {
            rawJson = cleanJson;
        }

        return rawJson;

    }

    public static T? ToInstance<T>(this string json)
    {
        var rehydrated = JsonSerializer.Deserialize<T>(json);
        return rehydrated;
    }

    public static string SerializeForOutput<T>(this T instance) where T : class
    {
        string json = JsonUtilities.GetCleanJson(instance);

        return json;
    }

    public static string SerializeForStorage<T>(this T instance) where T: class
    {
        JsonSerializerOptions options
            = JsonUtilities.CreateJsonOptions();
        //options.TypeInfoResolver =                  

        string json = JsonSerializer.Serialize<T>(instance, options);
        return json;
    }

    /// <summary>
    /// Creates a default JsonSerializerOptions instance
    /// </summary>
    /// <returns>Returns an unmodified JsonSerializerOptions instance</returns>
    public static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions();
    }

    /// <summary>
    /// Updates the provided JsonSerializerOptions to
    /// Set the WriteIndented option to true
    /// </summary>
    /// <param name="options">The JsonSerializerOptions to be modified </param>
    /// <returns>The modified JsonSerializerOptions </returns>
    public static JsonSerializerOptions WithWriteIndented(this JsonSerializerOptions options)
    {
        options.WriteIndented = true;
        return options;
    }

    /// <summary>
    /// Updates the provided JsonSerializerOptions to
    /// Add the default ignore condition of WhenWritingNull
    /// </summary>
    /// <param name="options">The JsonSerializerOptions to be modified </param>
    /// <returns>The modified JsonSerializerOptions </returns>
    public static JsonSerializerOptions OmitNulls(this JsonSerializerOptions options)
    {
        
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        return options;
    }

    /// <summary>
    /// Updates the provided JsonSerializerOptions to
    /// Add the default ignore condition of WhenWritingDefault
    /// </summary>
    /// <param name="options">The JsonSerializerOptions to be modified </param>
    /// <returns>The modified JsonSerializerOptions</returns>
    public static JsonSerializerOptions OmitDefaults(this JsonSerializerOptions options)
    {
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        return options;
    }

    /// <summary>
    /// Updates the provided JsonSerializerOptions to
    /// Add a converter that converts empty string properties to null
    /// Add the default ignore condition of WhenWritingNull
    /// </summary>
    /// <param name="options">The JsonSerializerOptions to be modified</param>
    /// <returns>The modified JsonSerializerOptions</returns>
    public static JsonSerializerOptions OmitEmptyStrings(this JsonSerializerOptions options)
    {
        EmptyStringToNullConverter emptyStringConverter = new EmptyStringToNullConverter();
        options.Converters.Add(emptyStringConverter);
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        
        return options;
    }

    /// <summary>
    /// Accepts a raw Json string, and removes any elements
    /// whose value is null, empty string, empty array, or empty object.
    /// </summary>
    /// <param name="jsonString">The original Json string</param>
    /// <returns>A new Json string that includes only those elements that are populated</returns>
    private static string? PurgeNullElements(this string? jsonString)
    {
        if (jsonString == null)
        {
            return null;
        }
        
        JsonObject? tempObject = JsonSerializer.Deserialize<JsonObject>(jsonString);
        if(tempObject == null)
        {
            return jsonString;
        }
        
        var filteredObject = CleanseNulls(tempObject);

        JsonSerializerOptions finalOptions = CreateJsonOptions()
                                                 .OmitDefaults()
                                                 .WithWriteIndented();

        string rebuiltJson = JsonSerializer.Serialize(filteredObject, finalOptions);

        return rebuiltJson;
    }

    /// <summary>
    /// Accepts a JsonObject and removes any properties whose
    /// value is null, empty string, empty array, or empty object.
    /// </summary>
    /// <param name="tempObject">The JsonObject to be cleaned</param>
    /// <returns>A new JsonObject that includes only those properties that 
    /// are populated</returns>
    private static JsonObject CleanseNulls(JsonObject tempObject)
    {
        var filteredObject = new JsonObject();

        foreach(var currentNode in tempObject.AsEnumerable())
        {

            if(currentNode.Value == null)
            {
                continue;
            }

            var currentValueKind = currentNode.Value.GetValueKind();
            var currentPropName = currentNode.Key;
            var currentPropValue = currentNode.Value;
            switch(currentValueKind)
            {
                case JsonValueKind.Null:    // Skip it.  We're purging nulls.
                    continue;
                
                case JsonValueKind.Array:
                    if(currentPropValue.AsArray().Count == 0)
                    {
                        continue;
                    }
                break;

                case JsonValueKind.Object:
                    var nullCleansedNode = CleanseNulls(currentPropValue.AsObject());
                    if(nullCleansedNode.Count == 0)
                    {
                        continue;
                    }
                    currentPropValue = nullCleansedNode;
                    break;
                default:    // We can just add it.
                
                break;
            }

            var filteredPropValue = currentPropValue.DeepClone();
            filteredObject.Add(currentPropName, filteredPropValue);

        }

        return filteredObject;
    }

}

