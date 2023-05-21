using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MyDataStream //Вот этот код единственный нормальный, пожалуйста, оцените хотя бы его
{
    public enum MyDataStreamType { Open, Write }

    private List<string> _allLines;
    private FileStream _openedFile;
    private int _curIndex = 0;
    private MyDataStreamType _type;
    private bool _isClosed = false;

    public MyDataStream(int SlotIndex, MyDataStreamType Type)
    {
        SetData(Type, SlotIndex);
    }

    private void SetData(MyDataStreamType Type, int SlotIndex)
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = StaticSaveData.GetPath(SlotIndex);

        if (Type == MyDataStreamType.Write)
        {
            _openedFile = File.OpenWrite(path);
            _allLines = new List<string>();
        }
        else
        {
            _openedFile = File.OpenRead(path);
            _allLines = new List<string>(((string)bf.Deserialize(_openedFile)).Split('\n'));
            _openedFile.Close();
        }
        _type = Type;
    }

    public void WriteLine(string line)
    {
        if(_type == MyDataStreamType.Write && !_isClosed)
            _allLines.Add(line);
    }

    public void WriteLine(int num)
    {
        if (_type == MyDataStreamType.Write && !_isClosed)
            _allLines.Add(num.ToString());
    }

    public void WriteLine(float num)
    {
        if (_type == MyDataStreamType.Write && !_isClosed)
            _allLines.Add(num.ToString());
    }

    public void WriteLineObj(object obj)
    {
        if(_type == MyDataStreamType.Write && !_isClosed)
            _allLines.Add(obj.ToString());
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
        if(_type == MyDataStreamType.Open && _curIndex < _allLines.Count && !_isClosed)
        {
            string res = _allLines[_curIndex];
            _curIndex++;
            return res;
        }
        return "";
    }

    public void Close()
    {
        if(_type == MyDataStreamType.Write && !_isClosed)
        {
            string res = "";
            for(int i = 0; i < _allLines.Count; i++)
            {
                res += _allLines[i];
                if (i < _allLines.Count - 1)
                    res += "\n";
            }
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(_openedFile, res);
            _openedFile.Close();
        }
        _isClosed = true;
    }
}
