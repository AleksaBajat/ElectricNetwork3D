using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ElectricNetwork3D.Lib;
using ElectricNetwork3D.Models;
using ElectricNetwork3D.Scenes.Polys;
using Point = ElectricNetwork3D.Models.Point;

namespace ElectricNetwork3D.Scenes;

public class MainScene
{
    public List<Node> Nodes { get; set; } = new List<Node>();
    public List<Substation> Substations { get; set; } = new List<Substation>();
    public List<Switch> Switches { get; set; } = new List<Switch>();
    public List<Wire> Wires { get; set; } = new List<Wire>();

    private Dictionary<Point, int> _verticalEntities;
    
    private Dictionary<string, GeometryModel3D> _allEntities;

    private Dictionary<string, Model3DGroup> _allWires;

    private GeometryModel3D _previousEntity1, _previousEntity2;
    
    private Material _previousEntity1Material, _previousEntity2Material;
    
    public Dictionary<GeometryModel3D, string> EntityTooltips { get; set; }

    private Model3DGroup _scene;

    private bool _activeNetwork = false;

    public MainScene(Model3DGroup scene)
    {
        _scene = scene;
        DataAccess.InitData(Nodes,Substations,Switches,Wires);
        _verticalEntities = new Dictionary<Point, int>();
        _allEntities = new Dictionary<string, GeometryModel3D>();
        _allWires = new Dictionary<string, Model3DGroup>();
        EntityTooltips = new Dictionary<GeometryModel3D, string>();
        
        
        DrawDefaultScene();
    }

    public void ClearScene()
    {
        _scene.Children.Clear();
        _allEntities.Clear();
        _verticalEntities.Clear();
        _allWires.Clear();
    }

    public void DrawScene(List<IElectricItem> substations, List<IElectricItem> switches, List<IElectricItem> nodes, List<Wire> wires)
    {
        ClearScene();
        DrawElectricItem(Brushes.Orange, substations, defaultTypeName:"Substation"); 
        DrawElectricItem(Brushes.Aqua, switches, defaultTypeName: "Switch"); 
        DrawElectricItem(Brushes.Chartreuse, nodes, defaultTypeName:"Node");
        DrawWires(wires);
    }

    public void DrawDefaultScene()
    {
        DrawScene(Substations.Cast<IElectricItem>().ToList(), 
            Switches.Cast<IElectricItem>().ToList(),
            Nodes.Cast<IElectricItem>().ToList(),
            Wires);
    }

    public void ToggleNetworkActivity()
    {
        var openSubstations = Switches.Where(x => x.Status == "Open").Select(x => x.Id).ToList();
        var wiresToRemove = Wires.Where(x => openSubstations.Contains(x.FirstEnd.ToString())).ToList();
        List<string> secondEndEntities = new List<string>();

        foreach (var wire in wiresToRemove)
        {
            if (_allWires.ContainsKey(wire.Id))
            {
                var entity = _allWires[wire.Id];
                foreach (GeometryModel3D wirePart in entity.Children)
                {
                    wirePart.Material = new DiffuseMaterial(Brushes.Transparent);
                }
                secondEndEntities.Add(wire.SecondEnd.ToString());
            }
        }

        foreach (var entityId in secondEndEntities)
        {
            if (_allEntities.ContainsKey(entityId))
            {
                var entity = _allEntities[entityId];
                entity.Material = new DiffuseMaterial(Brushes.Transparent);
            }
        }
    }

    public void ToggleSwitchStatus()
    {
        foreach (var sw in Switches)
        {
            if (_allEntities.ContainsKey(sw.Id))
            {
                var entity = _allEntities[sw.Id];
                if (sw.Status == "Open")
                {
                    entity.Material = new DiffuseMaterial(Brushes.Green);
                }
                else
                {
                    entity.Material = new DiffuseMaterial(Brushes.Crimson);
                }
            }
        }
    }

    public void NetworkResistanceMode()
    {
        foreach (var wire in Wires)
        {
            if (_allWires.ContainsKey(wire.Id))
            {
                var entity = _allWires[wire.Id];
                foreach (GeometryModel3D wirePart in entity.Children)
                {
                    if (wire.R < 1)
                    {
                        wirePart.Material = new DiffuseMaterial(Brushes.DarkRed);
                    }else if (wire.R > 2)
                    {
                        wirePart.Material = new DiffuseMaterial(Brushes.Blue);
                    }
                    else
                    {
                        wirePart.Material = new DiffuseMaterial(Brushes.Orange);
                    }
                }

            }
        }
    }

