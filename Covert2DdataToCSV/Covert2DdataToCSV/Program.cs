using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Covert2DdataToCSV
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a file or folder path.");
                return;
            }

            int rows = 100;
            int cols = 100;
            if (int.TryParse(args[0], out int t))
            {
                Console.WriteLine($"rows is : {t}");
                rows = t;
            }
            if (int.TryParse(args[1], out int tx))
            {
                Console.WriteLine($"cols is : {tx}");
                cols = tx;
            }
            string inputPath = args[2];
            List<string> fileNames = new List<string>();

            // Check if input is a file
            if (File.Exists(inputPath))
            {
                // Add the file name to the collection
                fileNames.Add(Path.GetFullPath(inputPath));
            }
            // Check if input is a folder
            else if (Directory.Exists(inputPath))
            {
                // Get all files in the folder and add their names to the collection
                string[] files = Directory.GetFiles(inputPath);
                foreach (var file in files)
                {
                    fileNames.Add(Path.GetFullPath(file));
                }
            }
            else
            {
                Console.WriteLine("The provided path is not valid.");
                return;
            }

            // Output the collected file names
            Console.WriteLine("Files in the collection:");
            foreach (var fileName in fileNames)
            {
                Console.WriteLine(fileName);
                uint[,] arrays = Read2DUnsignedIntArrayFromBinaryFile(fileName, rows, cols);
                string newfile = $"{fileName}.csv";

                SaveRegionPointsCloudToFile(newfile, arrays, rows, cols);
            }

            return;
        }

        static uint[,] Read2DUnsignedIntArrayFromBinaryFile(string filePath, int rows, int cols)
        {
            uint[,] result = new uint[rows, cols];

            // Read the binary file
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        result[i, j] = reader.ReadUInt32();  // Reading each 32-bit unsigned integer
                    }
                }
            }

            return result;
        }

        public struct PointXYZRGB
        {
            public float x;
            public float y;
            public float z;
            public byte r;
            public byte g;
            public byte b;

            public PointXYZRGB(float x, float y, float z, byte r, byte g, byte b)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.r = r;
                this.g = g;
                this.b = b;
            }
        }
        public static void SavePointCloud(string filePath, List<PointXYZRGB> points)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.ASCII))
                {
                    writer.WriteLine("x,y,z,r,g,b");

                    foreach (PointXYZRGB point in points)
                    {
                        writer.WriteLine($"{point.x.ToString(CultureInfo.InvariantCulture)},{point.y.ToString(CultureInfo.InvariantCulture)},{point.z.ToString(CultureInfo.InvariantCulture)},{point.r},{point.g},{point.b}");
                    }
                }

                Console.WriteLine("Point cloud data saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while saving the point cloud data.");
                Console.WriteLine(ex.Message);
            }
        }

        private static void SaveRegionPointsCloudToFile(string pointCloudPath, uint[,] regions, int outputWidth, int outputHeight)
        {
            int infoWidth = regions.GetLength(0);
            int infoHeight = regions.GetLength(1);
            float xOutputRatio = outputWidth / infoWidth;
            float yOutputRatio = outputHeight / infoHeight;

            List<PointXYZRGB> pcList = new List<PointXYZRGB>();
            Color color = Color.Orange;
            for (int i = 0; i < infoWidth; i++)
            {
                for (int j = 0; j < infoHeight; j++)
                {
                    if (
                        (regions[i, j] != 0)
                        )
                    {
                        bool isExtend = false;
                        if (isExtend)
                        {
                            int xStep = (int)(xOutputRatio);
                            int yStep = (int)(yOutputRatio);
                            int xStart = (int)(i * xOutputRatio + 1);
                            int xEnd = xStart + xStep;
                            int yStart = (int)(j * yOutputRatio + 1);
                            int yEnd = yStart + yStep;
                            for (int m = xStart; m < xEnd; m++)
                            {
                                for (int n = yStart; n < yEnd; n++)
                                {
                                    float xValue = m;
                                    float yValue = n;
                                    float zValue = regions[i, j];
                                    PointXYZRGB pc = new PointXYZRGB(xValue, yValue, zValue, color.R, color.G, color.B);
                                    pcList.Add(pc);
                                }
                            }
                        }
                        else
                        {
                            float xValue = i * xOutputRatio;
                            float yValue = j * yOutputRatio;
                            float zValue = regions[i, j];

                            PointXYZRGB pc = new PointXYZRGB(xValue, yValue, zValue, color.R, color.G, color.B);
                            pcList.Add(pc);
                        }
                    }
                }
            }
            SavePointCloud(pointCloudPath, pcList);
        }
    }
}
