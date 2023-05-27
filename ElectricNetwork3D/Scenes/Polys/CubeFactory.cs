using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ElectricNetwork3D.Scenes.Polys;

public static class CubeFactory
{
    public static GeometryModel3D Create(double size, Brush brush)
    {
        var geometryModel = new GeometryModel3D();
        var meshGeometry = new MeshGeometry3D();

        meshGeometry.Positions.Add(new Point3D(0, 0, 0));
        meshGeometry.Positions.Add(new Point3D(size, 0, 0));
        meshGeometry.Positions.Add(new Point3D(0, size, 0));
        meshGeometry.Positions.Add(new Point3D(size, size, 0));
        meshGeometry.Positions.Add(new Point3D(0, 0, size));
        meshGeometry.Positions.Add(new Point3D(size, 0, size));
        meshGeometry.Positions.Add(new Point3D(0, size, size));
        meshGeometry.Positions.Add(new Point3D(size, size, size));

        meshGeometry.TriangleIndices.Add(2);
        meshGeometry.TriangleIndices.Add(3);
        meshGeometry.TriangleIndices.Add(1);

        meshGeometry.TriangleIndices.Add(2);
        meshGeometry.TriangleIndices.Add(1);
        meshGeometry.TriangleIndices.Add(0);

        meshGeometry.TriangleIndices.Add(7);
        meshGeometry.TriangleIndices.Add(1);
        meshGeometry.TriangleIndices.Add(3);

        meshGeometry.TriangleIndices.Add(7);
        meshGeometry.TriangleIndices.Add(5);
        meshGeometry.TriangleIndices.Add(1);

        meshGeometry.TriangleIndices.Add(6);
        meshGeometry.TriangleIndices.Add(5);
        meshGeometry.TriangleIndices.Add(7);

        meshGeometry.TriangleIndices.Add(6);
        meshGeometry.TriangleIndices.Add(4);
        meshGeometry.TriangleIndices.Add(5);

        meshGeometry.TriangleIndices.Add(6);
        meshGeometry.TriangleIndices.Add(2);
        meshGeometry.TriangleIndices.Add(4);

        meshGeometry.TriangleIndices.Add(2);
        meshGeometry.TriangleIndices.Add(0);
        meshGeometry.TriangleIndices.Add(4);

        meshGeometry.TriangleIndices.Add(2);
        meshGeometry.TriangleIndices.Add(7);
        meshGeometry.TriangleIndices.Add(3);

        meshGeometry.TriangleIndices.Add(2);
        meshGeometry.TriangleIndices.Add(6);
        meshGeometry.TriangleIndices.Add(7);

        meshGeometry.TriangleIndices.Add(0);
        meshGeometry.TriangleIndices.Add(1);
        meshGeometry.TriangleIndices.Add(5);

        meshGeometry.TriangleIndices.Add(0);
        meshGeometry.TriangleIndices.Add(5);
        meshGeometry.TriangleIndices.Add(4);
        geometryModel.Geometry = meshGeometry;

        var material = new DiffuseMaterial(brush);
        geometryModel.Material = material;
        return geometryModel; 
    }
    
}