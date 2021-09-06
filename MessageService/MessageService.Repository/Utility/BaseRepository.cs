using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MessageService.InfraStructure.Helpers;

namespace MessageService.Repository.Utility
{
    public abstract class BaseRepository
    {
        /// <summary>
        /// Note: Default Connection string is Customer Support Database 
        /// </summary>
        public virtual string DbConnectionString { get; protected set; } = AppSettings.GetValue("ConnectionStrings:MessageServiceConnection");

        public const int CommandTimeOut = 120;
        protected T QueryFirstOrDefault<T>(string sql, object parameters = null)
        {
            using (var connection = CreateConnection())
            {
                return connection.QueryFirstOrDefault<T>(sql, parameters);
            }
        }

        /// <summary>
        /// QueryFirstOrDefaultAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null)
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, commandTimeout : CommandTimeOut).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected List<T> Query<T>(string sql, object parameters = null)
        {
            using (var connection = CreateConnection())
            {
                return connection.Query<T>(sql, parameters,commandTimeout: CommandTimeOut).ToList();
            }
        }

        /// <summary>
        /// QueryAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null)
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<T>(sql, parameters,commandTimeout: CommandTimeOut).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// It Will Return IEnumerable of Data with pagination 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<(IEnumerable<T>,int)> QueryAsPagingAsync<T>(string sql, object parameters = null)
        {
            using (var connection = CreateConnection())
            {
                var hashSet = new HashSet<int>();
                Func<T, int, T> map = (result, count) =>
                {
                    hashSet.Add(count);
                    return result;
                };
                var items = await connection.QueryAsync(sql, map, parameters, splitOn: "TotalCount",commandTimeout: CommandTimeOut).ConfigureAwait(false);

                return (items,hashSet.Sum());

            }
        }

        /// <summary>
        /// It Will Return List of Data with pagination
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<(IList<T>, int)> QueryAsPagingListAsync<T>(string sql, object parameters = null)
        {
            using (var connection = CreateConnection())
            {
                var hashSet = new HashSet<int>();
                Func<T, int, T> map = (result, count) =>
                {
                    hashSet.Add(count);
                    return result;
                };
                var items = await connection.QueryAsync(sql, map, parameters, splitOn: "TotalCount",commandTimeout:CommandTimeOut).ConfigureAwait(false);

                return (items.ToList(), hashSet.Sum());

            }
        }

        /// <summary>
        /// It Will Return List of Data with pagination
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<(IList<T>, int)> QueryProcAsPagingListAsync<T>(string procName, DynamicParameters parameters = null)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var hashSet = new HashSet<int>();
                    Func<T, int, T> map = (result, count) =>
                    {
                        hashSet.Add(count);
                        return result;
                    };
                    var items = await connection.QueryAsync(procName, map, parameters, splitOn: "TotalCount", commandType: CommandType.StoredProcedure, transaction: transaction,commandTimeout:CommandTimeOut).ConfigureAwait(false);
                    transaction.Commit();
                    return (items.ToList(), hashSet.Sum());
                }
            }
        }
        /// <summary>
        /// Procedure execute and will return values 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<T>> QueryProcAsync<T>(string procName, DynamicParameters parameters = null)
        {
            using (var connection = CreateConnection())
            {
               await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var result = await connection.QueryAsync<T>(procName, parameters, commandType: CommandType.StoredProcedure, transaction: transaction,commandTimeout: CommandTimeOut).ConfigureAwait(false);
                    transaction.Commit();
                    return result;
                }
               
            }
        }


        /// <summary>
        /// Procedure Execute and will return First or Default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<T> QueryFirstOrDefaultProcAsync<T>(string procName, DynamicParameters parameters = null)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var result = await connection.QueryFirstOrDefaultAsync<T>(procName, parameters, commandType: CommandType.StoredProcedure, transaction: transaction,commandTimeout: CommandTimeOut).ConfigureAwait(false);
                    transaction.Commit();
                    if (result == null)
                    {
                        return default(T);
                    }

                    return result;
                }
                
            }
        }
        /// <summary>
        /// Execute Procedure and will return affected rows
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<int> ExecuteProcAsync(string procName, DynamicParameters parameters = null)
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var result = await connection.ExecuteAsync(procName,param: parameters, commandType: CommandType.StoredProcedure, transaction: transaction,commandTimeout: CommandTimeOut).ConfigureAwait(false);
                    transaction.Commit();
                    return result;
                }
            }
            
        }

        protected int Execute(string sql, object parameters = null)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var result = connection.Execute(sql, parameters,commandTimeout: CommandTimeOut);
                    transaction.Commit();

                    return result;
                }
            }
        }

        protected async Task<int> ExecuteAsync(string sql, object parameters = null)
        {
            var result = 0;
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {                    
                    result = await connection.ExecuteAsync(sql, parameters, transaction: transaction,commandTimeout: CommandTimeOut).ConfigureAwait(false);
                    transaction.Commit();
                }
                connection.Close();
            }
            return result;
        }

        private SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(DbConnectionString);
            return connection;
        }
    }
}
