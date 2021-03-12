using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class Client : MonoBehaviour {

    TcpClient client;
    StreamWriter sw;

    InputField inputField;
    InputField userID;
    GameObject textInstance;
    GameObject content;
    // Use this for initialization

    string newMessage = ",";

    private void Awake()
    {
        userID = GameObject.Find("ID").GetComponent<InputField>();
        inputField = GameObject.Find("InputField").GetComponent<InputField>();
        textInstance = Resources.Load<GameObject>("Text");
        content = GameObject.Find("Content");
    }

    void Start()
    {
        client = new TcpClient("127.0.0.1", 12340);
        Debug.Log("Connected to server.\n");

        Thread c_thread = new Thread(new ParameterizedThreadStart(Client_Read));
        c_thread.Start(client);

        sw = new StreamWriter(client.GetStream());

        StartCoroutine(NewMessageCreate());
    }

    IEnumerator NewMessageCreate()
    {
        for(; ; )
        {
            yield return null;
            if (newMessage[0] == '/')
            {
                Debug.Log(newMessage);
                GameObject newText = Instantiate(textInstance);
                newText.transform.parent = content.transform;
                newText.GetComponent<Text>().text = newMessage.Split('/')[1] + " > " + newMessage.Split('/')[2];
                newMessage = ",";
            }
            else if(newMessage[0] == '|')
            {
                GameObject newText = Instantiate(textInstance);
                newText.transform.parent = content.transform;
                newText.GetComponent<Text>().text = newMessage.Split('|')[1] + " > " + newMessage.Split('|')[2];
                newText.GetComponent<Text>().color = new Color(1,0,0,1);
                newMessage = ",";
            }
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(!inputField.isFocused)
            {
                inputField.ActivateInputField();
                inputField.Select();
            }
            if(client.Connected && inputField.text != "")
            {
                string input = "/" + userID.text + "/" + inputField.text;
                inputField.text = "";
                sw.WriteLine(input);
                sw.Flush();
            }
            inputField.ActivateInputField();
            inputField.Select(); 
        }

        if(userID.isFocused)
        {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                inputField.ActivateInputField();
                inputField.Select();
            }
        }
    }

    void Client_Read(object sender)
    {
        TcpClient readClient = sender as TcpClient;
        StreamReader readSr = new StreamReader(readClient.GetStream());

        try
        {
            while (true)
            {
                string message = readSr.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.Log(message);
                    newMessage = message;
                }
            }
        }
        catch (SocketException se)
        {
            Debug.Log(se.Message);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        finally
        {
            readSr.Close();
            readClient.Close();
        }
    }

    private void OnApplicationQuit()
    {
        sw.Close();
        client.Close();
        Debug.Log("서버연결해제");
    }

}
