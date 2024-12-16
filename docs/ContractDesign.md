[Table of Contents](./_ToC.md)

# Detailed Design - Data and Service Contracts

This is where we describe what each of our components does, and "how" we need to talk to them, in terms of what kinds of input the component features require, and what we can expect to receive back from those features when we invoke them.

***Note:*** With the addition of the (optional) DomainUtilities namespace and its mapping capability, I need to update this document to include the additional considerations when using *Conceptual* Domain Entities in your application.

---

This next statement is important:

***At this stage, we're describing the PUBLIC behavior and expectations of the component and its features.***  

***We don't care HOW those features are implemented, we're simply formalizing the description of what kinds of things our component can do, as well as the expectations that consumers of our component should have.***

Keep in mind that when I use the word "Public" here, I'm describing the part of the component that is exposed to its consumers, not necessarily a Public Network, whether it be the Internet or your organization's internal Networks.

These public behaviors form the API for the component.  
<blockquote>
(Again, API has become an overloaded term, and it causes many people to immediately think of a REST or RPC based WebApi, or some other networked programming surface.  In this case, if I use API, I mean Application Programming Interface, and nothing more.)
</blockquote>

I'll be using .Net / C# terminology for these concepts.  The same or similar concepts exist in many other languages and frameworks, but may use different terms.

Our Contracts describe the Behavior or Capabilities of a component, along with the expected data formats for input and output for those behaviors.  "Contract" based language is useful here, and was widely used in WCF (Windows Communication Framework,) in the pre .Net Core days.  The terminology is still useful today, even if it's no longer formalized in the Framework itself.

## Behavior
Component Behavior is described using .Net Interfaces.

An Interface is a description of the Operations that a given component makes available to consumers of that component.

You could consider an Interface to be a "Service Contract", in that any service that implements a given interface MUST provide some implementation of each Operation described by that Interface.

<blockquote>
Yes, there are scenarios where you might implement an interface and leave one or two of its methods NotImplemented, instead choosing to throw an exception when those methods are called, but this is a *Design Smell* and signals that you need to rethink that operation's inclusion in the interface.
</blockquote>

The methods described on the Interface can be considered "Operation Contracts".

```csharp
namespace MyApp.Engines.Randomizers;

// IRandomizer is our "Service Contract" here.
public interface IRandomizer
{
    // RollDice, and ShuffleAndChoose are our "Operation Contracts".

    ///<summary>
    /// Rolls the requested number of the identified kind of dice, then returns the sum
    /// of the rolled dice.
    ///</summary>
    int RollDice(
        DiceKinds diceKind, 
        int numberOfDice);

    ///<summary>
    /// Shuffles the provided array of cards, and returns the card at position 0 of 
    /// the array.
    ///</summary>
    ICard ShuffleAndChoose(ICard[] deck);
}
```

## Input/Output Expectations
These are the parameters and return types for the methods on your Interface.  

Initially, it's perfectly valid to use a simple parameter list for inputs, and a single Return Type from a function.  However, it's my experience that these parameter lists always grow and change, and then we have to plumb those changes throughout the call chains, and we haven't really gained any encapsulation.

