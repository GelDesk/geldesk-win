# GelDesk Architecture

## The Problem

The problem is that there is still no quick and easy way to build *robust* 
native desktop user interfaces that can be used by any programming 
language in an application. There are many different paths to choose from, but 
each comes with drawbacks. For instance: 

- The very best results can be had by building a separate app for each 
environment using each OS' default native kit such as Cocoa, Winforms or WPF...
but, that takes a lot of work and skill.
- Cross-platform native kits like Qt and Xamarin can help save some of that 
effort...but they're still just wrappers around native APIs that add their 
own complications, kruft and limitations like which language can be used.
- Hybrid frameworks like NW.js and Electron currently offer access to a 
very limited set of native UI components and the ones that they do offer
access to have a small feature-set. Everything else must be HTML. 
- Even if it is decided to just build a web-only front-end instead, a lot of 
work must still be done to have decently performing high-level components.

## The Solution

The GelDesk solution is to run the UI in it's own process and the application 
logic in it's own process. That way, the UI components can be built with the 
OS' default native kit and the application logic can be built with any 
programming language.

This has been done before. Hopefully, GelDesk will do it better.

## Basic Story

So, we're going to have the UI in a separate process from the application 
logic and use some sort of inter-process communication (IPC) between them.

*Should the logic process launch the UI process or vice-versa?*

Both should be possible, eventually. The first implementation will be that 
the UI process starts first and then spawns a child process for the 
application logic.

*How should the logic process communicate with the UI?*

As quickly as possible! There are a range of inter-process communication (IPC) 
options to choose from, but the most basic is anonymous pipes (*aka 
`stdin/stdout/stderr`*). Let's start with those.

*What protocol should be used over the IPC channel?*

A text-based remote procedure call (RPC) style protocol is being used here for 
various reasons. The chief technical reason is that it should be easy to 
implement in any language or environment.

## Run Time

Here is generally what should happen at run time:

- The UI process starts and reads a config file from somewhere.
- The config file tells the UI how to run the logic process and connect to it. 
- The UI receives one or more RPC messages from the logic process telling it
what to do.
