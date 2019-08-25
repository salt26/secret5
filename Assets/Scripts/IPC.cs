﻿using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

/// <summary> 
/// Used to show simple C# and Python interprocess communication 
/// Author      : Ozcan ILIKHAN 
/// Created     : 02/26/2015 
/// Last Update : 04/30/2015 
/// </summary> 
public class IPC
{
    public Process myProcess;
    public StreamReader myStreamReader;
    public StreamWriter myStreamWriter;

    public IPC()
    {
        StartPython();
    }

    public void StartPython()
    {
        // full path of python interpreter 
        string python = @"C:\Program Files\Python35\python.exe";

        // python app to call 
        string myPythonApp = "DQN.py";

        // Create new process start info 
        ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);

        // make sure we can read the output from stdout 
        myProcessStartInfo.UseShellExecute = false;
        myProcessStartInfo.RedirectStandardInput = true;
        myProcessStartInfo.RedirectStandardOutput = true;

        // start python app with 3 arguments  
        // 1st arguments is pointer to itself,  
        // 2nd and 3rd are actual arguments we want to send 
        myProcessStartInfo.Arguments = myPythonApp;

        myProcess = new Process();
        // assign start information to the process 
        myProcess.StartInfo = myProcessStartInfo;

        //Console.WriteLine("Calling Python script with arguments {0} and {1}", x, y);
        // start the process 
        myProcess.Start();

        myStreamReader = myProcess.StandardOutput;
        myStreamWriter = myProcess.StandardInput;

    }

    public string ReceiveRequest()
    {

        // Read the standard output of the app we called.  
        // in order to avoid deadlock we will read output first 
        // and then wait for process terminate: 
        string myString;
        do
        {
            myString = myStreamReader.ReadLine();

        } while (myString.Length > 1 && !myString.Substring(0, 1).Equals("#"));

        return myString;

        /*if you need to read multiple lines, you might use: 
            string myString = myStreamReader.ReadToEnd() */

        // write the output we got from python app 
        //Console.WriteLine("Value received from script: " + myString);
    }

    public void SendRequest(string message)
    {
        myStreamWriter.WriteLine(message);
    }

    public void EndPython()
    {
        // wait exit signal from the app we called and then close it. 
        myProcess.WaitForExit();
        myStreamWriter.Close();
        myStreamReader.Close();
        myProcess.Close();
    }
}