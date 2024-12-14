# ThatDeveloperDad System Framework  
## Purpose
This repository contains the source code for a constrained and opinionated arcitectural framework that I use when designing and constructing  applications.

In the past, I've applied this framework only as a mental model during my design and build process, and not had a codified form of it.

The .Net project, "ThatDeveloperDad.iFX" contains the code that formalizes these concepts as an Architecture and Construction framework.  It features a taxonomy by which the components of a system can be classified and organized, and enforces some simple rules across those classified components.  It drives this system composition by using the application's configuration to describe how the components are to be combined within the system.

More Detailed essays and Framework Documentation can be found in the docs folder, at the [Table of Contents](./docs/_ToC.md)

Non-Trivial examples of this framework in use can be seen throughout the codebase in this repository: [ThatDeveloperDad/TDMF_admin](https://github.com/ThatDeveloperDad/TDMF_admin)  

That repository holds the work in progress Administrative subsystem that will handle the boring administrative aspects of running The DM's Familiar as a Subscription Product.

## Features and Goals

The first goal with this framework was to solve a problem I've observed with the Dependency Injection methods built into .Net Core.  We currently had to register every dependency of every service within that "front-most" service container.  This created risk that our Designed Abstractions would leak across vertical slices or domain boundaries and into components that were never intended to consume them.  The composition engine within the framework allows us to more formally describe that hierarchy of dependency, and organize our Systems into services where each "top-level" service is responsible for its own dependency collection.

My secondary goal was to improve the experience when adding in cross cutting concerns such as consistent method call timing and logging.  I added the ability to assign Behaviors either globally or on specific services within the Architecture's Configuration.  These behaviors are injected into the transparent proxies delivered to the DI container when our services are requested.  These Behaviors open a wide range of opportunities to simplify the code within our individual components.  

Finally, this method allows us to delegate service construction across members of a team, and allows the individual services to be constructed in isolation, being integrated with the system's codebase as they're completed.  As long as the Detailed Design specification for an individual service is followed, additional change to the service's code will be minimal during this integration with the wider codebase.

### Optional Capabilities (Added 12/13/2024)  
I'll be adding articles to the docs folder for these new framework features soon.  It's late, and I've been working at this stuff all day, and "I am le tired!"  
- **Generic Collection Filtering:**  
Implemented in the `/CollectionUtilities` namespace.  
    - This allows us to create flexible, Generic `Filter<T>` that can be applied to `ICollection<T>` instances.  
    - These Filters CAN be automatically converted from one Type to another, provided both Types represent idioms of the DomainEntity. (see Generic Type Mapping)

- **Generic Type Mapping**  
Implemented in the `/DomainUtilities` namespace.  
    - Using custom Attributes defined in the `/DomainUtilities/Attributes` namespace, we can designate Data Contracts declared for any of our Archetypal Services as local representations of identified *DomainEntities*.
    - When a service's data contract(s) inherits from `IdiomaticType`, and decorated with the `DomainEntityAttribute` at the class level, and has at least one property decorated with the `EntityAttribute` attribute, we gain the ability to automatically map any properties that are so decorated between different classes that share a DomainEntity name.

## Benefits
Our system's components become much more cohesive and independent.  Because of the structured way in which dependency graphs are isolated from each other, accidental leakage across domain boundaries becomes much more difficult and easier to catch during review.

Incorrect component dependencies are identified during app start; It won't load if you've made inappropriate connections between component archetypes.

Inter-component communication patterns will become a concern of the Application, rather than a concern of the individual components.  When dependencies are registered within the same application process, the proxies are very thin and unobtrusive.  When dependencies require network communication, the code within the component need not change.

Once the application (or system) has been architected, the actual CODE that needs to be written becomes incredibly simple, clean, and direct.

This Framework takes care of a lot of the "meta" goop that creeps its way into our code bases.  Things like Call Timing, Resiliency/Retry logic, etc... can all be omitted from our Component Implementations. This lets us focus on simple, clear, business logic within those files and projects.

## Setup
This codebase is built using .Net 9, though it could be compiled using .Net 8 with modifications to the project files.

Download the repository, build it, run the test console & step through the code.  

I'd considered building all of this into a nuget package and making it available through public repositories, but I've had second thoughts.  

Application Frameworks should belong to the applications that are built upon them.  The code for those frameworks should be owned by the application teams.  Yes, this framework could be promoted to organization-wide use.  I'd encourage that, in fact.  However, it's vital for the health of your projects and products that you understand the Whys and Hows of what's going on in this Framework.

So...

Fork it or Download it.  
Kick the tires, learn how it works.  
Adapt it to your team's needs and show your buddies.  
IF you decide to more broadly adopt this, turn it into a nuget package in your org's private repo and make maintenance & extension of the framework a team activity.

Learn more.  Always learn more.

## References & Attribution  
The concepts this Framework is built upon come from IDesign, Inc.  This is my attempt at making them useable for my own projects.  

[IDesign on the web](https://www.idesign.net)  

The concepts are presented much more thoroughly in the book, <u>Righting Software</u> by Juval Lowy.

[Righting Software](https://idesign.net/Books/Righting-Software)
