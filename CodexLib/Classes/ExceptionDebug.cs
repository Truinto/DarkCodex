using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ExceptionDebug : Exception
    {
        public ExceptionDebug(string message) : base(message)
        {
            this.SetStackTrace();
        }
    }
}
