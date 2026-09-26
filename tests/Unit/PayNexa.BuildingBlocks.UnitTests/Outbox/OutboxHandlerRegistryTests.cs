using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PayNexa.Common.Persistence;
using PayNexa.SqlServer.Outbox;

namespace PayNexa.BuildingBlocks.UnitTests.Outbox;

public sealed class OutboxHandlerRegistryTests
{
    public sealed record SampleEvent(Guid Id, string Name);

    [Fact]
    public async Task FromServices_DiscoversHandlersAndDispatchesToEveryOne()
    {
        var first = Substitute.For<IOutboxMessageHandler<SampleEvent>>();
        var second = Substitute.For<IOutboxMessageHandler<SampleEvent>>();
        var services = new ServiceCollection()
            .AddSingleton(first)
            .AddSingleton(second);

        var registry = OutboxHandlerRegistry.FromServices(services);
        await using var provider = services.BuildServiceProvider();
        var message = new SampleEvent(Guid.NewGuid(), "customer");

        registry.TryGetDispatcher(typeof(SampleEvent).FullName!, out var dispatch).ShouldBeTrue();
        await dispatch!(provider, JsonSerializer.Serialize(message, JsonSerializerOptions.Web), TestContext.Current.CancellationToken);

        await first.Received(1).HandleAsync(message, Arg.Any<CancellationToken>());
        await second.Received(1).HandleAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void TryGetDispatcher_UnknownType_ReturnsFalse() =>
        OutboxHandlerRegistry.FromServices(new ServiceCollection()).TryGetDispatcher("Unknown.Type", out _).ShouldBeFalse();
}
