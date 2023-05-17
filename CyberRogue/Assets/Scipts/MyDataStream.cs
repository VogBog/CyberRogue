using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MyDataStream
{
    public enum MyDataStreamType { Open, Write }

    private List<string> allLines;
    private FileStream openedFile;
    private int curIndex = 0;
    private MyDataStreamType type;
    private bool isClosed = false;

    public MyDataStream(int SlotIndex, MyDataStreamType Type)
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = StaticSaveData.GetPath(SlotIndex);
        if(Type == MyDataStreamType.Write)
        {
            openedFile = File.OpenWrite(path);
            allLines = new List<string>();
        }
        else
        {
            openedFile = File.OpenRead(path);
            allLines = new List<string>(((string)bf.Deserialize(openedFile)).Split('\n'));
            openedFile.Close();
        }
        type = Type;
    }

    public void WriteLine(string line)
    {
        if(type == MyDataStreamType.Write && !isClosed)
            allLines.Add(line);
    }

    public void WriteLine(int num)
    {
        if (type == MyDataStreamType.Write && !isClosed)
            allLines.Add(num.ToString());
    }

    public void WriteLine(float num)
    {
        if (type == MyDataStreamType.Write && !isClosed)
            allLines.Add(num.ToString());
    }

    public void WriteLineObj(object obj)
    {
        if(type == MyDataStreamType.Write && !isClosed)
            allLines.Add(obj.ToString());
    }

    public static void OpenWriteAndClose(int index, string lines)
    {
        string path = StaticSaveData.GetPath(index);
        FileStream file = File.OpenWrite(path);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, lines);
        file.Close();
    }

    public string ReadLine()
    {
        if(type == MyDataStreamType.Open && curIndex < allLines.Count && !isClosed)
        {
            string res = allLines[curIndex];
            curIndex++;
            return res;
        }
        return "";
    }

    public void Close()
    {
        if(type == MyDataStreamType.Write && !isClosed)
        {
            string res = "";
            for(int i = 0; i < allLines.Count; i++)
            {
                res += allLines[i];
                if (i < allLines.Count - 1)
                    res += "\n";
            }
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(openedFile, res);
            openedFile.Close();
        }
        isClosed = true;
    }
}
