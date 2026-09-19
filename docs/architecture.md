# Architecture and design decisions

## Scope

Smart Light Controller is a single-process automation reference implementation. It demonstrates one scheduled output and a defensive response to an unavailable time source. It does not model device fleets, messaging infrastructure, persistence, or physical hardware.

## Responsibilities

- `LightSchedule` decides whether a local wall-clock time is inside the configured window. It has no I/O or mutable state.
- `LightController` reads time, tracks consecutive provider failures, enters or leaves safe mode, and applies state changes only when the simulated output must change.
- `HttpTimeProvider` performs cancellable HTTPS requests with a reused `HttpClient`, validates status and JSON, and translates expected transport/data problems into `TimeProviderException`.
- `ConsoleLightOutput` is an honest demo adapter. It stores its current state and logs transitions; it does not claim to control hardware.
- `LightControllerWorker` owns the asynchronous polling lifecycle and stops through host cancellation.

## Time semantics

The external provider returns `DateTimeOffset`, preserving the reported UTC offset. The configured time-zone identifier is appended to the provider base URI, so scheduling is evaluated using the provider's reported local clock. This avoids silently using the host machine's time zone.

The schedule is a half-open interval: start is included and end is excluded. An overnight schedule uses the union of times at or after the start and times before the end. Equal boundaries represent an empty interval.

## Failure model

Only expected time-provider failures contribute to safe mode. Unexpected controller or output-adapter defects are not swallowed; allowing the host to surface them preserves diagnostic context. Below the configured threshold, the last known output state is retained. At and above the threshold, the output is forced off. Any successful read clears the consecutive count and reapplies the schedule.

## Deliberate trade-offs

- There is one options object because the configuration surface is small.
- There is no retry library: the worker's normal polling interval already supplies bounded retries, and the controller needs every failure to count toward safe mode.
- There is no repository, factory, mediator, or persistence layer because none serves the current behavior.
- The default runtime provider is external for demonstration purposes, while all automated tests replace the HTTP transport and remain deterministic.
