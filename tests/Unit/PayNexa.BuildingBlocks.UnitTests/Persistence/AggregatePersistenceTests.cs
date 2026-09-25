using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using PayNexa.Common.Security;
using PayNexa.SharedKernel.Domain;
using PayNexa.SqlServer.Auditing;
using PayNexa.SqlServer.Conventions;

namespace PayNexa.BuildingBlocks.UnitTests.Persistence;

public sealed class AggregatePersistenceTests
{
    private static readonly DateTime Now = new(2026, 9, 25, 8, 0, 0, DateTimeKind.Utc);

    public sealed class SampleId : StronglyTypedId
    {
        private SampleId(Guid value) : base(value)
        {
        }

        public static SampleId CreateUnique() => new(Guid.CreateVersion7());
    }

    public sealed class SampleCode : SingleValueObject<string>
    {
        private SampleCode(string value) : base(value)
        {
        }

        public static SampleCode Create(string value) => new(value);
    }

    public sealed class SampleAggregate : AggregateRoot<SampleId>
    {
        private SampleAggregate()
        {
            Code = null!;
        }

        private SampleAggregate(SampleId id, SampleCode code) : base(id) => Code = code;

        public SampleCode Code { get; private set; }

        public static SampleAggregate Create(string code) => new(SampleId.CreateUnique(), SampleCode.Create(code));

        public void Rename(string code) => Code = SampleCode.Create(code);
    }

    private sealed class SampleDbContext : DbContext
    {
        public DbSet<SampleAggregate> Samples => Set<SampleAggregate>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlServer("Server=unit-test-only;Database=Samples");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplySingleValueObjectConversions();
            modelBuilder.Entity<SampleAggregate>().HasKey(sample => sample.Id);
            modelBuilder.ApplyAuditingConventions();
        }
    }

    private sealed class FixedActor(string id) : ICurrentActor
    {
        public string Id => id;
    }

    [Fact]
    public void Conventions_MapSingleValueObjectsAuditColumnsAndConcurrencyToken()
    {
        using var context = new SampleDbContext();
        var entity = context.Model.FindEntityType(typeof(SampleAggregate))!;

        entity.FindProperty(nameof(SampleAggregate.Id))!.GetValueConverter().ShouldNotBeNull();
        entity.FindProperty(nameof(SampleAggregate.Code))!.GetValueConverter()!.ProviderClrType.ShouldBe(typeof(string));
        entity.FindProperty(nameof(IAuditable.CreatedBy))!.GetMaxLength().ShouldBe(AggregateModelConventions.ActorMaxLength);
        entity.FindProperty(nameof(IVersioned.Version))!.IsConcurrencyToken.ShouldBeTrue();
        entity.FindNavigation(nameof(IHasDomainEvents.DomainEvents)).ShouldBeNull();
    }

    [Fact]
    public void Stamper_OnAdd_SetsCreatedAndUpdatedAuditAndFirstVersion()
    {
        using var context = new SampleDbContext();
        var sample = SampleAggregate.Create("A");
        context.Samples.Add(sample);

        Stamper("alice").Stamp(context);

        sample.CreatedAtUtc.ShouldBe(Now);
        sample.CreatedBy.ShouldBe("alice");
        sample.UpdatedBy.ShouldBe("alice");
        sample.Version.ShouldBe(1);
    }

    [Fact]
    public void Stamper_OnModify_UpdatesAuditAndIncrementsVersionKeepingCreation()
    {
        using var context = new SampleDbContext();
        var sample = SampleAggregate.Create("A");
        context.Samples.Add(sample);
        Stamper("alice").Stamp(context);
        context.ChangeTracker.AcceptAllChanges();

        sample.Rename("B");
        Stamper("bob").Stamp(context);

        sample.CreatedBy.ShouldBe("alice");
        sample.UpdatedBy.ShouldBe("bob");
        sample.Version.ShouldBe(2);
        context.Entry(sample).Property(nameof(IVersioned.Version)).OriginalValue.ShouldBe(1L);
    }

    [Fact]
    public void Stamper_UnchangedEntity_IsNotStamped()
    {
        using var context = new SampleDbContext();
        var sample = SampleAggregate.Create("A");
        context.Samples.Add(sample);
        Stamper("alice").Stamp(context);
        context.ChangeTracker.AcceptAllChanges();

        Stamper("bob").Stamp(context);

        sample.UpdatedBy.ShouldBe("alice");
        sample.Version.ShouldBe(1);
    }

    private static AuditStamper Stamper(string actor) =>
        new(new FixedActor(actor), new FakeTimeProvider(new DateTimeOffset(Now)));
}
