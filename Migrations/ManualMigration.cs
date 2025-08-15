using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace UserService.Migrations
{
    public static class ManualMigration
    {
        public static void Run(string connectionString)
        {
            var sqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations/001_create_users_table.sql");
            Console.WriteLine($"[ManualMigration] Looking for migration file at: {sqlPath}");
            if (!File.Exists(sqlPath))
            {
                Console.WriteLine("[ManualMigration] Migration file not found.");
                return;
            }
            var sql = File.ReadAllText(sqlPath);
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            Console.WriteLine("[ManualMigration] Running schema migration...");
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Insert initial data if table is empty
            using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\";", conn))
            {
                var count = (long)checkCmd.ExecuteScalar();
                Console.WriteLine($"[ManualMigration] Users table record count: {count}");
                if (count == 0)
                {
                    var insertSql =
                        "INSERT INTO \"Users\" (\"Name\", \"Email\") VALUES " +
                        "('Alice', 'alice@example.com')," +
                        "('Bob', 'bob@example.com')," +
                        "('Charlie', 'charlie@example.com');";
                    using var insertCmd = new NpgsqlCommand(insertSql, conn);
                    insertCmd.ExecuteNonQuery();
                    Console.WriteLine("[ManualMigration] Inserted initial user records.");
                }
            }
        }
    }
}
