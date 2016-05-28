using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace splitAndCombine
{
    class Program
    {
        static int buffer_size = 1024 * 1024 * 10; // 버퍼 사이즈

        static string originalFilePath = @"C:\Users\Surface\Desktop\";
        static string originalFileName = "test.mp4";

        static string tempFilePath = @"C:\Users\Surface\Desktop\temp\";
        static string tempFileName = "temp";

        static string cloneFilePath = @"C:\Users\Surface\Desktop\";
        static string cloneFileName = "clone.mp4";

        static void Main(string[] args)
        {
            try
            {
                //FileStream file = new FileStream(filePath + fileName, FileMode.OpenOrCreate);
                Stream sr = new FileStream(originalFilePath + originalFileName, FileMode.Open, FileAccess.Read);

                //파일을 분할하여 저장한다
                splitFile(sr);

                //분할된 파일을 합친다
                combineFile();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        private static void splitFile(Stream sr)
        {
            try
            {
                // int maxFileSize = 1024 * 1024 * 1024;
                int readCnt = 0; // 읽기 카운터
                int writeCnt = 0; // 쓰기 카운터
                // int totCnt = 0; // 전체 카운터
                int fileIdx = 0; // 파일 인덱스

                //BufferedStream bfi = new BufferedStream(file); 필요가 없는듯
                byte[] buffer = null;
                Stream sw = null;

                //FileStream nFile = new FileStream(nFilePath + nFileName, FileMode.OpenOrCreate);

                do
                {
                    buffer = new byte[buffer_size];

                    readCnt = sr.Read(buffer, 0, buffer_size); // bfi를 사용하면 문자를 읽을수가 없다. 바이트만 가능
                    if (readCnt == 0)
                    {
                        break;
                    }

                    sw = new FileStream(tempFilePath + tempFileName + (fileIdx++) + "._tmp", FileMode.Create, FileAccess.Write);

                    sw.Write(buffer, 0, readCnt);
                    
                    sw.Flush();
                    sw.Dispose();

                    writeCnt = readCnt;
                } while (true);

                sr.Dispose();
                System.Console.WriteLine("##########나누기완료##########");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }
        private static void combineFile()
        {

            // FileStream nFiles = new FileStream(nFilePath, FileMode.OpenOrCreate); // file open
            int fileCounter = 0;
            byte[] buffer = null;
            Stream sr = null;

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(tempFilePath); // DirectoryInfo 객체 생성
            foreach (var item in di.GetFiles())
            {
                fileCounter++;
            }
            //List<string> files = new List<string>();
            string[] files = new string[fileCounter];//= nFiles.list(); // 해당 디렉토리에서 모든 파일이름 리스트를 string배열에 담는 부분인듯

            for (int i = 0; i < fileCounter; i++)
            {
                files = Directory.GetFiles(tempFilePath);
            }

            Stream sw = new FileStream(cloneFilePath + cloneFileName, FileMode.Create, FileAccess.Write); // file 읽기

            for (int i = 0; i < files.Length; i++)
            {
                sr = new FileStream(files[i], FileMode.Open, FileAccess.Read);

                buffer = new byte[buffer_size];
                int readCnt = 0;

                while ((readCnt = sr.Read(buffer, 0, buffer_size)) != 0) // offset을 4096으로 두고 문자 하나씩 Read
                {
                    sw.Write(buffer, 0, readCnt);
                }
            }

            sw.Flush();
            sw.Dispose();
            System.Console.WriteLine("##########합치기완료##########");
        }
    }
}