using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;

namespace WebApi2Book.Web.Common.Routing
{
    public class ApiVersionConstraint : IHttpRouteConstraint
    {
        /*alowedVersion dobi od ApiVersion1RoutePrefixAttribute, kadar se gre za contorller V1
         lahko pa constraint definiramo tudi kot RoutePrefix na samem kontorlerju, kot smo za verzijo V2*/
        public ApiVersionConstraint(string allowedVersion)
        {
            AllowedVersion = allowedVersion.ToLowerInvariant();
        }
        public string AllowedVersion { get; private set; }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName,
            IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            object value;
            if (values.TryGetValue(parameterName, out value) && value != null)
            {
                return AllowedVersion.Equals(value.ToString().ToLowerInvariant());
            }
            return false;
        }
    }
}
