# Event Sorcery

*build statuses go here.*

[![Twitter Follow](https://img.shields.io/twitter/url?label=reach%20me%20%40Faelor&style=social&url=https%3A%2F%2Ftwitter.com%2Ffaelor)](https://twitter.com/faelor)

## What is Event Sorcery?

Event Sorcery is a tool for .NET developers to assist with application development surrounding the software design patterns of Event Sourcing and CQRS.

This library for .NET is:
- Unambitious
- Opinionated
- Currently designed for usage with [Marten](https://martendb.io/) and [MediatR](https://github.com/jbogard/MediatR).

This all has been largely driven by finding myself writing the similar snippets of code over and over for multiple projects.

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