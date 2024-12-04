[Table of Contents](./_ToC.md)
# Closed Architecture
---
## What does Closed Architecture mean?
In this case, "Closed Architecture" is a design for a system's inner workings that specifies a Taxonomy and Ontology that can be applied to each component that contributes to our System.  

The primary benefit from use of the Closed Architecture that is described and facilitated by this framework is flexibility of deployment and composition.  

By using these concepts, you can start development of a complete system that is delivered as a true Modular Monolith with clear separation of concerns between the various modules of your application.  

As the application matures into a System, you'll be able to factor Subsystem modules into their own discrete hosts and replace the original implementations with strongly typed proxies to those services, leaving the rest of your application untouched.  

...Nice...!

We apply this principle as the next step, once we've identified our Volatilities.  Once you're used to the taxonomy, you'll be applying it WHILE you identify your static components.

---

### Taxonomy  
For systems built using this framework, the components you add will fall into one of the five following categories:

**Client**  
**Manager**  
**Engine**  
**Resource Access**  
**Utility**

---

### Ontology  
Within the framework, there is a corresponding "Marker" interface that will be used to identify each of your components as one of those archetypes.  These Marker interfaces are used by the framework while constructing your components to enforce the integration rules, which are:

* **Clients** may consume *Managers* and *Utilities*.  
* **Managers** may consume *Engines*, *ResourceAccess*, and *Utilities*.  
    * Managers MAY invoke public methods on other Managers with the following constraints (advanced scenario, see below <sup>1</sup>):
        * The invocation must be asynchronous.
        * The invocation must be one-way.
        * The invocation's result must have no effect on the success or failure of the invoking process.
* **Engines** may consume *ResourceAccess* and *Utilities*.  
* **ResourceAccess** may consume *Utilities*.  
* **Utilities** may consume other *Utilities*, but no other component archetype, nor may a Utility class contain any Domain-Specific logic or concept.  

Communication between components may not occur in any manner that breaks that ontology.  

<sup>1</sup>Manager-to-Manager invocations are a more advanced architectural scenario and require significant additional effort to "get right".  For now, we're not going to consider those scenarios.  

Additionally:  
* Each Component must be considered as a discrete independent service, with its own definitions for the Domain Concepts upon which it operates.
* Components must not consume other Components directly, but rather through a Proxy mechanism.

**For Most Components**  
It is advantageous to define the Contracts that describe the Behaviors and Data-Exchange objects of your components in an assembly that is separate from the implementation.  

This allows us to provide portability for these implementations later, if we need to.