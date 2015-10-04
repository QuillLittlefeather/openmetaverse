# Introduction #

Simian started out as an experiment in virtual world simulation software before growing into an umbrella of projects related to virtual world hosting and simulation. SimianGrid is a set of PHP web services that provide the central backend for a virtual world.

# Architecture #

![http://openmetaverse.googlecode.com/svn/wiki/simiangrid-architecture.png](http://openmetaverse.googlecode.com/svn/wiki/simiangrid-architecture.png)

SimianGrid follows a three tier (data, logic, presentation) architecture with the logic tier split into two levels for internal and external access levels.

## Data Tier ##

The data tier holds data for the entire system on database and file servers. Any persistent state must be held in the data tier to allow independent scalability and replication.

## Logic Tier ##

There are three parts of the logic tier: core web services, external web services, and capabilities. The core web services are an exclusive thin wrapper API for the data tier. All data reads and writes must come through the core web services as an authorized request (with the exception of public asset fetching). The external web services present a wide range of service endpoints with varying security requirements and data formats. Some are publicly accessible, many require a form of authorization. Multiple protocols may be supported in the external web services to allow compatibility with a variety of application protocols. Capability resources act as authorization proxies to allow access to protected web services.

## Presentation Tier ##

Virtual world simulators and the frontend website make up the presentation tier. Connections may be made to capabilities, external web services, or directly to core web services depending on the trust level of the simulator or frontend.

# How Does This Compare to OpenSim's ROBUST? #

The OpenSim project provides a set of HTTP-based grid services written in C# that provides the default backend for an OpenSim grid. These C# services are nicknamed ROBUST. The OpenSim simulator code also provides connectors to SimianGrid services, making switching between the two as easy as modifying your OpenSim configuration. SimianGrid has a few characteristic differences from the ROBUST services:

  * **SimianGrid is a grid service API first and an implementation second.** The HTTP API is the most important aspect of virtual world services. Database layer choices, content storage, reporting requirements, and more vary between deployments. The constant factor is the API that developers will program against.
  * **SimianGrid is a thin persistence layer.** Using a web scripting language like PHP encourages the logic tier to be stateless, decreasing the chance of memory leaks and concurrency problems. This also allows the logic tier to scale horizontally. SimianGrid getting hit too hard? Add more off the shelf web servers and cache servers.
  * **SimianGrid provides a web frontend.** All of the web frontend interactions route through SimianGrid instead of directly to the database layer, allowing your website to share the same common API, security model, and scalability provided to your virtual world. Features such as WebDAV inventory and the upcoming draft VWRAP implementation make it easier to connect your world with other worlds, whether they are virtual or on the web.

Aside from architecture and feature differences, SimianGrid and ROBUST provide a similar model of a virtual world grid and expose common services. The diagrams below show the common grid model that each set of services implement.

<a href='http://openmetaverse.googlecode.com/svn/wiki/simiangrid-services-01.png'><img src='http://openmetaverse.googlecode.com/svn/wiki/simiangrid-services-01.png' align='left' width='364' height='383' /></a><a href='http://openmetaverse.googlecode.com/svn/wiki/robust-services-01.png'><img src='http://openmetaverse.googlecode.com/svn/wiki/robust-services-01.png' align='left' width='364' height='383' /></a>