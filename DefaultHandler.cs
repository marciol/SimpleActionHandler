using System;
using System.Web;
using System.Web.Routing;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Web.SessionState;
using System.Text.RegularExpressions;

namespace SimpleActionHandler
{
    public class DefaultHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {

            var routeData = context.Items["RouteData"] as RouteData;
            var controllerName = routeData.DataTokens["controller"].ToString();
            var actionName = routeData.DataTokens["action"].ToString();

            var controllerTypes = AppDomain.CurrentDomain
                                 .GetAssemblies()
                                 .SelectMany(t => t.GetTypes())
                                 .Where(t => t.IsSubclassOf(typeof(HandlerController)));

            var type = controllerTypes.First(t => t.FullName.Contains(controllerName)); 
            var controller = System.Activator.CreateInstance(type) as IController;

            controller.Request = context.Request;
            controller.Response = context.Response;

            foreach (var segment in routeData.Values)
                controller.Params.Add(segment.Key, segment.Value);

            foreach (var key in context.Request.QueryString.AllKeys)
                controller.Params.Add(key, context.Request.QueryString[key]);

            foreach (var key in context.Request.Form.AllKeys)
                controller.Params.Add(key, controller.Request.Form[key]);
            
            var candidates =
                type.GetMethods()
                    .Where(method => method.Name == actionName);

            var action =
                candidates.FirstOrDefault(
                    method => 
                        method.GetParameters().Any(
                        parameter => 
                            controller.Params.Keys.Contains(parameter.Name)));

            action = action.GetOrElse(candidates.FirstOrDefault(method => method.GetParameters().Count() == 0));

            var filters =
                type.GetMethods()
                    .Where(
                        methods => 
                            methods.GetCustomAttributes(typeof(BeforeAttribute), true)
                                   .Cast<BeforeAttribute>()
                                   .Any(attr => attr.IsIncluded(action) && !attr.IsExcepted(action)))
                                   .ToList();
                    
            var parameters = new List<object>();
            
            foreach (var parameter in action.GetParameters())
            {
                object value = null;
                controller.Params.TryGetValue(parameter.Name, out value);
                object parameterValue = null;
                var parameterType = parameter.ParameterType;

                if (value == null)
                {
                    var a = parameterType.GetConstructor(Type.EmptyTypes);
                    if (parameterType.IsArray)
                        parameterValue = Activator.CreateInstance(parameterType, 0);
                    else
                        parameterValue = Activator.CreateInstance(parameterType);
                }
                else if (parameterType.BaseType.Name == "Array")
                {
                    var elementType = parameterType.GetElementType();

                    var collectedValues = value.ToString().Split(',').Select(p => p.Trim());

                    var convertedValues = collectedValues.Select(s => Convert.ChangeType(s, elementType));

                    var castedValues =
                        typeof(Enumerable).GetMethod("Cast", BindingFlags.Static | BindingFlags.Public)
                                          .MakeGenericMethod(elementType)
                                          .Invoke(null, new[] { convertedValues });

                    var values =
                        typeof(Enumerable).GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public)
                                          .MakeGenericMethod(elementType)
                                          .Invoke(null, new[] { castedValues });

                    parameterValue = values;
                }
                else if (parameterType.Name == "Int32")
                {
                    var strValue = (string)value;
                    parameterValue = int.Parse(strValue);
                }
                else
                {
                    parameterValue = value;
                }

                parameters.Add(parameterValue);
            }

            var actionResult = new ActionResult();

            try
            {
                filters.ForEach(
                    filter =>
                        filter.Invoke(controller, new object[] { }));

                actionResult = action.Invoke(controller, parameters.ToArray()) as ActionResult;
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is NotAuthorizated)
                {
                    actionResult =
                        new ActionResult
                        {
                            StatusCode = 302,
                            Headers = new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("Location", ((NotAuthorizated)e.InnerException).Path)
                            }
                        };
                }
                else
                {
                    throw e;
                }

            }

            buildResponseFrom(actionResult, controller.Response);
        }

        private void buildResponseFrom(ActionResult actionResult, HttpResponse response) 
        {
            actionResult.ExecuteResult(response);
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeAttribute : System.Attribute
    {
        string[] includedNames = new string[] { }, 
                 exceptedNames = new string[] { };

        Regex extractParameters = new Regex(@"\(([\w,]*)\)");

        public string Include {
            get
            {
                return string.Join(";", includedNames);
            }

            set
            {
                includedNames = Split(value);
            }
        }

        public string Except { 
            get
            {
                return string.Join(";", exceptedNames);
            }

            set
            {
                exceptedNames = Split(value);
            }
        }

        string[] Split(string coll)
        {
            return coll.Split(';').Select(s => s.Trim()).ToArray();
        }

        public bool IsIncluded(MethodInfo method)
        {
            if (Include == "*") return true;

            return Verify(method, methodName => includedNames.Contains(methodName));
        }

        public bool IsExcepted(MethodInfo method)
        {
            if (Except == "*") return true;

            return Verify(method, methodName => exceptedNames.Contains(methodName));
        }

        private bool Verify(MethodInfo method, Func<string, bool> predicate)
        {
            var parameters = 
                string.Join(
                    ",",
                    method.GetParameters().Select(t => t.ParameterType.ToString()).ToArray());

            var methodName = string.Format("{0}({1})", method.Name, parameters);

            var result = predicate.Invoke(methodName);

            return result;
        }
    }

    public class NotAuthorizated : Exception
    {
        public string Path { get; set; }

        public NotAuthorizated(string path)
        {
            this.Path = path;
        }
    }
}
