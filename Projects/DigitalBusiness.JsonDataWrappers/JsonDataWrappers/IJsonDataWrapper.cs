using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    public interface IJsonDataWrapper :IJsonData
    {
        new JsonData Json { get; init; }
    }
}
