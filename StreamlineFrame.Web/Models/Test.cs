using StreamlineFrame.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StreamlineFrame.Web.Models
{
    [DBName("ORT_Test")]
    public class Test
    {
        [DBKey]
        public int MyProperty { get; set; }

        [DBName("[Name]")]
        public string Name { get; set; }

        public int Age { get; set; }

        public int? Pyh { get; set; }
    }
}