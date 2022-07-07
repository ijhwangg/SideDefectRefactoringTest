using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WVision;

namespace SideDefectRefactoringTest
{
    internal static class MainApp                       
    {
        private static void Main(string[] args)
        {
            var basePath = @"C:\Users\WTA\Desktop\Daily Work\22.07\Tester";
            var shapeTypes = new List<string>() { "C", "T", "V" };
            var types = new List<string>() { "N", "P" };

            var bluImgName = @"SidBack.bmp";
            var faceImgName = @"SidColor.bmp";
            var noseImgName = @"SidSideF.bmp";
          
            var recipeName = @"tempRcp.rcp";
            var referenceName = @"Result_ref.dat";
            var dataName = @"Result.dat";

            Bitmap bmpB = null;
            BitmapData bmpBData = null;
            Bitmap bmpF = null;
            BitmapData bmpFData = null;
            Bitmap bmpN = null;
            BitmapData bmpNData = null;

            foreach (var shapeType in shapeTypes)
            {
                foreach (var type in types)
                {
                    var baseFolder = $@"{basePath}\{shapeType}\{type}";

                    var bluImgPath = $@"{baseFolder}\{bluImgName}";
                    var faceImgPath = $@"{baseFolder}\{faceImgName}";
                    var noseImgPath = $@"{baseFolder}\{noseImgName}";

                    var recipePath = $@"{baseFolder}\{recipeName}";
                    var referencePath = $@"{baseFolder}\{referenceName}";
                    var dataPath = $@"{baseFolder}\{dataName}";

                    if (File.Exists(dataPath)) File.Delete(dataPath);

                    Console.WriteLine($"Type = {shapeType} : {type}");

                    // run
                    try
                    {
                        SideDefectCpp.Create();


                        bmpB = new Bitmap(bluImgPath);
                        bmpBData = bmpB.LockBits(
                            new Rectangle(0, 0, bmpB.Width, bmpB.Height),
                            ImageLockMode.ReadOnly, bmpB.PixelFormat);
                        bmpF = new Bitmap(faceImgPath);
                        bmpFData = bmpF.LockBits(
                            new Rectangle(0, 0, bmpF.Width, bmpF.Height),
                            ImageLockMode.ReadOnly, bmpF.PixelFormat);
                        bmpN = new Bitmap(noseImgPath);
                        bmpNData = bmpN.LockBits(
                            new Rectangle(0, 0, bmpN.Width, bmpN.Height),
                            ImageLockMode.ReadOnly, bmpN.PixelFormat);

                        SideDefectCpp.SetBackLightImage(bmpBData.Scan0, bmpBData.Width, bmpBData.Height);
                        SideDefectCpp.SetFrontImage(bmpFData.Scan0, bmpFData.Width, bmpFData.Height);
                        SideDefectCpp.SetNoseRImage(bmpNData.Scan0, bmpNData.Width, bmpNData.Height);


                        SideDefectCpp.SetRecipe(recipePath);

                        SideDefectCpp.Compute();

                        SideDefectCpp.WriteData(dataPath);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.ToString());
                    }
                    finally
                    {
                        SideDefectCpp.Delete();

                        if (bmpB != null)
                        {
                            if (bmpBData != null)
                            {
                                bmpB.UnlockBits(bmpBData);
                            }

                            bmpB.Dispose();
                        }
                        if (bmpF != null)
                        {
                            if (bmpFData != null)
                            {
                                bmpF.UnlockBits(bmpFData);
                            }

                            bmpF.Dispose();
                        }
                        if (bmpN != null)
                        {
                            if (bmpNData != null)
                            {
                                bmpN.UnlockBits(bmpNData);
                            }

                            bmpN.Dispose();
                        }

                        NativeLogger.Flush("logging.txt", 0);
                    }

                    // test
                    if (File.Exists(dataPath))
                    {
                        bool isFail = false;
                        int line = 0;

                        using (var referenceReader = new StreamReader(referencePath))
                        using (var dataReader = new StreamReader(dataPath))
                        {
                            while (!referenceReader.EndOfStream)
                            {
                                line += 1;

                                if (dataReader.EndOfStream)
                                {
                                    isFail = true;
                                    break;
                                }

                                var rl = referenceReader.ReadLine().Trim();
                                var dl = dataReader.ReadLine().Trim();

                                if (rl != dl)
                                {
                                    isFail = true;
                                    break;
                                }
                            }
                        }

                        if (isFail)
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"    -->  X  (@line{line})");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("    -->  O");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("    -->  X  (file not exist)");
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
