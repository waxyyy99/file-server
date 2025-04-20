using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public record class HttpResponse(int StatusCode, string? Content);
}
