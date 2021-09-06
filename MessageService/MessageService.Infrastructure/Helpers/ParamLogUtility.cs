using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MessageService.InfraStructure.Helpers
{
    public  class ParamLogUtility
    {
        private static string _pipe = " | ";
        private StringBuilder _paramaterLog = new StringBuilder();

        //private readonly string _methodName;
        //private readonly Dictionary<string, Type> _methodParamaters;
        //private readonly List<Tuple<string, Type, object>> _providedParametars;

        public ParamLogUtility(params Expression<Func<object>>[] providedParameters)
        {
            try
            {
                // var currentMethod = new StackTrace().GetFrame(1).GetMethod();

                /*Set class and current method info*/
                //if (currentMethod.DeclaringType != null)
                //    _methodName = String.Format("Class = {0}, Method = {1}",
                //        currentMethod.DeclaringType.FullName, currentMethod.Name);

                /*Get current methods parameters*/
                //_methodParamaters = new Dictionary<string, Type>();
                //(from aParamater in currentMethod.GetParameters()
                // select new { Name = aParamater.Name, DataType = aParamater.ParameterType })
                //    .ToList()
                //    .ForEach(obj => _methodParamaters.Add(obj.Name, obj.DataType));

                //_providedParametars = new List<Tuple<string, Type, object>>();

                foreach (var aExpression in providedParameters)
                {
                    var bodyType = aExpression.Body;
                    if (bodyType is MemberExpression)
                    {
                        var memPara = bodyType.ToString().Split('.').Last();

                        _paramaterLog.Append(memPara + " : {" + memPara + "} " + _pipe);

                        // AddProvidedParamaterDetail((MemberExpression)aExpression.Body);
                    }
                    else if (bodyType is UnaryExpression)
                    {
                        var unaryExpression = (System.Linq.Expressions.UnaryExpression)(bodyType);
                       var   memberExpression = (System.Linq.Expressions.MemberExpression)(unaryExpression.Operand);

                        var substrings = bodyType.ToString().Split('.');
                        var uu = substrings.Last();

                        // var para = bodyType.ToString().Split('.').Last().Replace(")", "");

                        var para = uu.Replace(")", "");
                        _paramaterLog.Append(para + " : {" + para + "} " + _pipe);



                        //UnaryExpression unaryExpression = (UnaryExpression)aExpression.Body;
                        //AddProvidedParamaterDetail((MemberExpression)unaryExpression.Operand);
                    }

                }

                /*Process log for all method parameters*/
                //  ProcessLog();
            }
            catch 
            {
                // throw new Exception("Error in paramater log processing.", exception);
            }
        }
        //private void ProcessLog()
        //{
        //    try
        //    {
        //        foreach (var aMethodParamater in _methodParamaters)
        //        {
        //            var aParameter =
        //                _providedParametars.Single(obj => obj.Item1.Equals(aMethodParamater.Key) &&
        //                                                  obj.Item2 == aMethodParamater.Value);
        //            _paramaterLog += $@" ""{aParameter.Item1}"":{JsonConvert.SerializeObject(aParameter.Item3)},";
        //        }
        //        _paramaterLog = (_paramaterLog != null) ? _paramaterLog.Trim(' ', ',') : string.Empty;
        //    }
        //    catch (Exception exception)
        //    {
        //        throw new Exception("MathodParamater is not found in providedParameters.");
        //    }
        //}

        //private void AddProvidedParamaterDetail(MemberExpression memberExpression)
        //{
        //    ConstantExpression constantExpression = (ConstantExpression)memberExpression.Expression;
        //    var name = memberExpression.Member.Name;
        //    var value = ((FieldInfo)memberExpression.Member).GetValue(constantExpression.Value);
        //    var type = value.GetType();
        //    _providedParametars.Add(new Tuple<string, Type, object>(name, type, value));
        //}

        public string GetLog()
        {
            return _paramaterLog.ToString();
        }


    }
}
