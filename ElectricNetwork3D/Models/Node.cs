namespace ElectricNetwork3D.Models
{
    public class Node:IElectricItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Point Point { get; set; }
        
        public Node(string id, string name, Point point)
        {
            Id = id;
            Name = name;
            Point = point;
        }
    }
}