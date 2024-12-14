using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThatDeveloperDad.iFX.DomainUtilities.Attributes;

namespace ThatDeveloperDad.iFX.DomainUtilities;

/// <summary>
/// Provides Extension Methods that ease the use of Domain Entities
/// when they are used in Application Components.
/// </summary>
internal static class DomainEntityExtensions
{
    public static string ReadDomainEntityName(this IDomainIdiom instance)
    {
        var entityType = instance.GetType();
        var entityAttribute = entityType.GetCustomAttribute<DomainEntityAttribute>();
        return entityAttribute?.EntityName ?? throw new ArgumentException("The DomainEntityAttribute is required for classes marked as IDomainIdiom.");
    }

    public static Dictionary<string, PropertyInfo> ReadEntityAttributes(this IDomainIdiom instance)
    {
        var entityType = instance.GetType();
        var entityAttributes = new Dictionary<string, PropertyInfo>();
        
        var decoratedProperties = entityType.GetProperties()
            .Where(p => p.GetCustomAttribute<EntityAttributeAttribute>() != null)
            .ToArray();

        if(decoratedProperties.Any() == false)
        {
            string domainEntity = instance.ReadDomainEntityName();
            throw new ArgumentException("Idiomatic Types must include at least one attribute from the identified Domain Entity.");
        }

        foreach (var property in decoratedProperties)
        {
            // Null-forgiving operator is OK here.  We're only looking at properties that have the attribute.
            var attributeName = property.GetCustomAttribute<EntityAttributeAttribute>()!.EntityAttributeName;
            entityAttributes.Add(attributeName, property);
        }
        
        return entityAttributes;
    }
}
