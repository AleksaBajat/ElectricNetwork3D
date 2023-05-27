namespace ElectricNetwork3D.Models
{
    public class Substation:IElectricItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Point Point { get; set; }
        
        public Substation(string id, string name, Point point)
        {
            Id = id;
            Name = name;
            Point = point;
        }
    }
}