Once I've got a parameter list that consists of more than one or two parameters, it's time to wrap those into a Data Contract. (These days, the term DTO is more fashionable, but they're the same thing, really.)  

As time goes by and you learn more about what your components need to do, you'll need to add additional items to your "first pass" lists of Operation Parameters, and every time you change the "shape" of an Operation Contract, you need to also adjust anything that's consuming the Service Contract to which it belongs.  This gets annoying after one or two cascading changes.

Returning to the Randomizer engine, let's say we want to upgrade the RollDice method to give ourselves some more flexibility.

In addition to simply rolling the dice and returning the sum of each rolled mathrock, we want to automatically adjust the result by some amount (to represent bonuses or penalties.)

We COULD just add that as a parameter, but at this point, we know we're setting up some kind og TTRPG system, and that we'll likely need to add more capability to the Dice Roller methods in the future.  It makes more sense, (from the point of view of *protecting the rest of the codebase from changes*) to specify a simple data class or structure to carry the ingredients required to roll a handful of our shiny math rocks.

```csharp
namespace  MyApp.Engines.Randomizers;

// DiceTray becomes our DTO, or Data Contract.
// We ALWAYS need to know How Many, and What Kind of dice to roll, so we'll require those
// in the constructor.
public struct DiceTray
{
    public DiceTray(
        DiceKinds diceKind,
        int numberToRoll)
    {
        DiceKind = diceKind;
        NumberToRoll = numberToRoll;
    }

    public DiceKinds DiceKind { get; set; }
    public int NumberToRoll { get; set; }

    // Since Adjustment is an optional thing, we should initialize this property
    // with a default value.
    // Some coding standard require this default value initialization in the constructor,
    // I like this implicit property initialization pattern though, so I'm going to use it
    // here, I feel like it tells me exactly what's going on with this property.  
    //
    // If I'm working on a team where this is forbidden, I of course would comply with that
    // group's adopted standards.
    public int Adjustment { get; set; } = 0;
}

// Our Service Contract
public interface IRandomizer
{
    // We've updated the OperationContract to use the new DiceTray contract, making
    // subsequent parameter changes MUCH easier on the consuming code.
    int RollDice(DiceTray diceTry);

    // This one hasn't changed.
    ICard ShuffleAndChoose(ICard[] deck);
}
```

With the addition of the DiceTray data contract, adding additional optional ways to affect the result of the RollDice method becomes MUCH easier, and affects only the code that wants to leverage those new capabilities as they're added.  (Adding something like "RollWithAdvantage" or "RollWithDisadvantage" becomes MUCH easier with this way of doing things.)

It also makes our Operation Contracts MUCH easier to read and understand later on, when we're thinking about USING the component instead of BUILDING the component.

Pretty Nice!

## So where should I put these contracts?

There's a couple places you could add these contracts within your solution structure.  

You could:  
* Include the contracts in the same project as the code that implements them.  
    * When I go this route, I prefer to put all the "public" stuff in its own folder within the project.  
    * I've called that folder "Public", "Contracts", or "Abstractions" pretty interchangeably, and I really ought to pick one and stick with it.

OR

* Put these contracts in a separate project that contains only the contracts for that component.  
    * In the past, I've had a single "Abstractions" or "Contracts" project for my entire solution, more than once, even.
    * I've regretted that decision, EVERY TIME.
    * If you're going to use separate Contract and Implementation projects, keep the component definitions separate from each other, organized and named within the taxonomy rules.

I'll use either of these, even within a single solution, and I'm constantly fighting with myself over whether I should always use "Option 2".  

I've had scenarios where there's not a compelling case to add "yet another class library project" to my solution, and collapsed Contract & Implementation code into the same project, but it always feels a little funny to do so.  (Consistency is good.  It reduces head scratching.  I really ought to take my own advice.)

***How do I decide which one is correct?***  
In my opinion, you've got a little bit of flexibility here.  Remember though, High Consistency in your naming and code organization pays off with Low Cognitive Load later.

#### **Resource Access** components should ALWAYS have separate Contract & Implementation projects.  

If your component:  
1. Accesses a Database, File System, or any kind of Storage service
2. Wraps an external API of any kind
3. Wraps access to external IoT devices
4. Communicates any kind of ML model, LLM, or AI system

The DESCRIPTION and EXPECTATIONS of those components (within your system design,) are going to be relatively stable over time.  The implementations need to be changeable, sometimes quickly.  (API Vendors change their requirements without consulting you.  Sometimes we have to update the underlying connectivity assemblies for accessing storage services.  There's a lot of things outside of our control with Resource Access.  Keeping the Contracts stable and separate from the implementation allows us to change those implementation details as needed, without affecting the rest of the system.)  

Additionally:  Never, ever ever expose the "Storage" or "API" model types as the public DTOs for your ResourceAccess contracts.  The purpose of these Public Contracts is to protect the rest of your system from changes in the underlying resources.  By pushing those storage models throughout your application code, you're dooming yourself to playing catch-up with the data people.

#### **Engines and Managers** *CAN* have their contracts in the same project as their implementations.  
I find that the Contracts and Implementations tend to change on the same cadence, and I don't often have a need to replace the implementation of an Engine or Manager without significant changes to their Public Interfaces.  

***Keeping the contracts in a sub-folder in the project let's me pull them out later if I need to, though.***

Here's a sample solution structure I fall back on over and over again:  (Use whatever structure works best for you, of course.)
```
├─ /src  
│  (Solution files used to build or test the solution)  
│  ├─ /Clients
│  │  ├─ /Company.Product.PublicAPI
│  │  ├─ /Blazor (UI coded w/ Blazor) 
│  │  │  ├─ /Company.Product.BlazorUi (or MvcUi, or whatever...)  
│  │  │ /Web  ("Pure Web" technologies i.e.: React, Angular, etc...)  
│  ├─ /Engines  
│  │  ├─ /Company.Product.Engine.Randomization  
│  │  │  ├─ /Public  
│  │  │  │  ├─ IRandomizer.cs  (ServiceContract)   
│  │  │  │  ├─ DiceTray.cs     (DataContract)  
│  │  │  ├─ Randomizer.cs      (Implements Public.IRandomizer)  
│  ├─ /iFX  
│  │  ├─ /Company.iFX
│  │  ├─ /Company.Product.iFX
│  ├─ /Managers  
│  │  ├─ /Company.Product.Manager.Npc  
│  │  │  ├─ /Public  
│  │  │  │  ├─ INpcManager.cs    *(ServiceContract)*  
│  │  │  │  ├─ Townsperson.cs    *(DataContract)*  
│  │  │  ├─ NpcManager.cs        *(Implements INpcManager)*  
│  ├─ /ResourceAccess  
│  │  ├─ /Company.Product.Resources.Npcs.Abstractions  
│  │  │  ├─ INpcAccess.cs       (ServiceContract)  
│  │  │  ├─ NpcResource.cs      (DataContract)   
│  │  ├─ /Company.Product.Resources.Npcs.SqlServer*  
│  │  │  ├─ NpcSqlProvider.cs   (Implements INpcAccess against SQL Server)  
│  │  │  ├─ /Models  
│  │  │  │  ├─ NpcDbContext.cs  (internal class, EF DbContext for the Resource Provider)  
│  │  │  │  ├─ NpcEntity.cs     (internal class, tailors the NPC object for SQL)    
│  │  ├─ /Company.Product.Resources.Npc.AzureTableStorage  
│  │  │  ├─ NpcAzureTableProvider.cs (implements INpcAccess against Azure Table Storage)  
│  │  │  ├─ /Models  
│  │  │  │  ├─ NpcTableEntity.cs (internal class, represents the NPC as a TableEntity) 
... and so on ... 
```

** A note about the iFX projects:**  
If you have a company-wide internal framework, it's likely a good idea to turn that into a nuget package, and host it in a private company package repository.  This keeps people from going in a "tweaking" the source of your org's framework.  
* Company-Wide iFX should avoid specifying Domain concepts.
* Company-Wide iFX is for standardizing code patterns used throughout the organization's code bases.
* Company-Wide iFX should be curated by an engineering group that will "own" that framework.

The Product-Level iFX project SHOULD BE embedded as source-code within the project that it applies to.
* Product-Level iFX is "owned" by the team building the Product.
* Product-Level iFX ^can^ contain non-volatile Domain concepts that are used throughout the Product's codebase, as long as those concepts are used in the same way, and mean the same thing thoughout the Product's Codebase.
* Product-Level iFX is useful for ***non-volatile*** Constants, Enums, well-known and relatively static Legal Value Lists, etc...