    public void ChangeWireState(string type,bool enabled, Brush brush)
    {
        foreach (var wire in Wires)
        {
            if (_allWires.ContainsKey(wire.Id))
            {
                var entity = _allWires[wire.Id];
                foreach (GeometryModel3D wirePart in entity.Children)
                {
                    if (wire.ConductorMaterial == type)
                    {
                        if (enabled)
                        {
                            wirePart.Material = new DiffuseMaterial(brush);
                        }
                        else
                        {
                            wirePart.Material = new DiffuseMaterial(Brushes.Transparent);
                        }
                    }
                }
            }
        }
    }

    private void DrawElectricItem(Brush brush, List<IElectricItem> items, double size=0.005, string defaultTypeName="ElectricItem")
    {
        foreach (IElectricItem electricItem in items)
        {
            double offsetX = Constants.SomethingMagical + ((electricItem.Point.X - Constants.MinLongitude) /
                Constants.LongitudeDiff * Constants.MapEdgeSize);
            
            double offsetY = Constants.SomethingMagical + ((electricItem.Point.Y - Constants.MinLatitude) /
                Constants.LatitudeDiff * Constants.MapEdgeSize);

            double offsetZ = 0;

            GeometryModel3D cube = CubeFactory.Create(size, brush);

            if (CheckOverlap(electricItem.Point, out Point newPoint))
            {
                offsetZ = Constants.Step + _verticalEntities[newPoint] * Constants.Step;
                _verticalEntities[newPoint]++;
            }
            else
            {
                offsetZ = Constants.Step;
                _verticalEntities.Add(electricItem.Point, 1);
            }

            cube.Transform = new TranslateTransform3D(offsetX - (size / 2), offsetY - (size / 2), offsetZ);
            
            EntityTooltips.Add(cube, $"Type: {defaultTypeName.ToUpper()}\nId: {electricItem.Id}\nName: {electricItem.Name}\nCoordinates:\nX: {electricItem.Point.X} Y: {electricItem.Point.Y}");
            
            if(!_allEntities.ContainsKey(electricItem.Id))
            {
                _allEntities.Add(electricItem.Id, cube); 
            }
            
            _scene.Children.Add(cube);
        }
    }

    private void DrawWires(List<Wire> wires)
    {
        foreach (var wire in wires)
        {
            Model3DGroup group = new Model3DGroup();
            for(int i=0; i < wire.Points.Count - 1; i++)
            {
                double startX = ConvertToMapCoordinates(wire.Points[i].X, Constants.MinLongitude, Constants.LongitudeDiff);
                double startY = ConvertToMapCoordinates(wire.Points[i].Y, Constants.MinLatitude, Constants.LatitudeDiff);
                double endX = ConvertToMapCoordinates(wire.Points[i + 1].X, Constants.MinLongitude, Constants.LongitudeDiff);
                double endY = ConvertToMapCoordinates(wire.Points[i + 1].Y, Constants.MinLatitude, Constants.LatitudeDiff);

                Point3D startPoint = new Point3D(startX, startY, 0.00005);
                Point3D endPoint = new Point3D(endX, endY, 0.00005);

                Brush materialBrush = Brushes.Gold;

                if (wire.ConductorMaterial == "Steel")
                {
                    materialBrush = Brushes.Gray;
                }
                if (wire.ConductorMaterial == "Acsr")
                {
                    materialBrush = Brushes.DarkSlateGray;
                }
                if (wire.ConductorMaterial == "Copper")
                {
                    materialBrush = Brushes.SaddleBrown;
                }

                GeometryModel3D wireGeometry = CreateCylinderGeometry(startPoint, endPoint, materialBrush);
                
                EntityTooltips.Add(wireGeometry, $"Type: WIRE\nId: {wire.Id}\nName: {wire.Name}\nFirst end: {wire.FirstEnd}\nSecond end: {wire.SecondEnd}\nMaterial: {wire.ConductorMaterial}");

                group.Children.Add(wireGeometry);
                
                _scene.Children.Add(wireGeometry);
            }
            
            if (!_allWires.ContainsKey(wire.Id))
            {
                _allWires.Add(wire.Id, group);
            }
        }
    }

