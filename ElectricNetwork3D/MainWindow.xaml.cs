using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using ElectricNetwork3D.Models;
using ElectricNetwork3D.Scenes;

namespace ElectricNetwork3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private int _maxZoom = 40;
        private int _currentZoom = 1;
        private System.Windows.Point _diffOffset = new System.Windows.Point();
        private System.Windows.Point _previousPosition = new System.Windows.Point(0, 0);
        private Dictionary<GeometryModel3D, string> _entityTooltips;
        private ToolTip _toolTip = new ToolTip();
        private MainScene _electricScene;

        public MainWindow()
        {
            InitializeComponent();

            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            string path = System.IO.Path.Combine(solutionDirectory, "Content", "map.jpg");

            MapBrush.ImageSource = new BitmapImage(new Uri(path));

            _electricScene = new MainScene(ElectricScene);
            _entityTooltips = _electricScene.EntityTooltips;
        }
        
        private void ViewPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _toolTip.IsOpen = false;
            ViewportLand.CaptureMouse();
            _previousPosition = e.GetPosition(this);
            _diffOffset.X = Translate.OffsetX;
            _diffOffset.Y = Translate.OffsetY;

            System.Windows.Point mousePosition = e.GetPosition(ViewportLand);
            Point3D testpoint3D = new Point3D(mousePosition.X, mousePosition.Y, 0);
            Vector3D testdirection = new Vector3D(mousePosition.X, mousePosition.Y, 10);
            
            PointHitTestParameters pointparams = new PointHitTestParameters(mousePosition);
            RayHitTestParameters rayparams = new RayHitTestParameters(testpoint3D, testdirection);
            VisualTreeHelper.HitTest(ViewportLand, null, HitTest, pointparams);
        }
        
        private HitTestResultBehavior HitTest(HitTestResult result)
        {
            
            RayHitTestResult rayResult = result as RayHitTestResult;

            if (rayResult != null)
            {
                foreach (KeyValuePair<GeometryModel3D, string> pair in _entityTooltips)
                {
                    if (pair.Key == rayResult.ModelHit)
                    {
                        _toolTip.Content = pair.Value;
                        _toolTip.Dispatcher.Invoke(new Action(() => { _toolTip.IsOpen = true; }));
                        if (pair.Value.ToLower().Contains("wire"))
                        {
                            var id = pair.Value.Split(':', '\n')[3].Trim();
                            _electricScene.ColorWires(id);
                        }
                    }
                }
            }
            return HitTestResultBehavior.Stop;
        }


        private void ViewPort_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleX, scaleY, scaleZ;
            if (e.Delta > 0 && _currentZoom < _maxZoom)
            {
                scaleX = Scale.ScaleX + 0.1;
                scaleY = Scale.ScaleY + 0.1;
                scaleZ = Scale.ScaleZ + 0.1;
                Scale.ScaleX = scaleX;
                Scale.ScaleY = scaleY;
                Scale.ScaleZ = scaleZ;

                _currentZoom++;
                Camera.FieldOfView--;
            }
            else if (e.Delta <= 0 && _currentZoom > -_maxZoom)
            {
                scaleX = Scale.ScaleX - 0.1;
                scaleY = Scale.ScaleY - 0.1;
                scaleZ = Scale.ScaleZ - 0.1;
                Scale.ScaleX = scaleX;
                Scale.ScaleY = scaleY;
                Scale.ScaleZ = scaleZ;

                _currentZoom--;
                Camera.FieldOfView++;
            }
        }
        
        private void ViewPort_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewportLand.ReleaseMouseCapture();
        }

        private void ViewPort_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point currentPosition = e.GetPosition(this);

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                double offsetX = currentPosition.X - _previousPosition.X;
                double offsetY = currentPosition.Y - _previousPosition.Y;
                double step = 0.2;
                if ((AngleRotationX.Angle + (step * offsetY)) < 70 && (AngleRotationX.Angle + (step * offsetY)) > -70)
                    AngleRotationX.Angle += step * offsetY;
                if ((AngleRotationY.Angle + (step * offsetX)) < 70 && (AngleRotationY.Angle + (step * offsetX)) > -70)
                    AngleRotationY.Angle += step * offsetX;
            }
            

            if (ViewportLand.IsMouseCaptured)
            {
                System.Windows.Point end = e.GetPosition(this);
                double offsetX = end.X - _previousPosition.X;
                double offsetY = end.Y - _previousPosition.Y;
                double w = Width;
                double h = Height;
                double translateX = (offsetX * 100) / w;
                double translateY = -(offsetY * 100) / h;
                Translate.OffsetX = _diffOffset.X + (translateX / (100 * Scale.ScaleX));
                Translate.OffsetY = _diffOffset.Y + (translateY / (100 * Scale.ScaleX));
                
                _diffOffset.X = Translate.OffsetX;
                _diffOffset.Y = Translate.OffsetY;
            }
            
            _previousPosition = currentPosition;
        }

        private void ActivityButton_OnClick(object sender, RoutedEventArgs e)
        {
            _electricScene.ToggleNetworkActivity();
        }

        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            _electricScene.DrawDefaultScene();
        }

        private void SwitchButton_OnClick(object sender, RoutedEventArgs e)
        {
            _electricScene.ToggleSwitchStatus();
        }

        private void NetworkResistance_OnClick(object sender, RoutedEventArgs e)
        {
            _electricScene.NetworkResistanceMode();
        }

        private void SteelCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            _electricScene.ChangeWireState("Steel", false, Brushes.Transparent);
        }

        private void SteelCheck_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _electricScene.ChangeWireState("Steel", true, Brushes.Gray);
        }

        private void AscrCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            _electricScene.ChangeWireState("Acsr", false, Brushes.Transparent);
        }

        private void AscrCheck_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _electricScene.ChangeWireState("Acsr", true, Brushes.DarkSlateGray);
        }

        private void CopperCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            _electricScene.ChangeWireState("Copper", false, Brushes.Transparent);
        }

        private void CopperCheck_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _electricScene.ChangeWireState("Copper", true, Brushes.SaddleBrown);
        }

        private void OtherCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            _electricScene.ChangeWireState("Other", false, Brushes.Transparent);
        }

        private void OtherCheck_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _electricScene.ChangeWireState("Other", true, Brushes.Gold);
        }
    }
}