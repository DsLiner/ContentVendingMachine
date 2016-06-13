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
        static string originalFilePath = @"C:\Users\USER\Desktop\test\";
        static string originalFileName = "S04E01.mkv";

        static string tempFilePath = @"C:\Users\USER\Desktop\temp\";
        static string tempFilePath1 = @"C:\Users\USER\Desktop\temp\temp1\";
        static string tempFilePath2 = @"C:\Users\USER\Desktop\temp\temp2\";
        static string tempFilePath3 = @"C:\Users\USER\Desktop\temp\temp3\";
        static string tempFilePath4 = @"C:\Users\USER\Desktop\temp\temp4\";

        static string cloneFilePath = @"C:\Users\USER\Desktop\"; // 나중에 복사 파일도 temp안으로 저장되도록 수정하기
        static string cloneFileName = "S04E01.mkv";

        static string serverDirectory = @"C:\Users\USER\Desktop\server\";
        static string txtFileName = "FileList.txt";

        static FileInfo fInfo = new FileInfo(@"C:\Users\USER\Desktop\test\" + "S04E01.mkv");// 파일의 정보를 담는 객체
        static long fileSize = fInfo.Length; // 파일의 총 사이즈를 담는 변수

        static int buffer_size = ((int)fileSize / 20) + 1; // 버퍼 사이즈 = 파일 사이즈 / 20


        static void Main(string[] args)
        {
            try
            {
                Stream sr = new FileStream(originalFilePath + originalFileName, FileMode.Open, FileAccess.Read); // StreamReader

                if (System.IO.File.Exists(serverDirectory + txtFileName))
                {
                    System.IO.File.WriteAllText(serverDirectory + txtFileName, originalFileName);
                    System.Console.WriteLine("##########파일 생성 완료##########");
                }
                else
                {
                    System.IO.File.Create(serverDirectory + txtFileName);
                }

                splitFile(sr); //파일을 분할하여 저장한다

                distributeFile(); // 분할된 파일을 각 폴더에 분배한다

                combineFile(); //분배된 파일을 조합하여 합친다
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
                int readCnt = 0; // 읽기 카운터
                int fileIdx = 0; // 파일 인덱스

                string sDirPath = cloneFilePath + "temp"; // 폴더의 디렉토리를 저장할 변수
                DirectoryInfo di = new DirectoryInfo(sDirPath);

                if (di.Exists == false)   // 만약 폴더가 존재하지 않으면
                {
                    di.Create();             // 새 폴더를 생성
                    System.Console.WriteLine("##########폴더 생성 완료##########");
                }

                byte[] buffer = null; // 버퍼 선언
                Stream sw = null; // 스트림 선언

                do
                {

                    buffer = new byte[buffer_size]; // 버퍼 사이즈만큼 버퍼 할당

                    readCnt = sr.Read(buffer, 0, buffer_size); // 파일을 버퍼 사이즈만큼 읽음
                    if (readCnt == 0) // 파일의 끝에 다다르면
                    {
                        break; // 반복문 탈출
                    }

                    sw = new FileStream(tempFilePath + originalFileName + "_" + (++fileIdx), FileMode.Create, FileAccess.Write); // StreamWriter

                    sw.Write(buffer, 0, readCnt); // 분할될 파일에 데이터를 씀

                    sw.Flush();
                    sw.Dispose();

                } while (true);

                sr.Dispose();
                System.Console.WriteLine("##########파일 분할 완료##########");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }



        private static void combineFile()
        {

            int fileCounter = 0;
            byte[] buffer = null;
            Stream sr = null;

            string sDirPath = cloneFilePath + "temp"; // 폴더의 디렉토리를 저장할 변수
            DirectoryInfo dir = new DirectoryInfo(sDirPath);

            if (dir.Exists == false)   // 만약 폴더가 존재하지 않으면
            {
                dir.Create();             // 새 폴더를 생성
                System.Console.WriteLine("##########폴더 생성 완료##########");
            }

            for (int i = 1; i < 16; i++)
            {
                string sourceFile = tempFilePath1 + originalFileName + "_" + i;
                string destFile = tempFilePath + originalFileName + "_" + i;
                System.IO.File.Copy(sourceFile, destFile, true);
            }
            for (int i = 16; i < 21; i++)
            {
                string sourceFile = tempFilePath2 + originalFileName + "_" + i;
                string destFile = tempFilePath + originalFileName + "_" + i;
                System.IO.File.Copy(sourceFile, destFile, true);
            }

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(tempFilePath); // DirectoryInfo 객체 생성
            foreach (var item in di.GetFiles()) // 해당 디렉토리 내의 파일들 개수 만큼 count를 쌓는다
            {
                fileCounter++;
            }

            string[] files = new string[fileCounter]; // 파일 개수만큼 string 배열을 선언한다

            for (int i = 0; i < fileCounter; i++) // 해당 디렉토리에서 모든 파일이름 리스트를 string배열에 담는 부분
            {
                files = Directory.GetFiles(tempFilePath);
            }

            Stream sw = new FileStream(cloneFilePath + cloneFileName, FileMode.Create, FileAccess.Write); // file 읽기

            for (int i = 0; i < files.Length; i++)
            {
                sr = new FileStream(files[i], FileMode.Open, FileAccess.Read);

                buffer = new byte[buffer_size];
                int readCnt = 0;

                while ((readCnt = sr.Read(buffer, 0, buffer_size)) != 0) // 문자 하나씩 Read
                {
                    sw.Write(buffer, 0, readCnt);
                }
            }

            sw.Flush();
            sw.Dispose();
            System.IO.Directory.Delete(@"C:\Users\USER\Desktop\temp", true); // 임시 폴더 삭제
            System.Console.WriteLine("##########합치기완료##########");
        }



        private static void distributeFile()
        {

            for (int i = 1; i < 16; i++)
            {
                string sourceFile = tempFilePath + originalFileName + "_" + i;
                string destFile = tempFilePath1 + originalFileName + "_" + i;
                System.IO.File.Copy(sourceFile, destFile, true);
            }
            for (int i = 16; i < 31; i++)
            {
                if (i <= 20)
                {
                    string sourceFile = tempFilePath + originalFileName + "_" + i;
                    string destFile = tempFilePath2 + originalFileName + "_" + i;
                    System.IO.File.Copy(sourceFile, destFile, true);
                }
                else
                {
                    string sourceFile = tempFilePath + originalFileName + "_" + (i - 20);
                    string destFile = tempFilePath2 + originalFileName + "_" + (i - 20);
                    System.IO.File.Copy(sourceFile, destFile, true);
                }
            }
            for (int i = 11; i < 26; i++)
            {
                if (i <= 20)
                {
                    string sourceFile = tempFilePath + originalFileName + "_" + i;
                    string destFile = tempFilePath3 + originalFileName + "_" + i;
                    System.IO.File.Copy(sourceFile, destFile, true);
                }
                else
                {
                    string sourceFile = tempFilePath + originalFileName + "_" + (i - 20);
                    string destFile = tempFilePath3 + originalFileName + "_" + (i - 20);
                    System.IO.File.Copy(sourceFile, destFile, true);
                }
            }
            for (int i = 6; i < 21; i++)
            {
                string sourceFile = tempFilePath + originalFileName + "_" + i;
                string destFile = tempFilePath4 + originalFileName + "_" + i;
                System.IO.File.Copy(sourceFile, destFile, true);
            }

            System.IO.Directory.Delete(@"C:\Users\USER\Desktop\temp", true); // 임시 폴더 삭제
            System.Console.WriteLine("##########파일 분배 완료##########");
        }
    }
}