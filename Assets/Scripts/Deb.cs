using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Deb : MonoBehaviour
{
    public string pathToSent = "";
    public string pathToReceived = "";

    public void WriteSent(byte[] bytes)
    {
        try
        {
            File.WriteAllBytes(pathToSent.Replace("\"", ""), bytes);
        }
        catch (Exception) { }
    }
    public void WriteReceived(byte[] bytes)
    {
        try
        {
            File.WriteAllBytes(pathToReceived.Replace("\"", ""), bytes);
        }
        catch (Exception) { }
    }
}
