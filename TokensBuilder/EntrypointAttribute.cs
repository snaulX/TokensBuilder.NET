using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokensBuilder
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EntrypointAttribute : Attribute
    {
        public EntrypointAttribute() { }
    }
}
