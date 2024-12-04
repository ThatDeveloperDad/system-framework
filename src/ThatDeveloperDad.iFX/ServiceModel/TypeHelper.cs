using System;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using ThatDeveloperDad.iFX.ServiceModel.Taxonomy;

namespace ThatDeveloperDad.iFX.ServiceModel;

public class TypeHelper
{    

    private static List<Type> serviceArchetypes = new List<Type>(){
					typeof(IManagerService),
					typeof(IUtilityService),
					typeof(IEngineService),
					typeof(IResourceAccessService),
					typeof(IClientService),
                    typeof(IApplicationContainer)
				};

    public static Type LoadType(string typeName, string assemblyName)
    {
        Type? type = Type.GetType(typeName);
        if (type == null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loadedTypes = assemblies.SelectMany(a => a.GetTypes());
            type = loadedTypes
                .Where(t => t.Name == typeName)
                .FirstOrDefault();
        }

        if(type == null && string.IsNullOrWhiteSpace(assemblyName) == false)
        {
            Assembly typeSource = Assembly.Load(assemblyName);
            if(typeSource == null)
            {
                // If we STILL haven't got the assembly, we'll need to
                // load it from the file system.
                string assemblyFileName = $"{assemblyName}.dll";
                var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyFileName);
                if(File.Exists(assemblyPath))
                {
                    typeSource = Assembly.LoadFrom(assemblyPath);
                }
                if(typeSource == null)
                {
                    throw new Exception($"The assembly {assemblyName} could not be loaded.");
                }
            }
            type = typeSource.ExportedTypes
                .Where(t => t.Name == typeName)
                .FirstOrDefault();
        }

        if (type == null)
        {
            throw new Exception($"The type {typeName} could not be loaded.");
        }
        return type;

    }

    /// <summary>
    /// Scans the provided assembly for a class that implements the provided contractName.
    /// </summary>
    /// <param name="contractName">The name of the Interface we need implemented.</param>
    /// <param name="assemblyName">The Assembly in which to look for the requested Implementation.</param>
    /// <returns>The Type of the class that implements the requested contract.</returns>
    /// <exception cref="TypeLoadException">If an implementation is not found in the provided Assembly, we throw this exception.</exception>
    public static Type LoadImplementingType(string contractName, string assemblyName)
    {
        Type? type = null;

        Assembly typeSource = Assembly.Load(assemblyName);
        if(typeSource == null)
        {
            // If we STILL haven't got the assembly, we'll need to
            // load it from the file system.
            string assemblyFileName = $"{assemblyName}.dll";
            var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyFileName);
            if(File.Exists(assemblyPath))
            {
                typeSource = Assembly.LoadFrom(assemblyPath);
            }
            if(typeSource == null)
            {
                throw new TypeLoadException($"The assembly {assemblyName} could not be loaded.");
            }
        }

        type = typeSource.ExportedTypes
            .FirstOrDefault(t=> (t.IsClass || 
                    (t.IsValueType && t.IsPrimitive == false && t.IsEnum == false))
                    && t.GetInterfaces()
                        .Select(i=>i.Name)
                        .Contains(contractName));

        if(type == null)
        {
            throw new TypeLoadException($"An implementation for {contractName} was not found in the {assemblyName} assembly.");
        }

        return type;
    }

    /// <summary>
    /// Given a contract, this method will return the Archetype
    /// if the contract has been grouped into the Service Taxonomy.
    /// </summary>
    /// <param name="contract">The Contract to check.</param>
    /// <returns></returns>
    public static Type? GetArchetype(Type contract)
    {
        if(contract.IsInterface == false)
        {
            return null;
        }

        if(contract.IsAssignableTo(typeof(ISystemComponent)) == false)
        {
            return null;
        }

        Type? contractArchetype = 
            serviceArchetypes
            .FirstOrDefault(sa=> contract.IsAssignableTo(sa));

        return contractArchetype;
    }

    public static Type[] GetAllowedDependencyArchetypes(Type contract)
    {
        
        Type? contractArchetype = GetArchetype(contract);
        if(contractArchetype == null)
        {
            return Array.Empty<Type>();
        }

        List<Type> allowedArchetypes = new List<Type>();
        
        if(contractArchetype == typeof(IApplicationContainer))
        {
            allowedArchetypes.Add(typeof(IManagerService));
            allowedArchetypes.Add(typeof(IClientService));
            allowedArchetypes.Add(typeof(IUtilityService));
        }
        else if(contractArchetype == typeof(IClientService))
        {
            allowedArchetypes.Add(typeof(IUtilityService));
            allowedArchetypes.Add(typeof(IEngineService));
        }
        else if(contractArchetype == typeof(IEngineService))
        {
            allowedArchetypes.Add(typeof(IUtilityService));
            allowedArchetypes.Add(typeof(IResourceAccessService));
        }
        else if(contractArchetype == typeof(IManagerService))
        {
            allowedArchetypes.Add(typeof(IUtilityService));
            allowedArchetypes.Add(typeof(IEngineService));
            allowedArchetypes.Add(typeof(IResourceAccessService));
        }
        else if(contractArchetype == typeof(IResourceAccessService))
        {
            allowedArchetypes.Add(typeof(IUtilityService));
        }
        else if(contractArchetype == typeof(IUtilityService))
        {
            allowedArchetypes.Add(typeof(IUtilityService));
        }
        
        return allowedArchetypes.ToArray();
    }

    /// <summary>
    /// Within the Closed Architecture pattern we use,
    /// we need to ensure that dependency modules are being added
    /// to appropriate dependent modules.
    /// 
    /// This method will validate that the dependency is allowed
    /// </summary>
    /// <param name="receiver">The interface that the dependent module complies with.</param>
    /// <param name="dependency">The interface of the dependency type being validated.</param>
    /// <returns></returns>
    public static bool ReferenceIsValid(Type receiver, Type dependency)
    {
        bool isAllowed = false;
        Type? receiverArchetype = GetArchetype(receiver);
        Type? dependencyArchetype = GetArchetype(dependency);

        if(receiverArchetype == null)
        {
            return isAllowed;
        }

        var allowedArchetypes = GetAllowedDependencyArchetypes(receiver);
        if(allowedArchetypes.Contains(dependencyArchetype))
        {
            isAllowed = true;
        }
        return isAllowed;
    }
}
