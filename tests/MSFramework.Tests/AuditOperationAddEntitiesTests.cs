using System;
using System.Collections.Generic;
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
    [Fact]
    public void Entities_ExposesICollectionPublicContract()
    {
        var property = typeof(AuditOperation).GetProperty(nameof(AuditOperation.Entities));

        Assert.NotNull(property);
        Assert.Equal(typeof(ICollection<AuditEntity>), property.PropertyType);
    }

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

        var collected = operation.Entities.ToArray();
        Assert.Equal(3, collected.Length);
        Assert.Same(entities[0], collected[0]);
        Assert.Same(entities[1], collected[1]);
        Assert.Same(entities[2], collected[2]);
        Assert.All(collected, entity => Assert.Same(operation, entity.Operation));
    }

    /// <summary>
    /// 同一实体不同操作类型（如先 Add 后 Modify）属于不同变更状态，必须都收集
    /// </summary>
    [Fact]
    public void AddEntities_DifferentOperationTypes_BothCollected()
    {
        var operation = CreateOperation();
        var first = new AuditEntity("Order", "O1", OperationType.Add);
        var second = new AuditEntity("Order", "O1", OperationType.Modify);

        operation.AddEntities([first, second]);

        var collected = operation.Entities.ToArray();
        Assert.Equal(2, collected.Length);
        Assert.Same(first, collected[0]);
        Assert.Same(second, collected[1]);
        Assert.Equal(OperationType.Add, collected[0].OperationType);
        Assert.Equal(OperationType.Modify, collected[1].OperationType);
        Assert.All(collected, entity => Assert.Same(operation, entity.Operation));
    }

    [Fact]
    public void AddEntities_SameOperationTypeWithDifferentPropertySnapshots_BothCollected()
    {
        var operation = CreateOperation();
        var first = CreateModifiedEntity("before-1", "after-1");
        var second = CreateModifiedEntity("before-2", "after-2");

        operation.AddEntities([first, second]);

        var collected = operation.Entities.ToArray();
        Assert.Equal(2, collected.Length);
        Assert.Same(first, collected[0]);
        Assert.Same(second, collected[1]);
        Assert.Equal("after-1", collected[0].Properties.Single().NewValue);
        Assert.Equal("after-2", collected[1].Properties.Single().NewValue);
        Assert.All(collected, entity => Assert.Same(operation, entity.Operation));
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
        Assert.All(entities, entity => Assert.Same(operation, entity.Operation));
    }

    /// <summary>
    /// 同一操作类型不同实体标识属于不同实体，必须都收集
    /// </summary>
    [Fact]
    public void AddEntities_DifferentEntityIds_BothCollected()
    {
        var operation = CreateOperation();
        var first = new AuditEntity("Order", "O1", OperationType.Modify);
        var second = new AuditEntity("Order", "O2", OperationType.Modify);

        operation.AddEntities([first, second]);

        var collected = operation.Entities.ToArray();
        Assert.Equal(2, collected.Length);
        Assert.Same(first, collected[0]);
        Assert.Same(second, collected[1]);
        Assert.All(collected, entity => Assert.Same(operation, entity.Operation));
    }

    /// <summary>
    /// 独立空集合必须被安全跳过
    /// </summary>
    [Fact]
    public void AddEntities_EmptyCollection_DoesNotAddEntities()
    {
        var operation = CreateOperation();

        operation.AddEntities(Array.Empty<AuditEntity>());
        Assert.Empty(operation.Entities);
    }

    /// <summary>
    /// null 集合必须被安全跳过
    /// </summary>
    [Fact]
    public void AddEntities_NullCollection_DoesNotAddEntities()
    {
        var operation = CreateOperation();

        operation.AddEntities(null);

        Assert.Empty(operation.Entities);
    }

    /// <summary>
    /// 两个有效实体之间的 null 元素必须被跳过，并保持有效实体顺序与操作关联
    /// </summary>
    [Fact]
    public void AddEntities_NullElementBetweenEntities_PreservesOrderAndOperation()
    {
        var operation = CreateOperation();
        var first = new AuditEntity("Order", "O1", OperationType.Add);
        var second = new AuditEntity("Order", "O2", OperationType.Modify);

        operation.AddEntities([first, null, second]);

        var collected = operation.Entities.ToArray();
        Assert.Equal(2, collected.Length);
        Assert.Same(first, collected[0]);
        Assert.Same(second, collected[1]);
        Assert.All(collected, entity => Assert.Same(operation, entity.Operation));
    }

    private static AuditEntity CreateModifiedEntity(string originalValue, string newValue)
    {
        var entity = new AuditEntity("Order", "O1", OperationType.Modify);
        entity.AddProperties([new AuditProperty("Name", "System.String", originalValue, newValue)]);
        return entity;
    }
}
