namespace TpaoWebApp.Model
{
	public class HatKoordinat
	{
		public int X { get; set; }
		public int Y { get; set; }
	}

	public class JsonDataModel
	{
		public string HatAdi { get; set; }
		public int Tolerans { get; set; }
		public List<HatKoordinat> HatKoordinatlari { get; set; }
		public List<SadelestirilmisHatKoordinat> SadelestirilmisHat { get; set; }
	}
	public class SadelestirilmisHatKoordinat
	{
		public double X { get; set; }
		public double Y { get; set; }
	}
}
