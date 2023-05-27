using System.Collections.Generic;

namespace ElectricNetwork3D.Models
{
    public class Wire
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsUnderground { get; set; }

        public double R { get; set; }

        public string LineType { get; set; }

        public int ThermalConstantHeat { get; set; }

        public int FirstEnd { get; set; }

        public int SecondEnd { get; set; }
        
        public string ConductorMaterial { get; set; }
        
        public List<Point> Points { get; set; }

        public Wire(string id, string name, bool isUnderground, double r, string lineType, int thermalConstantHeat, int firstEnd, int secondEnd, List<Point> points,string conductorMaterial)
        {
            Id = id;
            Name = name;
            IsUnderground = isUnderground;
            R = r;
            LineType = lineType;
            ThermalConstantHeat = thermalConstantHeat;
            FirstEnd = firstEnd;
            SecondEnd = secondEnd;
            Points = points;
            ConductorMaterial = conductorMaterial;
        }
    }
}