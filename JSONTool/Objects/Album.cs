using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace JSONTool.Objects
{
    public class Album
    {
        public string id;
        private string name;
        private string type;
        private string releaseCode;
        private string coverURL;
        private Artist artist;
        private string genreprimary;
        private string genresecondary;

        public Album(string id, string name, Artist artist, string type, string releaseCode, 
            string coverURL, string genreprimary, string genresecondary)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.releaseCode = releaseCode;
            this.coverURL = coverURL;
            this.genreprimary = genreprimary;
            this.genresecondary = genresecondary;
            this.artist = artist;
        }

        public Album() {}

        public string getId()
        {
            return this.id;
        }

        public string getName()
        {
            return this.name;
        }

        public string getType()
        {
            return this.type;
        }

        public string getReleaseCode()
        {
            return this.releaseCode;
        }

        public string getCoverURL()
        {
            return this.coverURL;
        }

        public string getPrimaryGenre()
        {
            return this.genreprimary;
        }

        public string getSecondaryyGenre()
        {
            return this.genresecondary;
        }

        public Artist getArtist()
        {
            return this.artist;
        }

        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(new
            {
                id = getId(),
                name = getName(),
                type = getType(),
                releaseCode = getReleaseCode(),
                genreprimary = getPrimaryGenre(),
                genresecondary = getSecondaryyGenre(),
                coverURL = getCoverURL(),
                artist = new Artist()
                {
                    name = artist.getName()
                }
            });
        }
    }
}
