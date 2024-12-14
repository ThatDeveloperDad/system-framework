using System;
using ThatDeveloperDad.iFX.ServiceModel.Taxonomy;

namespace ThatDeveloperDad.iFX.DomainUtilities.Attributes;

/// <summary>
/// This attribute is applied to a class to indicate that it's an expression of a Domain Entity.
/// 
/// Classes marked with the same EntityName can be used by the DomainObjectMapper
/// to map between different idiomatic expressions (Local Idiom, or facet) of the same Domain Entity.
/// </summary>
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public class DomainEntityAttribute:Attribute
{
    /// <summary>
    /// This attribute is applied to a class to indicate that it's a component-local expression of a Domain Entity.
    /// </summary>
    /// <param name="entityName">Identifies which Entity in the Domain Ubiquitous Language is expressed.</param>
    /// <param name="declaringArchetype">Identifies the archetype of the component that owns "this" Local Idiom</param>
    /// <param name="domainSubcontext">Optional.  If the local idiom is specific to a particular Sub-Domain context, it can be identified here.
    /// The Default value for domainSubcontext is "*", which is a wildcard.  Idioms decorated with the WildCard can be exchanged with
    /// any Sub-Domain contexts in the system.
    /// </param>
    public DomainEntityAttribute(string entityName, 
        ComponentArchetype declaringArchetype,
        string domainSubcontext = "*")
    {
        EntityName = entityName;
        DeclaringArchetype = declaringArchetype;
        DomainSubcontext = domainSubcontext;
    }

    /// <summary>
    /// The name of the conceptual DomainEntity in the Ubiquitous Language
    /// that the decorated class represents.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// The Archetype of the component that declares and owns the
    /// decorated Local Idiom of the Domain Entity.
    /// </summary>
    public ComponentArchetype DeclaringArchetype { get; }

    /// <summary>
    /// If the LocalIdiom is specific to a particular subContext of the Domain,
    /// that can be identified by this property.
    /// </summary>
    public string DomainSubcontext { get; }
}
