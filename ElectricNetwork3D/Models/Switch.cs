namespace ElectricNetwork3D.Models
{
    public class Switch:IElectricItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public Point Point { get; set; }
        
        public Switch(string id, string name, Point point, string status)
        {
            Id = id;
            Name = name;
            Point = point;
            Status = status;
        }
    }
}