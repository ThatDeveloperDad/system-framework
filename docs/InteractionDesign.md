[Table of Contents](./_ToC.md)
# Detailed Design - Component Interactions
## "Have your people talk to my people..."  ***PROXY MADNESS!!!***
---
For now, we're not going to talk about complex topics like Message based communication.  Instead, we'll focus on the baseline interaction pattern:  Direct component invocation, but marshalled through ***Proxies***.

One of the goals for ThatDeveloperDad.iFX was to help me use this Proxy Pattern throughout my own applications, WITHOUT over-complicating the component code itself.  

### Proxies?!?  What the heck do you mean?  Isn't it enough that I'm using Interfaces?
Well, Interfaces (or Contracts if you wanna be all fancy & generic about it,) are a great start and a fantastic way to "Talk Through" and "Reason About" your call chains at a higher cognitive level.  They remove the need to talk in terms of high crunch-factor code entities and stuff like that.  They allow you to anthropomorphise the different parts of your system.

Interfaces DO let you change the "HOW" or "WHERE" of a component so that you don't break your consumers.

Sometimes though, we need to change what happens WHEN we call the components in our system, and when we're just using interfaces & classes, that means we have to alter those classes, sometimes significantly.  And it usually means we have to add code to our components that really doesn't have all that much to do with those component's jobs in our system.

Interfaces DON'T keep you from having to revise your classes when you need to add ^this specific kind of log message^ to EVERY CALL.  (Yes, those technical requirements matter, and you really don't want to have them crop up in your discussions with non-technical folks.)

Interfaces DON'T help you when you realize "Oh Crap, this part is bottlenecking my process, and I really ought to put it behind a Queue to give myself some load-leveling".

***This is where the concept of a Proxy object comes in super handy.***

All a proxy REALLY is, is a wrapper around a component instance that lets you do "extra work" on either side of the component call.  (Logging, generalized error handling, retry logic in case of transient errors, stuff like that...  There are more applications that I'm not ready to discuss just yet.  We'll add those once I am.)

### Why do I care about Proxies, and why should you?  
"What if I told you that you could add method-call timers to every component in your system without having to manually add the code to every component and method in your system?"

See, sometimes, we'll run into scenarios where the functionality remains stable, but we need to make material changes to what happens AROUND invoking our components.  

Sure, we could crack open the class file and add the logging statement.  We could change the code so that it handles errors differently in different scenarios.

But what happens when we want to apply that particular change everywhere?  If we have hundreds of component invocations throughout our system, and we haven't had any standardized logging until now, we have to change ALL THE THINGS.  (And that gets expensive & risky.)  It also pollutes our nice, clean business code with a bunch of technical crap, making it MUCH harder to read and change later.

So what to do?  Well, instead of injecting an instance of the class that implements our desired dependency interface, what if we wrap that class in a transparent "proxy" that, to our code, "Looks exactly like" the class, but also lets us run other things before and/or after the call gets to the actual class?

## PassthroughProxy  
For now, I've implemented the simplest possible proxy, I call it the PassthroughProxy.  Luckily, it's largely invisible when you're developing your components.  The SystemFramework takes care of instantiating it, and from the view of your classes and interfaces, you don't need to change a thing.

Behind the scenes, the components you code get wrapped in "Module Providers" that take care of each component's dependency graph and configuration.  When I register a ModuleProvider, I also add the standard "services.AddScoped<IContract>" stuff with a little bit of added magic.  

When your classes get requested from the DI service provider, it calls a "factory method" on the ModuleProvider for that service that:  
1. Obtains the properly configured instance of the configured implementation type.
2. Wraps it in a PassthroughProxy
3. Returns that PassethroughProxy as the "service" that satisfies the dependency contract.

***What's so special about this PassthroughProxy?***
The PassthroughProxy class itself has a method called "AddBehavior" which accepts an IOperationBehavior instance, and optionally, a method name to which the Behavior would apply.

An IOperationBehavior describes two methods:  
* OnMethodEntry:  Code in ths method executes BEFORE the component call is made.
* OnMethodExit:  Surprise surprise, executes AFTER the component call is made.

The PassthroughProxy can have a collection of these behaviors, each of which is inserted into the call chain around your component invocation.

***Warning**: We're not NESTING these behaviors just yet, so I've not added any kind of ordering facility within the Behaviors collection*
While the behaviors receive a "MethodContext" object, that MethodContext should currently be used only for reference within the Behavior, and not used to transform inputs or outputs.

I've included a sample "CallTimerBehavior" in the Utilities namespace that demonstrates what this concept can do for us.
