using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example_04_DI_IOCcontainer
{
    public class GetCreatedTimeImplementation : ITransientGetCreatedTime,
        IScopedGetCreatedTime,
        ISingletonGetCreatedTime
    {
        private readonly DateTime createdOn;

        public GetCreatedTimeImplementation()
        {
            createdOn = DateTime.Now;
        }

        public DateTime GetCreatedTime()
        {
            return createdOn.AddHours(-6);
        }
    }
}
