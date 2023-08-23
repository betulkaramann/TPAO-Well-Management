using System.ComponentModel.DataAnnotations;

namespace TpaoProject1.Model
{
    public class Formation
    {
        public Formation(int _WellId)
        {
            WellId = _WellId;
        }
        public Formation()
        {
        }
        [Key]
        public int Id { get; set; }
        public int WellId { get; set; }
        public string? FormationType { get; set; }
        public int? FormationMeter { get; set; }


    }
}
