using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    public interface IHandlerInfo
    {
        long RegistrationOrder { get; }
        public string Owner { get; }
        public string Name { get; }

        public string FullName {  get; }
    }
}
