namespace TpaoWebApp.Model
{
    public class WellAndFormation
    {
        public WellTop? Well { get; set; }
        public List<Formation>? Formation { get; set; }
        public Dictionary<string, string> Color { get; set; }
    }
}
