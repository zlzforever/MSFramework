# MSFramework.Auditing.Loki（已废弃）

Loki 不便创建索引，本包已废弃，仅保留供存量消费者过渡使用（所有公共 API 均已标注 `[Obsolete]`）。

## 替代方案

审计存储请改用：

- **关系数据库存储**：`MSFramework.Ef` 的 `EfAuditingStore<TDbContext>`（推荐 PostgreSQL）
- **自定义 `IAuditingStore` 实现**：接入任意存储（含直接使用 Serilog 写 Loki 的自维护实现）
