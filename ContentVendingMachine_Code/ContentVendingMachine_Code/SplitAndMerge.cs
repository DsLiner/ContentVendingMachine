using System;
using System.IO;

public class Arr
{
    public static void main(string[] args)
    {
        try
        {
            string filePath = "d:/";
            string fileName = "sp.pdf";
            FileStream file = new FileStream(filePath + fileName, FileMode.OpenOrCreate);
            StreamReader fi = new StreamReader(file);

            string nFilePath = "d:/bb/";
            string nFileName = "nfile0";

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
            int maxFileSize = 1024 * 1024 * 1;
            int readCnt = 0;
            int totCnt = 0;
            int fileIdx = 0;

            BufferedInputStream bfi = new BufferedInputStream(fi);
            Char[] readBuffer = new Char[2048];

            FileStream nFile = new FileStream(nFilePath + nFileName, FileMode.OpenOrCreate);
            StreamWriter fo = new StreamWriter(nFile);

            do
            {
                readCnt = bfi.read(readBuffer);
                if (readCnt == -1)
                {
                    break;
                }

                fo.Write(readBuffer, 0, readCnt);
                totCnt += readCnt;

                if (totCnt % maxFileSize == 0)
                {
                    fo.Flush();
                    fo.Dispose();

                    FileStream nfile = new FileStream(nFilePath + nFileName + (++fileIdx) + "._tmp", FileMode.OpenOrCreate);
                    fo = new StreamWriter(nfile);
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

        FileStream nFiles = new FileStream(nFilePath, FileMode.OpenOrCreate);

        string[] files = nFiles.list();

        StreamWriter nFo = new StreamWriter(nFilePath + oriFileName);

        for(int i = 0; i<files.Length;i++){

            StreamReader nFi = new StreamReader(nFilePath + files[i]);

            char[] buf = new char[2048];
            int readCnt = 0;

            while((readCnt =  nFi.Read(buf)) >-1){
                    nFo.Write(buf,0,readCnt);
            }
        }

        nFo.Flush();
        nFo.Dispose();
        System.Console.WriteLine("##########합치기완료##########");
    }
}