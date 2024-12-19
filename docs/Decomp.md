[Table of Contents](./_ToC.md)
# Decomposition: Functional vs. Volatility

Huh?  Wut???  

This is probably the hardest part to wrap your brain around.  I haven't found a way to describe the thought process as a repeatable set of steps yet.  It's really something you have to experience to figure out.  

So instead, I'll explain the What and the Why.

### Before I dive into this...
My goal here isn't to fully explain Functional Decomposition vs. Volatility based Decomposition.  There are people who are FAR smarter than me that have written books and given talks about this for decades.  

All I hope to accomplish here is to introduce the CONCEPT, and perhaps plant the seeds of curiosity that you can water with a little bit of internet searching.  (Look up *"Righting Software" by Juval Lowy* and then follow the trail down the YouTube rabbit hole.)

At the end of the day, we want a software system that:  
1.  Can be delivered on time.
2.  Can be delivered with quality.
3.  Building it and running it doesn't break the bank.

Extra Special Bonus Points if you can build a system that can be changed as needed without having to burn it all to the ground and start from scratch every time Jerry from Marketing comes up with a Hot New Thing<sup>tm</sup>

### TLDR:
Identifying the "Chunks" of your system by what they do is called Functional Decomposition, and usually leads to scenarios where you have to change many parts of your application whenever some detail in your system needs to be modified in some way.  You end up in a place where you're always playing catch-up and dealing with *"Huh, I didn't expect THIS to affect THAT..."*  (I've been there, you've been there.  We've all been there.)

Instead, with a little bit of extra thought at the start, we can identify the general Activities that are performed within the system, and come up with ways to describe those generalized activities that DON'T change in material ways that would make us update code in all kinds of unexpected places.  Then, we stitch those generalized components together with code that we expect to experience change, and the whole thing becomes much more stable.  This is the essence of Volatility Decomposition.

## Functional Decomposition  
This is likely the kind of application decomp you're most used to.  You need a "screen", that'll accept a bunch of values, and you need a place to put those values so you can do something with them later.  

So, you build a webpage with a form, and add some API calls to some server that takes the data from your screen, makes sure the data is "right" and stuffs it in a database.  

Later, you need to show a list of the things, so you make another page that calls a different API to get the List of Things from the database and then shows them in a pretty grid.  

Then, you need to be able to find a Thing by say, its name.  So you add another API method that returns the Things that match that name.  And then, while you're doing that, you find out you need to get the Things by some other attibute, so you add another endpoint for that attribute.  

*"Heck, that didn't take too long.  I might as well add the other attributes as Search endpoints while I'm at it.  I don't need em now, but Jerry the Product Manager's a real pain, and I know he's gonna tell me at lunch on Friday that he needs it for the weekend...."*

Jerry's overjoyed!  Until Monday, when he comes to you and says, *"How hard would it be to split that Thing into a Master-Detail type whatsit, and send the Details somewhere, because the Accounting folks really need that for an audit.  That's happening Wednesday, by the way.  You got this, you're a super hero!"*

So you have to burn all the code you wrote and start over, because you never thought there'd be that kind of change come down the pipe.

This is where Functional Decomposition takes you.

## Volatility Decomposition
This is a little trickier, because it's WAY more abstract than we're used to.

Same scenario, different approach.

You build your front-end just like you always have.

At the API, you spin things a little differently.  You know Jerry's a bit of a wild card and shoots fromt the hip when it comes to requirements.

Instead of going directly from the API to the database, you know that the Storage stuff is going to change over time.  You also know that the search attributes are going to vary from one day to the next.

Instead of dozens of "Get_Thing_By_Attribute" API methods, you devise a "Filter_Thing" API method, and accept a collection of Key-Value pairs on that.  Now, you can search by whatever you want from the Front end without changing the API.  Sweet.

(You've just encapsulated that kind of change into a single, configurable concept.)

Next, because you remember Last Time... You realize that the ACTIVITIES around The Things didn't really change all that much.  You still need to Save, List, and Load, but the DETAILS behind those activities is subject to Jerry's whims.  

*"Let's hide the details of those database activities inside  something that ISN'T likely to change all that much, so that when Jerry comes to ruin my week, he only ruins it a little bit."*

You've once again hidden the Area that's Subject to Change from the rest of the system.

This is Volatility based Decomposition.

## Sounds sweet... Tell me more?
Let's start with the Why behind Volatility Decomp.

The Goal of this is to describe a limited number of components that:

1:  Have a Stable public surface and are Shared between use cases.
or
2:  Fully encapsulate the ways in which the component's behavior will change over time behind a stable programming interface.

Once we've made those determinations, we can then assemble configurations of those pieces to create the functionality we need to deliver.

So what we need to do is take a wide view of what the system we're designing needs to be able to do.  We're going to get dozens, perhaps scores of Use Cases dropped on our desks at the start of a new project.

We need to study those Use Cases and by doing so, we'll identify that the vast majority of them fall into a much smaller set of categories.  Perhaps the details are widely variable from one use case to another within a category, but the high-level concepts are shared. These concepts don't really change all that much over time, in terms of their broad behavior.

### The Vectors of Volatility
As you look over these Categorized Use Cases, and the Steps for each use case, you can ask yourself the following questions:

"Is this piece going to change every release cycle?"
  - That is, we KNOW that the requirements of the component are going to experience material changes to its behavior and calling semantics.  Our task with these parts of the system is to ensure that the impact of those changes is limited to a small area of our system.  
  - Change over time is represented in mathematics by the Greek letter Delta, so I use that term to refer to components that can be considered to experience "High Delta".

"Is this piece the same from one Use to the Next?"
  - That doesn't mean the usage is EXACTLY the same between different use cases.  The input & output values can vary.  Heck, even the input & output TYPES can vary.  But the described behavior of a shared piece of code must be stable over time.  
  - I've had a bit more trouble finding a symbolic designation for components that are widely shared, but I've settled on Sigma for this, to describe components that are consumed by a set of other components.

Only one of those questions should get a "Yes" answer for each piece of the system.

 - If you see that both questions can be answered "Yes," you must more closely inspect that aspect of the use case, and further decompose it into smaller parts that do satisfy our XOR constraint for the two vectors.

 - If the answer to both questions is "No", then that piece is non-volatile and you don't need to do any encapsulation of it.  Hard code it wherever it sits.

 ### Then, you look at your volatilities and try to build a more manageable grouping of them.
 You might end up with quite a few parts with a high Delta.  Study those, and try to sort them into groups based on either the Frequency or Reason for those parts to change.  If you've identified common conditions that incur material change for  parts of the use case and can collapse THOSE parts into a single component, you'll reduce the total number of services you need to deal with.  That's never a bad thing.
