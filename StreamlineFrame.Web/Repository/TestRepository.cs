using StreamlineFrame.Web.Common;
using StreamlineFrame.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StreamlineFrame.Web.Repository
{
    public class TestRepository : BaseRepository<Test>
    {
        public TestRepository() : base("default")
        {
        }
    }
}