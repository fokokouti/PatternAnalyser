using PADataProcessing.VoronoiToolBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAGui.DataLoader
{
    public static class DataManager
    {
        // UniqID_NUCL,N_COM_X,N_COM_Y,N_Orient
        public static string UniqIDLabel = "UniqID_NUCL";
        public static string ComXLabel = "N_COM_X";
        public static string ComYLabel = "N_COM_Y";
        public static string ComOrientationLabel = "N_Orient";
        public static double ScaleX = 5000;
        public static double ScaleY = 5000;
        public static System.Windows.Point MapVectorToPoint(this Vector Source, double ActualHeigth, bool scaled = false)
        {
            System.Windows.Point Result = new System.Windows.Point();
            if (Source.Dim == 2)
            {
                if (!scaled)
                {
                    Result = new System.Windows.Point(Source[0] / ScaleX, Source[1] / ScaleY);
                }
                else
                {
                    Result = new System.Windows.Point(Source[0], Source[1]);
                }
            }
            return Result;
        }

        public static Edge MapVoronoiEdgeToEdge(this VoronoiEdge Source, double ActualHeigth, bool Voronoi = true, bool scaled = false)
        {
            Edge Result = new Edge();
            if (Voronoi)
            {
                Result.A = Source.VVertexA.MapVectorToPoint(ActualHeigth, scaled);
                Result.B = Source.VVertexB.MapVectorToPoint(ActualHeigth, scaled);
            }
            else
            {
                Result.A = Source.LeftData.MapVectorToPoint(ActualHeigth, scaled);
                Result.B = Source.RightData.MapVectorToPoint(ActualHeigth, scaled);
            }

            return Result;
        }

        public static IEnumerable<System.Windows.Point> MapVectorHashSetToPointList(this PADataProcessing.VoronoiToolBox.HashSet<Vector> Source, double ActualHeigth, bool scaled = false)
        {
            List<System.Windows.Point> Result = new List<System.Windows.Point>();
            foreach (var elem in Source)
            {
                Result.Add(elem.MapVectorToPoint(ActualHeigth, scaled));
            }
            return Result;
        }

        public static IEnumerable<Edge> MapEdgeHashSetToEdgeList(this PADataProcessing.VoronoiToolBox.HashSet<VoronoiEdge> Source,  double ActualHeigth, bool Vorornoi = true, bool scaled = false)
        {
            List<Edge> Result = new List<Edge>();
            foreach (var elem in Source)
            {
                Result.Add(elem.MapVoronoiEdgeToEdge(ActualHeigth, Vorornoi, scaled));
            }
            return Result;
        }

        public static List<Dictionary<string, string>> LoadPointCloudFromCsv(string FilePath, char Separator = ',')
        {
            List<Dictionary<string, string>> ResultList = new List<Dictionary<string, string>>();
            try
            {
                StreamReader streamReader = new StreamReader(FilePath);
                string Line = "";
                string[] Fields = null;
                int lineCounter = 0;
                List<string> Headers = new List<string>();
                while (!streamReader.EndOfStream)
                {
                    Line = streamReader.ReadLine();
                    if (Line == null || Line != string.Empty || Line.Length == 0)
                    {
                        if (Line.IndexOf(Separator) == -1)
                        {
                            throw new ArgumentException($"Wrong separator ({Separator}) provided. open your csv or txt file in an editor and check the right separator");
                        }

                        Fields = Line.Split(Separator);
                        if (lineCounter == 0)
                        {
                            for (int i = 0; i < Fields.Length; i++)
                            {
                                Headers.Add(Fields[i].Trim());
                            }
                        }
                        else
                        {
                            int attrCounter = 0;
                            Dictionary<string, string> EntryLine = new Dictionary<string, string>();
                            for (int i = 0; i < Fields.Length; i++)
                            {
                                string attr = Fields[i].Trim();
                                if (attr != null)
                                {
                                    EntryLine.Add(Headers.ElementAt(i), attr);
                                    if (attr.Length > 0)
                                        attrCounter++;
                                }
                            }
                            if (attrCounter > 0)
                                ResultList.Add(EntryLine);
                        }
                    }
                    lineCounter++;
                }
                streamReader.Close();
            }
            catch (IOException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }

            return ResultList;
        }

        public static IEnumerable<Vector> ToIEnumerableVectorList(this List<Dictionary<string, string>> Source)
        {
            List<Vector> Result = new List<Vector>();
            foreach (Dictionary<string, string> elem in Source)
            {
                string XValue;
                string YValue;
                double X = 0, Y = 0;

                elem.TryGetValue(ComXLabel, out XValue);
                elem.TryGetValue(ComYLabel, out YValue);

                if (XValue != null && XValue.Length > 1)
                {
                    double.TryParse(XValue, out X);
                }

                if (YValue != null && YValue.Length > 1)
                {
                    double.TryParse(YValue, out Y);
                }

                Vector VectorElem = new Vector(X, Y);
                Result.Add(VectorElem);
            }

            return Result;
        }

        public static IEnumerable<System.Windows.Point> ToIEnumerableNodeList(this List<Dictionary<string, string>> Source, ref double[] BoundingBox)
        {
            List<System.Windows.Point> Result = new List<System.Windows.Point>();
            foreach (Dictionary<string, string> elem in Source)
            {
                string XValue;
                string YValue;
                double X = 0, Y = 0;

                elem.TryGetValue(ComXLabel, out XValue);
                elem.TryGetValue(ComYLabel, out YValue);

                if (XValue != null && XValue.Length > 1)
                {
                    double.TryParse(XValue, out X);
                }

                if (YValue != null && YValue.Length > 1)
                {
                    double.TryParse(YValue, out Y);
                }

                if ((X / ScaleX) > BoundingBox[2])
                {
                    BoundingBox[2] = X / ScaleX;
                }
                else if ((X / ScaleX) < BoundingBox[0])
                {
                    BoundingBox[0] = X / ScaleX;
                }
                if ((Y / ScaleY) > BoundingBox[3])
                {
                    BoundingBox[3] = Y / ScaleY;
                }
                else if ((Y / ScaleY) < BoundingBox[1])
                {
                    BoundingBox[1] = Y / ScaleY;
                }

                System.Windows.Point point = new System.Windows.Point(X / ScaleX, Y / ScaleY);
                Result.Add(point);
            }

            BoundingBox[0] = BoundingBox[0] - 50;
            BoundingBox[1] = BoundingBox[1] - 20;
            BoundingBox[2] = BoundingBox[2] + 50;
            BoundingBox[3] = BoundingBox[3] + 20;

            return Result;
        }

        public static Dictionary<string, object> GetOneKeyValues(this List<Dictionary<string, string>> Source, string KeyName = "UniqID_NUCL")
        {
            Dictionary<string, object> Result = new Dictionary<string, object>();

            for(int i = 0; i < Source.Count(); i++)
            {
                string KeyValue;

                Source[i].TryGetValue(KeyName, out KeyValue);
                Result.Add($"Node_{i}", KeyValue);
            }

            return Result;
        }
    }
}
