using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThatDeveloperDad.iFX.DomainUtilities;

/// <summary>
/// Base Class that can be applied to any DTO or POCO to identify it as
/// a local idiomatic form of a defined Domain Entity.
/// 
/// If you want to use the DomainObjectMapper with your classes, they must inherit from this class,
/// and be decorated with the DomainEntityAttribute, and have at least one property decorated with
/// the EntityAttributeAttribute.
/// 
/// This mechanism is used by the DomainObjectMapper to ease the
/// Developer Experience of mapping between instances of different idiomatic classes
/// that represent the smae conceptual Domain Entity.
/// </summary>
public abstract class IdiomaticType : IDomainIdiom
{
    // It'd be really cool to build in a Roslyn Analyzer that would inspect
    // classes that inherit this and ensure that they are decorated with 
    // the DomainEntityAttribute.  Maybe I'll do that one day, when I'm feeling frisky.
    [JsonIgnore]
    private string? _entityName;
    
    [JsonIgnore]
    public string EntityName  
    {
        get
        {
            // use the Lazy-load pattern here.
            if(_entityName != null)
            {
                return _entityName;
            }
            _entityName = this.ReadDomainEntityName();
            return _entityName;
        }
    }

    [JsonIgnore]
    private Dictionary<string, PropertyInfo>? _entityAttributes;
    [JsonIgnore]
    public Dictionary<string, PropertyInfo> EntityProperties
    {
        get
        {
            // use the Lazy-load pattern here too.
            if(_entityAttributes != null)
            {
                return _entityAttributes;
            }
            _entityAttributes = this.ReadEntityAttributes();
            return _entityAttributes;
        }
    }

    [JsonIgnore]
    private Type? _localType;
    [JsonIgnore]
    public Type LocalType
    {
        get
        {
            // use the Lazy-load pattern here too.
            if(_localType != null)
            {
                return _localType;
            }
            _localType = this.GetType();
            return _localType;
        }
    }
}
