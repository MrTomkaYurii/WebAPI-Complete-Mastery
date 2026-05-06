using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericHostBoilerplate.App.Services.Contracts
{
    internal interface IExampleService
    {
        Task<string> DoWork(CancellationToken cancellationToken);
    }
}
