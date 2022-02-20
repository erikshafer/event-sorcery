# Event Sorcery

*build statuses go here.*

[![Twitter Follow](https://img.shields.io/twitter/url?label=reach%20me%20%40Faelor&style=social&url=https%3A%2F%2Ftwitter.com%2Ffaelor)](https://twitter.com/faelor)

## What is Event Sorcery?

Event Sorcery is a tool for .NET developers to assist with application development surrounding the software design patterns of Event Sourcing and CQRS.

This library for .NET is:
- Opinionated. To help accelerate a project's initial development.
- Subscribes to various tenants of DDD, CQRS, Event Sourcing, and Asynchronous Messaging.
- Built on top of proven, battle-worn, and time-tested technologies.

This all has been largely driven by finding myself writing the similar snippets of code over and over for multiple projects.

## Getting Started

Event Sorcery is written for .NET 6. With 2020's release of .NET 5 we finally had a single successor to both .NET Core 3.1 and .NET Framework 4.8. With that unified vision for the .NET world. While many years-long issues being resolved, .NET 6 was able to focus on performance instead of compatability. This is why it is our starting point.

## Documentation

Coming soon.


## Acknowledgements

- [EventStoreDB](https://www.eventstore.com) - The first event sourcing focused database. Its documentation, community, and developer advocates are what got me interested in event sourcing as a viable solution for problems I was needing to solve.
- [Marten](https://martendb.io) - The library that introduced me to Event Sourcing and the first target for adoption for this helper library.
- [EventSourcing.DotNet](https://github.com/oskardudycz/EventSourcing.NetCore) - A wonderful resource on how to adopt event sourcing for .NET developers. Teaches how to handroll a custom event store, along with how to work with both Marten and EventStoreDb.
- [Eventuous](https://eventuous.dev) - A project that deserves more attention than it has. A large motivator for creating Event Sorcery.
- [Akkatecture](https://github.com/Lutando/Akkatecture) - A project that influenced API design and urged further adaption of DDD patterns.


## Change Log

Actual log coming soon. Currently high-level notes for self.

2022-Feb-19: Continued experimenting with interfaces and DDD-oriented patterns
2021-Dec-29: Branch pushed that experiments with aggregate and domain model patterns
2021-Dec-12: Stood up project after talking about it for months.


## License

[MIT license](../../LICENSE).