
namespace ElectricNetwork3D.Models
{
    public interface IElectricItem
    {
        string Id { get; set; }
        
        string Name { get; set; }
        
        Point Point { get; set; }
    }
}