using System.Collections.Generic;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.Utils;

/// <summary>
/// 数据库工具类，提供数据库名称到 <see cref="Database"/> 枚举的映射。
/// </summary>
public static class DatabaseUtils
{
    /// <summary>
    /// 根据名称获取数据库类型，支持多种别名。
    /// </summary>
    /// <param name="name">数据库名称或别名</param>
    /// <returns>对应的 Database 枚举值</returns>
    /// <exception cref="KeyNotFoundException">不支持的数据库名称</exception>
    public static Database Get(string name)
    {
        return name switch
        {
            "postgre" or "postgresql" => Database.PostgreSQL,
            "highgo" => Database.HighGo,
            "mysql" => Database.MySql,
            "mssql" or "sqlserver" => Database.SQLServer,
            "oracle" => Database.Oracle,
            "mongo" or "mongodb" => Database.MongoDB,
            "redis" => Database.Redis,
            "clickhouse" => Database.ClickHouse,
            "tidb" => Database.TiDB,
            "duckdb" => Database.DuckDB,
            "rocksdb" => Database.RocksDB,
            "dolphindb" => Database.DolphinDB,
            "ravendb" => Database.RavenDB,
            "druid" => Database.Druid,
            "tdengine" => Database.TDengine,
            "leveldb" => Database.LevelDB,
            "opengauss" => Database.OpenGauss,
            "spatialite" => Database.SpatiaLite,
            "oceanbase" => Database.OceanBase,
            "cassandra" => Database.Cassandra,
            "hbase" => Database.HBase,
            "kingbase" => Database.kingbase,
            "dameng" => Database.Dameng,
            _ => throw new KeyNotFoundException($"Database '{name}' is not supported.")
        };
    }
}
