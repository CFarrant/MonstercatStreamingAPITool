using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace JSONTool.Objects
{
    public class Artist
    {
        public string name;

        public Artist(string name)
        {
            this.name = name;
        }

        public Artist() {}

        public string getName()
        {
            return this.name;
        }

        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(new
            {
                name = getName()
            });
        }
    }
}
