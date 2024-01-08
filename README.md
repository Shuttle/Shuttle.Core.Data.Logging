# Service Bus Logging

```
PM> Install-Package Shuttle.Esb.Logging
```

## Configuration

```c#
services.AddServiceBusLogging(); // all logging options enabled
```

Specific logging options may be specified:

```c#
services.AddServiceBusLogging(builder =>
{
	builder.Options.PipelineTypes = new List<string> { "pieline-type-name" };
	builder.Options.PipelineEventTypes = new List<string> { "pieline-event-type-name" };
	builder.Options.AddPipelineType<PipelineType>();
	builder.Options.AddPipelineType(pipelineType);
	builder.Options.AddPipelineEventType<PipelineEventType>();
	builder.Options.AddPipelineEventType(pipelineEventType);
});
```