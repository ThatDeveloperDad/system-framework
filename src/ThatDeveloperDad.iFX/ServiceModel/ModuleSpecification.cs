using System;
using System.Diagnostics.Contracts;
using Microsoft.Extensions.Configuration;

namespace ThatDeveloperDad.iFX.ServiceModel;

public struct ModuleSpecification
{
    public ModuleSpecification()
    {
        LogicalName = string.Empty;
        Contract = string.Empty;
        ContractAssembly = string.Empty;
        Lifetime = "Transient";
        Implementation = new ImplementationSpec();
        Dependencies = Array.Empty<ModuleSpecification>();
        Behaviors = Array.Empty<BehaviorSpec>();        
    }   
    public string LogicalName { get; set; }
    public string Contract { get; set; }
    public string ContractAssembly { get; set; }
    public string Lifetime { get; set; }

    public ImplementationSpec Implementation { get; set; }

    public ModuleSpecification[] Dependencies { get; set; } 

    public BehaviorSpec[] Behaviors { get; set; }

    /// <summary>
    /// Merges any behaviors that have been declared by the Architecture as "Global"
    /// to the Module's own set of Behaviors.
    /// 
    /// If a behavior Type is added both Globally and to the Module, 
    /// only one instance of the behavior is added.
    /// </summary>
    /// <param name="globalBehaviors"></param>
    public void AddGlobalBehaviors(BehaviorSpec[] globalBehaviors)
    {
        List<BehaviorSpec> newBehaviors = Behaviors.ToList()?? new List<BehaviorSpec>();
        foreach(var behavior in globalBehaviors)
        {
            if(newBehaviors.Any(b=> b.Name == behavior.Name))
            {
                // set the existing behavior as "global" to that it gets passed
                // through to dependency modules.
                var existing = newBehaviors.First(b=> b.Name == behavior.Name);
                existing.IsGlobal = true;
            }
            else
            {
                var newBehavior = new BehaviorSpec
                {
                    Name = behavior.Name,
                    AssemblyName = behavior.AssemblyName,
                    IsGlobal = true
                };
                newBehaviors.Add(newBehavior);
            }
        }
        Behaviors = newBehaviors.ToArray();
    }
}

public struct ImplementationSpec
{
    public string Source { get; set; }
    public string Assembly { get; set; }

    public IConfigurationSection? ServiceOptions { get; set; }
}

public struct BehaviorSpec
{
    public BehaviorSpec()
    {
        Name = string.Empty;
        AssemblyName = string.Empty;
    }
    public string Name { get; set; } 
    public string AssemblyName { get; set; } = string.Empty;
    public bool IsGlobal { get; set; } = false;
}
