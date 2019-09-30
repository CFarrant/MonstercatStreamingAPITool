using System.Web.Script.Serialization;

namespace JSONTool.Objects
{
    public class Track
    {
        private string id;
        private Album album;
        private string name;
        private Artist artist;
        private string genreprimary;
        private string genresecondary;
        private string songURL;
        private int tracknumber;

        public Track(string id, string name, Artist artist, string genreprimary, string genresecondary, string songURL, Album a, int tracknumber)
        {
            this.id = id;
            this.name = name;
            this.genreprimary = genreprimary;
            this.genresecondary = genresecondary;
            this.artist = artist;
            this.songURL = songURL;
            this.tracknumber = tracknumber;
            album = a;
        }

        public string getSongURL()
        {
            return this.songURL;
        }

        public string getId()
        {
            return this.id;
        }

        public string getName()
        {
            return this.name;
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

        public Album getAlbum()
        {
            return this.album;
        }

        public int getTrackNumber()
        {
            return this.tracknumber;
        }

        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(new
            {
                id = getId(),
                tracknumber = getTrackNumber(),
                title = getName(),
                genreprimary = getPrimaryGenre(),
                genresecondary = getSecondaryyGenre(),
                songURL = getSongURL(),
                artist = new Artist()
                {
                    name = artist.getName()
                },
                album = new Album()
                {
                    id = album.getId()
                }
            });
        }
    }
}