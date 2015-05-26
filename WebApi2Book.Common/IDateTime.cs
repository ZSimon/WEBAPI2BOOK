using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi2Book.Common
{
    public interface IDateTime
    {
        DateTime UtcNow { get; }
    }
}
