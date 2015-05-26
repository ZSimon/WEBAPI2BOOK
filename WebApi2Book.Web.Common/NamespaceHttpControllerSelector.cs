using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace WebApi2Book.Web.Common
{
    public class NamespaceHttpControllerSelector:IHttpControllerSelector
    {
        private readonly HttpConfiguration _configuration;
        private readonly Lazy<Dictionary<string, HttpControllerDescriptor>> _controllers;

        public NamespaceHttpControllerSelector(HttpConfiguration config)
        {
            _configuration = config;
            _controllers = new Lazy<Dictionary<string,
                HttpControllerDescriptor>>(InitializeControllerDictionary);
        }

        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            //izvede se za constraini:
            var routeData = request.GetRouteData();
            if (routeData == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var controllerName = GetControllerName(routeData);
            if (controllerName == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var namespaceName = GetVersion(routeData);
            if (namespaceName == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            /*controllerKey-kar je direktorijev pod controllers + ime metode
             prevrim, če obstaja v naši zbriki, ki smo jo definirali v InitializeControllerDictionary*/
            var controllerKey = String.Format(CultureInfo.InvariantCulture, "{0}.{1}",
                namespaceName, controllerName);
            HttpControllerDescriptor controllerDescriptor;
            if (_controllers.Value.TryGetValue(controllerKey, out controllerDescriptor))
            {
                return controllerDescriptor;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllers.Value;
        }

        private Dictionary<string, HttpControllerDescriptor> InitializeControllerDictionary()
        {
            //tale dictionary se napolni čisto na začetku, pred vsemi filtri)
            var dictionary = new Dictionary<string,
                HttpControllerDescriptor>(StringComparer.OrdinalIgnoreCase);
            var assembliesResolver = _configuration.Services.GetAssembliesResolver();
            var controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();
            var controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);
            foreach (var controllerType in controllerTypes)
            {
                //primer:controllerType.Namespace = "WebApi2Book.Web.Api.Controllers.V1", controllerType.Name=TasksController
                var segments = controllerType.Namespace.Split(Type.Delimiter);
                /*controlleName odstrani iz naših kontrolerjev controller besedo, recimo: TasksController je controllerName enak Tasks*/
                var controllerName =
                    controllerType.Name.Remove(controllerType.Name.Length -
                                               DefaultHttpControllerSelector.ControllerSuffix.Length);
                /*sestavimo kontroler key, ki je verzija + ime kontrolerja(tako dobimo naš namespace)*/
                var controllerKey = String.Format(CultureInfo.InvariantCulture, "{0}.{1}",
                    segments[segments.Length - 1], controllerName);
                if (!dictionary.Keys.Contains(controllerKey))
                {
                    dictionary[controllerKey] = new HttpControllerDescriptor(_configuration,
                        controllerType.Name,
                        controllerType);
                }
            }
            return dictionary;
        }

        private T GetRouteVariable<T>(IHttpRouteData routeData, string name)
        {
            object result;
            if (routeData.Values.TryGetValue(name, out result))
            {
                return (T)result;
            }
            return default(T);
        }

        private string GetControllerName(IHttpRouteData routeData)
        {
            var subroute = routeData.GetSubRoutes().FirstOrDefault();
            if (subroute == null) return null;
            var dataTokenValue = subroute.Route.DataTokens.First().Value;
            if (dataTokenValue == null) return null;
            var controllerName =
                ((HttpActionDescriptor[])dataTokenValue).First()
                    .ControllerDescriptor.ControllerName.Replace("Controller", string.Empty);
            return controllerName;
        }

        private string GetVersion(IHttpRouteData routeData)
        {
            var subRouteData = routeData.GetSubRoutes().FirstOrDefault();
            if (subRouteData == null) return null;
            return GetRouteVariable<string>(subRouteData, "apiVersion");
        }

    }
}