    private double ConvertToMapCoordinates(double coordinate, double minCoordinate, double diffCoordinate)
    {
        return Constants.SomethingMagical +
               ((coordinate - minCoordinate) / diffCoordinate * Constants.MapEdgeSize);
    }

    private GeometryModel3D CreateCylinderGeometry(Point3D startPoint, Point3D endPoint, Brush brush, int resolution = 16)
    {
        MeshGeometry3D mesh = new MeshGeometry3D();
        Vector3D diffVector = endPoint - startPoint;

        for (int i = 0; i < resolution; i++)
        {
            double theta = (double)i / resolution * 2.0 * Math.PI;
            double nextTheta = (double)(i + 1) / resolution * 2.0 * Math.PI;
            Vector3D textureVector = new Vector3D(Math.Cos(theta), Math.Sin(theta), 0);
            Vector3D nextTextureVector = new Vector3D(Math.Cos(nextTheta), Math.Sin(nextTheta), 0);

            // Add the vertices for two triangles
            mesh.Positions.Add(startPoint + textureVector * 0.0005);
            mesh.Positions.Add(endPoint + textureVector * 0.0005);
            mesh.Positions.Add(endPoint + nextTextureVector * 0.0005);

            mesh.Positions.Add(endPoint + nextTextureVector * 0.0005);
            mesh.Positions.Add(startPoint + nextTextureVector * 0.0005);
            mesh.Positions.Add(startPoint + textureVector * 0.0005);

            // Add the indices
            for (int j = 0; j < 6; j++)
                mesh.TriangleIndices.Add(mesh.Positions.Count - 6 + j);
        }

        GeometryModel3D cylinderGeometry = new GeometryModel3D();
        DiffuseMaterial material = new DiffuseMaterial();
        material.Brush = brush;
        cylinderGeometry.Material = material;
        cylinderGeometry.Geometry = mesh;
        cylinderGeometry.Transform = new TranslateTransform3D(0, 0, Constants.Step);

        return cylinderGeometry;
    }  
    private bool CheckOverlap(Point point, out Point? newPoint)
    {
        foreach (var entity in _verticalEntities)
        {
            if (CalculateDistance(point, entity.Key) <= Constants.Step * 2)
            {
                newPoint = entity.Key;
                return true;
            }
        }

        newPoint = new Point();
        return false;
    }

    private double CalculateDistance(Point point1, Point point2)
    {
        // Haversine formula
        double theta = point1.X - point2.X;

        double radTheta = theta * Math.PI / 180.0;
        double radLat1 = point1.Y * Math.PI / 180.0;
        double radLat2 = point2.Y * Math.PI / 180.0;

        double dist = Math.Sin(radLat1) * Math.Sin(radLat2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Cos(radTheta);
        dist = Math.Acos(dist);
        dist = (dist / Math.PI * 180.0);
        dist = dist * 60 * 1.1515;
        dist *= 1.609344;
        return dist;
    }
    
    public void ColorWires(string id)
    {
        foreach (var wire in Wires)
        {
            if (wire.Id == id)
            {
                if (_previousEntity1 != null && _previousEntity2 != null)
                {
                    _previousEntity1.Material = _previousEntity1Material;
                    _previousEntity2.Material = _previousEntity2Material;
                }

                if (_allEntities.ContainsKey(wire.FirstEnd.ToString()) && _allEntities.ContainsKey(wire.SecondEnd.ToString()))
                {
                    var entity1 = _allEntities[wire.FirstEnd.ToString()];
                    _previousEntity1Material = entity1.Material;
                    _previousEntity1 = entity1;
                    DiffuseMaterial material1 = new DiffuseMaterial();
                    material1.Brush = Brushes.Black;
                    entity1.Material = material1;

                    var entity2 = _allEntities[wire.SecondEnd.ToString()];
                    _previousEntity2Material = entity2.Material;
                    _previousEntity2 = entity2;
                    DiffuseMaterial material2 = new DiffuseMaterial();
                    material2.Brush = Brushes.Black;
                    entity2.Material = material2;
                }

                return;
            }
        }
    }
}