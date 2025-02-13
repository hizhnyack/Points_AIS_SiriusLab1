using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using System.Xml.Serialization;
using Newtonsoft.Json;
using PointLib;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Point = PointLib.Point;


namespace FormsApp
{
    public partial class PointForm : Form
    {
        private Point[] points = null;
        public PointForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            points = new Point[5];

            var rnd = new Random();

            for (int i = 0; i < points.Length; i++)
                points[i] = rnd.Next(3) % 2 == 0 ? new Point() : new Point3D();

            listBox.DataSource = points;
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            if (points == null)
                return;

            Array.Sort(points);

            listBox.DataSource = null;
            listBox.DataSource = points;
        }
        private void btnSerialize_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "SOAP|*.soap|XML|*.xml|JSON|*.json|YAML|*.yaml|Binary|*.bin|Custom Format|*.cust";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            using (var fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
            {
                switch (Path.GetExtension(dlg.FileName))
                {
                    case ".bin":
                        var bf = new BinaryFormatter();
                        bf.Serialize(fs, points);
                        break;
                    case ".soap":
                        var sf = new SoapFormatter();
                        sf.Serialize(fs, points);
                        break;
                    case ".xml":
                        var xf = new XmlSerializer(typeof(Point[]), new[] { typeof(Point3D) });
                        xf.Serialize(fs, points);
                        break;
                    case ".json":
                        using (var w = new StreamWriter(fs))
                        {
                            var jf = new JsonSerializer();
                            jf.Serialize(w, points);
                        }
                        break;
                    case ".yaml":
                        var serializer = new SerializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .Build();
                        using (var w = new StreamWriter(fs))
                        {
                            string yaml = serializer.Serialize(points);
                            w.Write(yaml);
                        }
                        break;
                    case ".cust":
                        using (var writer = new StreamWriter(fs))
                        {
                            foreach (var point in points)
                            {
                                writer.WriteLine(SerializePointToCustomFormat(point));
                            }
                        }
                        break;

                }
            }
        }
        private void btnDeserialize_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "SOAP|*.soap|XML|*.xml|JSON|*.json|YAML|*.yaml|Binary|*.bin|Custom Format|*.cust";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            using (var fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
            {
                switch (Path.GetExtension(dlg.FileName))
                {
                    case ".bin":
                        var bf = new BinaryFormatter();
                        points = (Point[])bf.Deserialize(fs);
                        break;
                    case ".soap":
                        var sf = new SoapFormatter();
                        points = (Point[])sf.Deserialize(fs);
                        break;
                    case ".xml":
                        var xf = new XmlSerializer(typeof(Point[]), new[] { typeof(Point3D) });
                        points = (Point[])xf.Deserialize(fs);
                        break;
                    case ".json":
                        using (var r = new StreamReader(fs))
                        {
                            string json = r.ReadToEnd();
                            points = JsonConvert.DeserializeObject<Point3D[]>(json);
                        }
                        break;
                    case ".yaml":
                        using (var r = new StreamReader(fs))
                        {
                            var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                .Build();
                            points = deserializer.Deserialize<Point[]>(r);
                        }
                        break;
                    case ".cust":
                        using (var reader = new StreamReader(fs))
                        {
                            var tempPoints = new System.Collections.Generic.List<Point>();
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                var point = DeserializePointFromCustomFormat(line);
                                tempPoints.Add(point);
                            }
                            points = tempPoints.ToArray();
                        }
                        break;
                }
            }


            listBox.DataSource = null;
            listBox.DataSource = points;
        }
        private string SerializePointToCustomFormat(Point point)
        {
            if (point is Point3D point3D)
            {
                return $"X={point.X},Y={point.Y},Z={point3D.Z}";
            }
            else
            {
                return $"X={point.X},Y={point.Y}";
            }
        }

        private Point DeserializePointFromCustomFormat(string line)
        {
            var parts = line.Split(',');

            int x = 0, y = 0, z = 0;

            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    switch (keyValue[0].Trim())
                    {
                        case "X":
                            x = int.Parse(keyValue[1].Trim());
                            break;
                        case "Y":
                            y = int.Parse(keyValue[1].Trim());
                            break;
                        case "Z":
                            z = int.Parse(keyValue[1].Trim());
                            break;
                    }
                }
            }
            if (z == 0)
            {
                return new Point { X = x, Y = y };
            }
            else
            {
                return new Point3D { X = x, Y = y, Z = z };
            }
        }
    }
}
