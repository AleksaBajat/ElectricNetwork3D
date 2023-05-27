using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ElectricNetwork3D.Lib;
using ElectricNetwork3D.Models;


namespace ElectricNetwork3D
{
    public static class DataAccess
    {
        
         private static void InitWires(XmlDocument xmlDocument, List<Wire> wires)
        {
            XmlNode wireNodes = xmlDocument.SelectSingleNode("//Lines");

            foreach (XmlNode wire in wireNodes.ChildNodes)
            {
                GetNodeValue<string>(wire, "Id", out string id);
                GetNodeValue<string>(wire, "Name", out string name);
                GetNodeValue<bool>(wire, "IsUnderground", out bool isUnderground);
                GetNodeValue<double>(wire, "R", out double r);
                GetNodeValue<string>(wire, "ConductorMaterial", out string conductorMaterial);
                GetNodeValue<string>(wire, "LineType", out string lineType);
                GetNodeValue<int>(wire, "ThermalConstantHeat", out int thermalConstantHeat);
                GetNodeValue<int>(wire, "FirstEnd", out int firstEnd);
                GetNodeValue<int>(wire, "SecondEnd", out int secondEnd);
                List<Point> points = new List<Point>();
                XmlNode verticesNode = wire.SelectSingleNode("Vertices");
                XmlNodeList pointNodes = verticesNode.SelectNodes("Point");

                var iter = pointNodes.GetEnumerator();
                iter.Reset();

                while (iter.MoveNext())
                {
                    XmlNode node = (XmlNode)iter.Current;
                    var x = double.Parse(node.SelectSingleNode("X").InnerText);
                    var y = double.Parse(node.SelectSingleNode("Y").InnerText);
                    Geographic.ToLatLon(x,y, 34, out double latitude, out double longitude);

                    if (latitude > Constants.MinLatitude && latitude < Constants.MaxLatitude && longitude > Constants.MinLongitude &&
                        longitude < Constants.MaxLongitude)
                    {
                        Point point = new Point(longitude, latitude);
                        points.Add(point);
                    }
                }
                
                if (points.Count >= 2)
                    wires.Add(new Wire(id, name,
                        isUnderground, r, lineType,
                        thermalConstantHeat, firstEnd, secondEnd, points, conductorMaterial));
            }
        }


        private static void InitSwitches(XmlDocument xmlDocument, List<Switch> switches)
        {
            XmlNode swNodes = xmlDocument.SelectSingleNode("//Switches");

            foreach(XmlNode sw in swNodes.ChildNodes)
            {
                GetNodeValue<string>(sw, "Id", out string? id);
                GetNodeValue<string>(sw, "Name", out string? name);
                GetNodeValue<string>(sw, "Status", out string? status);
                var r1 = GetNodeValue<double>(sw, "X", out double x);
                var r2 = GetNodeValue<double>(sw, "Y", out double y);

                if (r1 && r2)
                {
                    Geographic.ToLatLon(x,y, 34, out double latitude, out double longitude);
                    if (latitude > Constants.MinLatitude && latitude < Constants.MaxLatitude && longitude > Constants.MinLongitude &&
                        longitude < Constants.MaxLongitude)
                        switches.Add(new Switch(id, name, new Point(longitude, latitude), status));
                }
            } 
        }


        private static void InitNodes(XmlDocument xmlDocument, List<Node> nodes)
        {
            XmlNode nodesNode = xmlDocument.SelectSingleNode("//Nodes");

            foreach(XmlNode node in nodesNode.ChildNodes)
            {
                GetNodeValue<string>(node, "Id", out string id);
                GetNodeValue<string>(node, "Name", out string name);
                var r1 = GetNodeValue<double>(node, "X", out double x);
                var r2 = GetNodeValue<double>(node, "Y", out double y);

                if (r1 && r2)
                {
                    Geographic.ToLatLon(x,y, 34, out double latitude, out double longitude);
                    if (latitude > Constants.MinLatitude && latitude < Constants.MaxLatitude && longitude > Constants.MinLongitude &&
                        longitude < Constants.MaxLongitude)
                        nodes.Add(new Node(id, name, new Point(longitude, latitude)));
                }

            } 
        }



        private static void InitSubstations(XmlDocument xmlDocument, List<Substation> substations)
        {
            XmlNode substationsNode = xmlDocument.SelectSingleNode("//Substations");

            foreach(XmlNode substationNode in substationsNode.ChildNodes)
            {
                GetNodeValue<string>(substationNode, "Id", out string id);
                GetNodeValue<string>(substationNode, "Name", out string name);
                var r1 = GetNodeValue<double>(substationNode, "X", out double x);
                var r2 = GetNodeValue<double>(substationNode, "Y", out double y);
                
                if (r1 && r2)
                {
                    Geographic.ToLatLon(x,y, 34, out double latitude, out double longitude);
                    if (latitude > Constants.MinLatitude && latitude < Constants.MaxLatitude && longitude > Constants.MinLongitude &&
                        longitude < Constants.MaxLongitude)
                        substations.Add(new Substation(id, name, new Point(longitude, latitude)));
                }
            }
        }


        public static bool TryParse<T>(string s, out T result)
        {
            result = default(T);

            Type type = typeof(T);

            if (typeof(T) == typeof(string))
            {
                result = (T)(object)s;
                return true;
            }
            
            MethodInfo tryParseMethod = type.GetMethod("TryParse", new[] { typeof(string), type.MakeByRefType() });

            

            if (tryParseMethod != null)
            {
                object[] args = { s, result };
                bool success = (bool)tryParseMethod.Invoke(null, args);

                result = (T)args[1];
                return success;
            }

            return false;
        }
        
        private static bool GetNodeValue<T>(XmlNode node,string name, out T value)
        {
            var dataContainer= node.SelectSingleNode(name);

            if (dataContainer == null)
            {
                value = default(T);
                return false;
            }

            return TryParse<T>(dataContainer.InnerText, out value);
        }
        
        public static void InitData(List<Node> nodes, List<Substation> substations,
            List<Switch> switches, List<Wire> wires)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            string path = System.IO.Path.Combine(solutionDirectory, "Content", "Geographic.xml");
            XmlDocument xmlDocument = new XmlDocument();
            if (File.Exists(path))
            {
                xmlDocument.Load(path);
            }

            XmlNode rootNode = xmlDocument.SelectSingleNode("NetworkModel");

            InitSwitches(xmlDocument, switches);
            InitSubstations(xmlDocument, substations);
            InitNodes(xmlDocument, nodes);
            InitWires(xmlDocument, wires);
        }
        
    }
}