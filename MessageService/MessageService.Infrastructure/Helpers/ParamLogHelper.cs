using System;
using System.Linq.Expressions;
using System.Text;

namespace MessageService.InfraStructure.Helpers
{
    public static class ParamLogHelper
    {
        public static string Pipe = " | ";
        public static string Colon = " : ";
        public static string GetLogString(params Expression<Func<object>>[] providedParameters)
        {
            var paramaterLog = new StringBuilder();
            try
            {

                foreach (var aExpression in providedParameters)
                {
                    var bodyType = aExpression.Body;
                    if (bodyType is MemberExpression memberExpression)
                    {
                        paramaterLog.Append(memberExpression.Member.ToString() + " : {" + memberExpression.Member.Name + "} " + Pipe);
                    }
                    else if (bodyType is UnaryExpression unaryexpression)
                    {

                            var unaryMemberExpression = (MemberExpression)(unaryexpression.Operand);
                            paramaterLog.Append(unaryMemberExpression.Member.ToString() + " : {" + unaryMemberExpression.Member.Name + "} " + Pipe);
                        
                    }
                }

                return paramaterLog.ToString();
            }
            catch
            {
                return paramaterLog.ToString();
            }
        }

    }
}
