using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ExampleMod;
using NineSolsAPI;

public class ScriptableObjectSaver : MonoBehaviour {
    // Directory path where JSON files will be saved
    private string directoryPath = @"E:\Games\json";

    // Set of unique objects to avoid duplicates
    private HashSet<ScriptableObject> uniqueScriptableObjects = new HashSet<ScriptableObject>();

    public void SaveAllScriptableObjectsAsJson() {
        // Create the DropTable folder inside the directory path
        string dropTableFolderPath = Path.Combine(directoryPath, "GameStatScriptableObject");
        if (!Directory.Exists(dropTableFolderPath)) {
            Directory.CreateDirectory(dropTableFolderPath);
        }

        // Serializer settings to format and handle special cases like reference loops
        var settings = new JsonSerializerSettings {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new SafeContractResolver()
        };

        // Iterate over all ScriptableObjects of a specific type (DropTable)
        foreach (var type in new Type[] { typeof(GameStatScriptableObject) }) {
            foreach (var obj in Resources.FindObjectsOfTypeAll(type)) {
                // Skip TeleportPointData objects
                if (obj is TeleportPointData teleportPointData) {
                    continue;  // Skip this object
                }

                if (obj is ScriptableObject so && uniqueScriptableObjects.Add(so)) {
                    // Log the name and type of the object
                    ToastManager.Toast($"Found ScriptableObject: Name = {so.name}, Type = {so.GetType()}");

                    // Serialize the ScriptableObject to JSON
                    string json = JsonConvert.SerializeObject(so, settings);

                    // Generate the file path for the JSON file inside the DropTable folder
                    string fileName = $"{so.name}_{so.GetType().Name}.json";
                    string filePath = Path.Combine(dropTableFolderPath, fileName);

                    // Write the serialized JSON to the file
                    File.WriteAllText(filePath, json);

                    // Log the success
                    Log.Info($"Saved {so.name} as JSON: {filePath}");
                }
            }
        }
    }
}

// Custom contract resolver to skip Unity-specific properties and objects
public class SafeContractResolver : DefaultContractResolver {
    protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization) {
        var property = base.CreateProperty(member, memberSerialization);

        // Skip Unity-specific properties and properties with complex types (e.g., UnityEngine.Object)
        if (typeof(UnityEngine.Object).IsAssignableFrom(property.PropertyType)) {
            property.ShouldSerialize = instance => false;
        }

        return property;
    }
}
