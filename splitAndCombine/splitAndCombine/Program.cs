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
        static int buffer_size = 1024 * 1024; // 버퍼 사이즈

        static void Main(string[] args)
        {
            try
            {
                string filePath = "C:\\Users\\Surface\\Desktop\\";
                string fileName = "Distributed Event-Triggered Control for Multi-Agent Systems.pdf";
                //FileStream file = new FileStream(filePath + fileName, FileMode.OpenOrCreate);
                StreamReader fi = new StreamReader(filePath + fileName);

                string nFilePath = "C:\\Users\\Surface\\Desktop\\temp\\";
                string nFileName = "temp0";

                //파일을 분할하여 저장한다
                splitFile(nFilePath, nFileName, fi);

                //분할된 파일을 합친다
                combineFile(fileName, nFilePath);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        private static void splitFile(string nFilePath, string nFileName, StreamReader fi)
        {
            try
            {
                // int maxFileSize = 1024 * 1024 * 1024;
                int readCnt = 0; // 읽기 카운터
                int writeCnt = 0; // 쓰기 카운터
                // int totCnt = 0; // 전체 카운터
                int fileIdx = 0; // 파일 인덱스

                //BufferedStream bfi = new BufferedStream(file); 필요가 없는듯
                char[] readBuffer = new char[buffer_size];

                //FileStream nFile = new FileStream(nFilePath + nFileName, FileMode.OpenOrCreate);
                StreamWriter fo = new StreamWriter(nFilePath + nFileName);

                do
                {
                    readCnt = fi.Read(readBuffer, 0, buffer_size); // bfi를 사용하면 문자를 읽을수가 없다. 바이트만 가능
                    if (readCnt == 0)
                    {
                        break;
                    }

                    fo.Write(readBuffer, 0, readCnt);

                    writeCnt = readCnt;

                    if (readCnt == buffer_size)
                    {
                        fo.Flush();
                        fo.Dispose();

                        // FileStream nfile = new FileStream(nFilePath + nFileName + (++fileIdx) + "._tmp", FileMode.OpenOrCreate);
                        fo = new StreamWriter(nFilePath + nFileName + (++fileIdx) + "._tmp");
                    }
                } while (true);

                fi.Dispose();
                fo.Flush();
                fo.Dispose();
                System.Console.WriteLine("##########나누기완료##########");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }
        private static void combineFile(string oriFileName, string nFilePath)
        {

            // FileStream nFiles = new FileStream(nFilePath, FileMode.OpenOrCreate); // file open
            int fileCounter = 0;

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(nFilePath); // DirectoryInfo 객체 생성
            foreach (var item in di.GetFiles())
            {
                fileCounter++;
            }
            //List<string> files = new List<string>();
            string[] files = new string[fileCounter];//= nFiles.list(); // 해당 디렉토리에서 모든 파일이름 리스트를 string배열에 담는 부분인듯

            for (int i = 0; i < fileCounter; i++)
            {
                files = Directory.GetFiles(nFilePath);
            }

            StreamWriter nFo = new StreamWriter("C:\\Users\\Surface\\Desktop\\sum.pdf"); // file 읽기

            for (int i = 0; i < files.Length; i++)
            {

                StreamReader nFi = new StreamReader(files[i]);

                char[] buf = new char[buffer_size];
                int readCnt = 0;

                while ((readCnt = nFi.Read(buf, 0, buffer_size)) != 0) // offset을 4096으로 두고 문자 하나씩 Read
                {
                    nFo.Write(buf, 0, readCnt);
                }
            }

            nFo.Flush();
            nFo.Dispose();
            System.Console.WriteLine("##########합치기완료##########");
        }
    }
}