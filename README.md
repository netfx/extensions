NetFx Extensions
==========

Extension methods and lightweight extensions that are shipped as source code via content-only nuget packages.


## What

Lightweight NuGet packages with reusable source code extending core .NET functionality, typically in self-contained source files added to your projects as internal classes that can be easily kept up-to-date with NuGet.

See available packages documentation (work in progress) or the fairly extensive and ever growing list.

## Why

Who doesn’t have an ever growing and ever less cohesive miscellaneous collection of helpers, extension methods and utility classes in the usual “Common.dll”? Well, the problem is that there’s really no good place for all that baggage: do we split them by actual behavioral area and create “proper” projects for them?

In most cases, that’s totally overkill and you end up in short time with the same pile of assorted files as you try to avoid setting up an entire new project to contain just a couple cohesive classes.

But, it turns out that in the vast majority of cases, those helpers are just meant for internal consumption by the actual important parts of your code. In many cases, they are just little improvements and supplements over the base class libraries, such as adding missing overloads via extension methods, adding factory methods for otherwise convoluted object initialization, etc. It’s almost inevitable that as the .NET framework and its languages evolve, existing APIs will start to look dated and lacking (i.e. lack of generics from v1 APIs, or eventually lack of async-friendly Task-based APIs once C# 5 is with us, etc.).

But with the advent of NuGet there’s a new way to maintain, evolve and share those useful little helpers: just make them content files in a NuGet package!

And thus NETFx was born: a repository of the source and accompanying unit tests for all those helpers, neatly organized (under the Extensions folder in the source tree, by target namespace being extended), deployed exclusively using NuGet, and licensed entirely under BSD for everyone to use and contribute.
