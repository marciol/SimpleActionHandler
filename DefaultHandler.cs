using System;
using System.Web;
using System.Web.Routing;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SimpleActionHandler
{
    public class DefaultHandler : IHttpHandler
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

            var type = controllerTypes.First(t => t.Name == controllerName); 
            var controller = System.Activator.CreateInstance(type) as IController;

            controller.Request = context.Request;
            controller.Response = context.Response;

            foreach (var segment in routeData.Values)
                controller.Params.Add(segment.Key, segment.Value);

            foreach (var pkey in context.Request.QueryString.AllKeys)
                controller.Params.Add(pkey, context.Request.QueryString[pkey]);

            var candidates =
                type.GetMethods()
                    .Where(method => method.Name == actionName);

            var action =
                candidates.FirstOrDefault(method => method.GetParameters().Any(parameter => controller.Params.Keys.Contains(parameter.Name)))
                          .GetOrElse(candidates.FirstOrDefault(method => method.GetParameters().Count() == 0));
                    
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
                else
                {
                    parameterValue = value;
                }

                parameters.Add(parameterValue);
            }

            var actionResult = action.Invoke(controller, parameters.ToArray()) as ActionResult;
            buildResponseFrom(actionResult, controller.Response);
        }

        private void buildResponseFrom(ActionResult actionResult, HttpResponse response) 
        {
            actionResult.ExecuteResult(response);
        }

    }
}
