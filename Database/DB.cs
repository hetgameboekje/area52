using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ReservationSystem.Database
{
    public class DB
    {
        private readonly string _connectionString;

        public DB(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Executes a SQL query and returns the results as a list of dictionaries
        /// </summary>
        public List<Dictionary<string, object>> Query(string sql, Dictionary<string, object>? parameters = null)
        {
            var results = new List<Dictionary<string, object>>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Executes a SQL command (INSERT, UPDATE, DELETE) and returns the number of affected rows
        /// </summary>
        public int Execute(string sql, Dictionary<string, object>? parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes a SQL command and returns the identity of the inserted row
        /// </summary>
        public int ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    var result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }
    }
}
