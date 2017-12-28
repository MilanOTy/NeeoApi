using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Neeo.Interfaces
{
    interface ITokenSearch
    {
        string          Name            { get; }
        double          Score           { get; set; }
        double          MaxScore        { get; set; }
        HashSet<string> DataEntryTokens { get; set; }
        string          GetKeyItem      (string name);
    }
}
