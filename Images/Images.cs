namespace Topo.Images
{
    public interface IImages
    {
        public string GetLogoForSection(string section);
    }
    public class Images : IImages
    {
        public string GetLogoForSection(string section)
        {
            var logoName = "";
            switch (section)
            {
                case "joey":
                    logoName = "Joey Scouts Full Col Vertical.jpg";
                    break;
                case "cub":
                    logoName = "Cub Scouts Full Col Vertical.png";
                    break;
                case "scout":
                    logoName = "Scouts Full Col Vertical.jpg";
                    break;
                case "venturer":
                    logoName = "Venturer Scouts Full Col Vertical.jpg";
                    break;
                case "rover":
                    logoName = "Rover Scouts Full Col Vertical.jpg";
                    break;
            }
            return logoName;
        }
    }
}
