using System.Linq;
using MicroserviceFramework.Auditing.Model;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// <see cref="AuditOperation.AddEntities"/> 顺序收集测试：
/// 同一 scope 内按调用顺序追加每个非空 <see cref="AuditEntity"/>，同时保持 SetOperation 关联行为。
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

    /// <summary>同一次收集内出现重复值身份的不同实例时，仍按输入顺序完整追加且正常关联</summary>
    [Fact]
    public void AddEntities_DuplicateValueIdentity_PreservesEveryEntityInOrder()
    {
        var operation = CreateOperation();
        var first = new AuditEntity("Order", "O1", OperationType.Modify);
        var second = new AuditEntity("Order", "O1", OperationType.Modify);

        operation.AddEntities([first, second]);

        var entities = operation.Entities.ToArray();
        Assert.Equal(2, entities.Length);
        Assert.Same(first, entities[0]);
        Assert.Same(second, entities[1]);
        Assert.All(entities, entity => Assert.Same(operation, entity.Operation));
    }

    /// <summary>同一批次中的每个实体实例都应被保留</summary>
    [Fact]
    public void AddEntities_RepeatedCollectionSameState_PreservesEveryEntity()
    {
        var operation = CreateOperation();
        var entities = new[]
        {
            new AuditEntity("Order", "O1", OperationType.Modify),
            new AuditEntity("Order", "O1", OperationType.Modify),
            new AuditEntity("Order", "O1", OperationType.Modify)
        };

        operation.AddEntities(entities);

        Assert.Equal(entities, operation.Entities);
        Assert.All(operation.Entities, entity => Assert.Same(operation, entity.Operation));
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
    public void AddEntities_RepeatedCalls_AppendAllEntitiesInOrder()
    {
        var operation = CreateOperation();
        var firstBatch = CreateModifiedEntity("before-1", "after-1");
        var secondBatch = CreateModifiedEntity("before-1", "after-1");

        operation.AddEntities([firstBatch]);
        operation.AddEntities([secondBatch]);

        var entities = operation.Entities.ToArray();
        Assert.Equal(2, entities.Length);
        Assert.Same(firstBatch, entities[0]);
        Assert.Same(secondBatch, entities[1]);
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

    private static AuditEntity CreateModifiedEntity(string originalValue, string newValue)
    {
        var entity = new AuditEntity("Order", "O1", OperationType.Modify);
        entity.AddProperties([new AuditProperty("Name", "System.String", originalValue, newValue)]);
        return entity;
    }
}
