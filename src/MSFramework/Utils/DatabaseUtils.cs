using System.Collections.Generic;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.Utils;

/// <summary>
///
/// </summary>
public static class DatabaseUtils
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
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
