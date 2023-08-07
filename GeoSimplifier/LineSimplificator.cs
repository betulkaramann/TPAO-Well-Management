using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


namespace GeoSimplifier
{
   
    public class Veri
    {
        public List<Cooordinate> HatKoordinatlari { get; set; }

        public class Cooordinate
        {
            public double X { get; set; }
            public double Y { get; set; }


		}
    }

    [MemoryDiagnoser]
    public static class LineSimplificator
    {
        [Benchmark]
        public static List<(double, double)> ReadData(string jsonFile)
        {
            string jsonFilePath = jsonFile;

            string jsonData = File.ReadAllText(jsonFilePath);

            // Remove empty lines from the JSON data
            // jsonData = RemoveEmptyLines(jsonData);
            // Deserialize JSON data
            Veri veri = JsonConvert.DeserializeObject<Veri>(jsonData);
            List<(double x, double y)> list = new();
            foreach (var koordinat in veri.HatKoordinatlari)
            {
                list.Add((koordinat.X, koordinat.Y));
            }

            return list;
        }

		[Benchmark]
		public static Boolean IsFileEmpty(string jsonFile)
        {
            string jsonFilePath = jsonFile;

            string jsonData = File.ReadAllText(jsonFilePath);
            // Deserialize JSON data
            Veri veri = JsonConvert.DeserializeObject<Veri>(jsonData);
            List<(double x, double y)> list = new();
            // Check for negative values in the coordinates
            if (new FileInfo(jsonFilePath).Length != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

		[Benchmark]
		public static Boolean IsNullValue(string jsonFile)
        {
            string jsonFilePath = jsonFile;

            string jsonData = File.ReadAllText(jsonFilePath);
            // Deserialize JSON data
            Veri veri = JsonConvert.DeserializeObject<Veri>(jsonData);
            List<(double x, double y)> list = new();
            // Check for negative values in the coordinates
            if (veri.HatKoordinatlari.Any(koordinat => koordinat.X == null || koordinat.Y == null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
		
        [Benchmark]
		public static Boolean IsNegative(string jsonFile)
        {
            string jsonFilePath = jsonFile;

            string jsonData = File.ReadAllText(jsonFilePath);
            // Deserialize JSON data
            Veri veri = JsonConvert.DeserializeObject<Veri>(jsonData);
            List<(double x, double y)> list = new();
            // Check for negative values in the coordinates
            if (veri.HatKoordinatlari.Any(koordinat => koordinat.X < 0 || koordinat.Y < 0))
            {
                return false;
            }
            else
            {
                return true;
            }
        }


		[Benchmark]
		public static bool HasDuplicateCoordinates(String jsonFile)
        {
            string jsonFilePath = jsonFile;

            string jsonData = File.ReadAllText(jsonFilePath);

            // Remove empty lines from the JSON data
            // jsonData = RemoveEmptyLines(jsonData);
            // Deserialize JSON data
            Veri veri = JsonConvert.DeserializeObject<Veri>(jsonData);
            List<(double x, double y)> coordinates = new();
            foreach (var koordinat in veri.HatKoordinatlari)
            {
                coordinates.Add((koordinat.X, koordinat.Y));
            }
            bool hasDuplicates = coordinates.GroupBy(coord => coord).Any(group => group.Count() > 1);

            if (hasDuplicates)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

		

		[Benchmark]
		public static double CalculateDistanceToLine(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            double nominaotor = Math.Abs((x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1));

            double denominator = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));

            var res = nominaotor / denominator;
            return res;
        }

		[Benchmark]
		public static List<(double x, double y)> SimplifyPoints(List<(double x, double y)> list, int first_index, int last_index, double tolerance)
        {
            List<(double x, double y)> simplyfied_list = new List<(double x, double y)>();
            simplyfied_list.Add((list[first_index].x, list[first_index].y));
            double max = 0;


            int chech_index = 0;
            for (int i = first_index + 1; i < last_index; i++)
            {
                double distance = CalculateDistanceToLine(list[i].x, list[i].y, list[first_index].x, list[first_index].y, list[last_index].x, list[last_index].y);
                if (distance > max)
                {
                    max = distance;
                    chech_index = i;
                }
            }
            bool flag = false;
            if (max < tolerance)
                flag = true;
            if (!flag)
            {
                var first_half = SimplifyPoints(list, first_index, chech_index, tolerance);
                var second_half = SimplifyPoints(list, chech_index, last_index, tolerance);

                // İlk yarısı ve ikinci yarısı, ana simplyfied_list'e birleştirilir.
                simplyfied_list.AddRange(first_half.Skip(1)); // İlk elemanı eklememeliyiz çünkü zaten ana listede var.
                simplyfied_list.AddRange(second_half);
                //simplyfied_list.Add((list[list.Count() - 1].x, list[list.Count() - 1].y));
            }
            //simplyfied_list.Add((list[list.Count() - 1].x, list[list.Count() - 1].y));

            return simplyfied_list;

        }

    }
}