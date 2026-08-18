using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class NotNullExceptions : Exception
    {
        public NotNullExceptions(string message) : base(message)
        {
        }

        public NotNullExceptions(string message, Exception innerException) : base(message, innerException)
        {
        }

        public NotNullExceptions() : base("Can not be Null")
        {
        }
    }
}
