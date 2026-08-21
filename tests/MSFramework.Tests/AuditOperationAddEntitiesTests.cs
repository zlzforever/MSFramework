using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// <see cref="AuditOperation.AddEntities"/> 防重收集测试：
/// 残留处理器可能对同一请求重复触发收集，且每次收集都会新建 <see cref="AuditEntity"/> 实例
/// （引用不相等），因此必须按值身份（类型 + 实体标识 + 操作类型）去重，
/// 保证同一实体的同一变更状态只收集一次，同时保持 SetOperation 关联行为不变。
/// </summary>
public class AuditOperationAddEntitiesTests
{
    /// <summary>
    /// 构建测试用审计操作
    /// </summary>
    /// <returns>审计操作实例</returns>
    private static AuditOperation CreateOperation()
    {
        return new AuditOperation("/orders", "ua", "1.2.3.4", "iPhone", "device-1",
            null, null, "trace-1", "POST");
    }

    /// <summary>
    /// 同一次收集内出现相同值身份（类型+标识+操作类型）的不同实例时，只保留第一个，且 SetOperation 正常关联
    /// </summary>
    [Fact]
    public void AddEntities_DuplicateValueIdentity_CollectedOnce()
    {
        var operation = CreateOperation();
        var first = new AuditEntity("Order", "O1", OperationType.Modify);
        var second = new AuditEntity("Order", "O1", OperationType.Modify);

        operation.AddEntities([first, second]);

        var entity = Assert.Single(operation.Entities);
        Assert.Same(first, entity);
        Assert.Same(operation, entity.Operation);
    }

    /// <summary>
    /// 同一收集批次内重复触发收集场景：每次收集都会新建实例（引用不同），按值身份去重后仍只收集一次
    /// </summary>
    [Fact]
    public void AddEntities_RepeatedCollectionSameState_CollectedOnce()
    {
        var operation = CreateOperation();

        operation.AddEntities(
        [
            new AuditEntity("Order", "O1", OperationType.Modify),
            new AuditEntity("Order", "O1", OperationType.Modify),
            new AuditEntity("Order", "O1", OperationType.Modify)
        ]);

        var entity = Assert.Single(operation.Entities);
        Assert.Equal("Order", entity.Type);
        Assert.Equal("O1", entity.EntityId);
        Assert.Equal(OperationType.Modify, entity.OperationType);
        Assert.Same(operation, entity.Operation);
    }

    /// <summary>
    /// 同一实体不同操作类型（如先 Add 后 Modify）属于不同变更状态，必须都收集
    /// </summary>
    [Fact]
    public void AddEntities_DifferentOperationTypes_BothCollected()
    {
        var operation = CreateOperation();

        operation.AddEntities(
        [
            new AuditEntity("Order", "O1", OperationType.Add),
            new AuditEntity("Order", "O1", OperationType.Modify)
        ]);

        Assert.Equal(2, operation.Entities.Count);
        Assert.Contains(operation.Entities, x => x.OperationType == OperationType.Add);
        Assert.Contains(operation.Entities, x => x.OperationType == OperationType.Modify);
    }

    [Fact]
    public void AddEntities_SameOperationTypeWithDifferentPropertySnapshots_BothCollected()
    {
        var operation = CreateOperation();
        var first = CreateModifiedEntity("before-1", "after-1");
        var second = CreateModifiedEntity("before-2", "after-2");

        operation.AddEntities([first, second]);

        Assert.Equal(2, operation.Entities.Count);
        Assert.Contains(operation.Entities, entity =>
            entity.Properties.Single().NewValue == "after-1");
        Assert.Contains(operation.Entities, entity =>
            entity.Properties.Single().NewValue == "after-2");
    }

    [Fact]
    public void AddEntities_SameSnapshotAcrossCollectionBatches_PreservesEachBatch()
    {
        var operation = CreateOperation();
        var firstBatch = CreateModifiedEntity("before-1", "after-1");
        var secondBatch = CreateModifiedEntity("before-1", "after-1");

        operation.AddEntities([firstBatch]);
        operation.AddEntities([secondBatch]);

        Assert.Equal(2, operation.Entities.Count);
        Assert.Contains(operation.Entities, entity => ReferenceEquals(entity, firstBatch));
        Assert.Contains(operation.Entities, entity => ReferenceEquals(entity, secondBatch));
    }

    /// <summary>
    /// 同一操作类型不同实体标识属于不同实体，必须都收集
    /// </summary>
    [Fact]
    public void AddEntities_DifferentEntityIds_BothCollected()
    {
        var operation = CreateOperation();

        operation.AddEntities(
        [
            new AuditEntity("Order", "O1", OperationType.Modify),
            new AuditEntity("Order", "O2", OperationType.Modify)
        ]);

        Assert.Equal(2, operation.Entities.Count);
    }

    /// <summary>
    /// 空集合与 null 元素必须被安全跳过，不影响其他实体收集
    /// </summary>
    [Fact]
    public void AddEntities_NullEntities_SkippedSafely()
    {
        var operation = CreateOperation();

        operation.AddEntities(null);
        Assert.Empty(operation.Entities);

        operation.AddEntities([null, new AuditEntity("Order", "O1", OperationType.Add)]);
        Assert.Single(operation.Entities);
    }

    [Fact]
    public async Task AddEntities_ConcurrentWriters_PreserveEveryEntity()
    {
        var operation = CreateOperation();

        await Task.WhenAll(Enumerable.Range(0, 32).Select(worker => Task.Run(() =>
        {
            for (var index = 0; index < 100; index++)
            {
                var entity = new AuditEntity("Order", $"O-{worker}-{index}", OperationType.Modify);
                operation.AddEntities([entity]);
            }
        })));

        Assert.Equal(3200, operation.Entities.Count);
        var entityIds = operation.Entities.Select(entity => entity.EntityId).ToArray();
        Assert.Equal(3200, entityIds.Distinct().Count());
        Assert.All(entityIds, entityId => Assert.StartsWith("O-", entityId));
    }

    private static AuditEntity CreateModifiedEntity(string originalValue, string newValue)
    {
        var entity = new AuditEntity("Order", "O1", OperationType.Modify);
        entity.AddProperties([new AuditProperty("Name", "System.String", originalValue, newValue)]);
        return entity;
    }
}
