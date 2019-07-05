
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zip;
using System.IO.Compression;



namespace MVArchive
{
    class Program
    {
    
        static void Main(string[] args)
        {
            //toSplit(@"C:\github", "Duma_Aleksandr_Bolshoi_kulinarnyi_slovar_Litmir.net_bid177525_original_27179.pdf", 30 * 1024 * 1024);
            join(@"C:\github", "Duma_Aleksandr_Bolshoi_kulinarnyi_slovar_Litmir.net_bid177525_original_27179.pdf");
        }

        static void toSplit(string path, string fileName, int size)
        {
            byte[] bt = File.ReadAllBytes(path + "\\" + fileName);

            // Нахождения индекса элемента с которого начинается расширение
            int indxx = 0;
            for (int i = 0; i < fileName.Length; i++)
            {
                if (fileName[i] == '.')
                {
                    indxx = i;
                }
            }
            // Находит расширение
            string extension = "";
            for (int i = indxx; i < fileName.Length; i++)
            {
                extension += fileName[i];
            }

            // Имя файла без расширения
            string flname = "";
            for (int i = 0; i < fileName.Length-extension.Length; i++)
            {
                flname += fileName[i];
            }

            // Определяет количество архивов
            int count = (bt.Length + size - 1) / size;
            int cnt = 0;

            
            for (int i = 0; i < count; i++)
            {
                //Создает архив
                using (FileStream zipToOpen = new FileStream(path + "\\" +flname+"@"+i + ".zip", FileMode.OpenOrCreate))
                {
                    //Открывает архив
                    using (ZipArchive zip = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        //Создает файл внутри архива
                        ZipArchiveEntry readmeEntry = zip.CreateEntry(i + extension);
                        //Открывает поток
                        using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                        {

                            long _byte_counter = size;
                            // Записывает байты в поток
                            while (_byte_counter > 0 && cnt < bt.Length)
                            {
                                _byte_counter--;
                                writer.BaseStream.WriteByte(bt[cnt]);
                                cnt++;
                            }
                        }


                    }
                }
            }
           
            // Создание служебного файла; bt.Length - количество бит; size - размер 1-го тома; extension - расширение; fileName - имя без расширения
            //System.IO.File.WriteAllText(path + "\\" +flname+ "_Info.txt", bt.Length.ToString() + "\r\n" + size + "\r\n" + extension);

           
        }

        static void join(string path, string fileName)
        {
            
            int indxx = 0;
            for (int i = 0; i < fileName.Length; i++)
            {
                if (fileName[i] == '.')
                {
                    indxx = i;
                }
            }

            // Определяет имя файла без расширения
            string flname = "";
            for (int i = 0; i < indxx; i++)
            {
                flname += fileName[i];
            }


            string extension = "";
            for (int i = indxx; i < fileName.Length; i++)
            {
                extension += fileName[i];
            }
           

            long bytesCount = 0;
            //Нахождение всех файлов с фильтром *@*.zip
            string[] allFoundFiles = Directory.GetFiles(path, "*@*.zip", SearchOption.AllDirectories);
            long count = 0;
            //int size = allFoundFiles.Length;
            //int bytesCount = Convert.ToInt32(nbOfByte);
           
         
            int[,] array = new int[allFoundFiles.Length, 2];


            string tm = "";
            // Записывает файлы в двумерный массив; 1-й столбец - число после @; 2-й столбец - индекс этого файла
            for (int i = 0; i < allFoundFiles.Length; i++)
            {
                var a = allFoundFiles[i];
                var fInd = a.IndexOf('@');
                tm = "";
                for (int j = fInd+1; j < a.Length - 4; j++)
                {
                    tm += a[j];
                }
                array[i, 0] = Convert.ToInt32(tm);
                array[i, 1] = i;

            }
            //Сортировка файлов
            for (int i = 0; i < allFoundFiles.Length; i++)
            {
                for (int j = 0; j < allFoundFiles.Length - 1; j++)
                {
                    if (array[j, 0] > array[j + 1,0])
                    {
                        int z = array[j,0];
                        int y = array[j, 1];
                        array[j,0] = array[j + 1,0];
                        array[j,1] = array[j + 1,1];
                        array[j + 1,0] = z;
                        array[j + 1, 1] = y;
                    }
                }
            }
       

            for (int i = 0; i < allFoundFiles.Length; i++)
            {
                using (FileStream zipToOpen = new FileStream(allFoundFiles[array[i, 1]], FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        ZipArchiveEntry readmeEntry = archive.GetEntry(i + extension);

                        bytesCount += readmeEntry.Length;

                    }
                }
            }

            byte[] byteArray = new byte[bytesCount];

            //Читывает биты всех архивов
            for (int i = 0; i < allFoundFiles.Length; i++)
            {
                using (FileStream zipToOpen = new FileStream(allFoundFiles[array[i,1]], FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        ZipArchiveEntry readmeEntry = archive.GetEntry(i+extension);
                        //var length = readmeEntry.Length;
                        using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                        {
                            var _byte_counter = bytesCount;
                            while (_byte_counter > 0 && count < bytesCount)
                            {
                                _byte_counter--;
                                byteArray[count] = (byte)writer.BaseStream.ReadByte();
                                count++;

                            }


                        }
                    }
                }
            }
            int o = 0;
            // Создание файла из бит
            using (FileStream zipToOpen = new FileStream(path+ "\\" + flname + ".zip", FileMode.OpenOrCreate))
            {
                using (ZipArchive zip = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry readmeEntry = zip.CreateEntry(flname + extension);
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        foreach (var item in byteArray)
                        {
                            writer.BaseStream.WriteByte(item);
                            
                        }


                    }
                }
            }

            
        }
    }
}
