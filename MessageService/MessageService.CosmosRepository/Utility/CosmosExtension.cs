using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MessageService.InfraStructure.Helpers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;

namespace MessageService.CosmosRepository.Utility
{
    public static class CosmosExtension
    {
        public static async Task<List<T>> ToListAsync<T>(this IDocumentQuery<T> queryable)
        {
            var list = new List<T>();
            while (queryable.HasMoreResults)
            {   //Note that ExecuteNextAsync can return many records in each call
                var response = await queryable.ExecuteNextAsync<T>();
                list.AddRange(response);
            }
            return list;
        }

        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> query)
        {
            return await query.AsDocumentQuery().ToListAsync();
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> queryable)
        {
            var result = await queryable.AsDocumentQuery().ExecuteNextAsync<T>();
            return result.FirstOrDefault();
        }
        public static SqlParameterCollection ToSQLParameterCollection(this Dictionary<string,object> parametersDic)
        {
            if (parametersDic.IsNull())
            {
                return null;
            }
            SqlParameterCollection collection = new SqlParameterCollection();
            foreach (var item in parametersDic)
            {
                collection.Add(new SqlParameter() { Name = item.Key, Value = item.Value });
            }
            return collection;
        }

    }
}
