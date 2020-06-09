using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class TextWriter : MonoBehaviour
{
    public string path = "";
    public string name = "";


    public void TextWrite(List<string> contents)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (StreamWriter file = new StreamWriter(Path.Combine(path, name + ".txt")))
            {
                foreach (string text in contents)
                {
                    file.WriteLine(text);
                }
            }
        }

        catch(System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void TextWrite(string contents)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (StreamWriter file = new StreamWriter(Path.Combine(path, name + ".txt")))
            {
                file.Write(contents);
            }
        }

        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
