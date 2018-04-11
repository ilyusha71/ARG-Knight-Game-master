/**************************************************************************************** 
 * Wakaka Studio 2017
 * Author: iLYuSha Dawa-mumu Wakaka Kocmocovich Kocmocki KocmocA
 * Project: 0escape Medieval - Arduino Controller
 * Tools: Unity 5.6 + Arduino Mega2560
 * Version: Arduino Module v3.6c
 * Last Updated: 2017/11/28 - 更新資源釋放
 ****************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO.Ports;
using System.Threading;
using System;

public class ArduinoController : MonoBehaviour
{
    public static ArduinoController instance;
    public static SerialPort arduinoSerialPort;
    private Thread myThread;
    [HideInInspector]
    public bool connectAruidnoCompleted;
    private bool threadBuffer; // 執行緒結束緩衝
    private bool stop; // 重連Flag
    private int resetTimes;

    [Header("Arduino Setting")]
    public GameObject panelSetting;
    public GameObject groupPort;
    public GameObject groupBaud;
    private bool initializeCheck;
    Toggle[] port;
    Toggle[] baud;
    string serialPort = "COM9";
    int serialBaud = 9600;
    int[] valueBaud = new int[] { 9600, 4800, 9600, 19200, 38400 };

    [Header("Messages")]
    public MessageBox msgBox;
    public static string realtimeMsg;
    public static Queue<string> msgQueue = new Queue<string>();
    static string arduinoMsg;
    static string[] commands;

    [Header("Project Setting")]
    [SerializeField]
    public static int commandsCount;
    [SerializeField]
    public static bool msgQueueCombine = false; // 訊息處理合併

    [Header("Synchronize")]
    public Text textTime;
    static bool synchronize; // Arduino-Unity同步確認
    static int bootTime, receiveTime, unloadTime;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(this);

        int sceneIndex = PlayerPrefs.GetInt("lastScene");
        if (sceneIndex >= 100)
            SceneManager.LoadScene(PlayerPrefs.GetInt("lastScene") - 100);

        int com = PlayerPrefs.GetInt("lastPort");
        int rate = PlayerPrefs.GetInt("lastBaud");
        if (com != 0)
            initializeCheck = true;

        port =  groupPort.GetComponentsInChildren<Toggle>();
        port[com].isOn = true;
        serialPort = "COM" + com;
        baud = groupBaud.GetComponentsInChildren<Toggle>();
        baud[rate].isOn = true;
        serialBaud = valueBaud[rate];
        unloadTime = 600;
    }

    public void ChangePort(int com)
    {
        if (port[com].isOn)
        {
            serialPort = "COM" + com;
            PlayerPrefs.SetInt("lastPort", com);
            msgBox.Keyword("<color=lime>已重設Serial Port為</color>" + serialPort);
        }
    }

    public void ChangeBaud(int rate)
    {
        if (baud[rate].isOn)
        {
            serialBaud = valueBaud[rate];
            PlayerPrefs.SetInt("lastBaud", rate);
            msgBox.Keyword("<color=lime>已重設Serial Baud為</color>" + serialBaud);
        }
    }

    void Start()
    {
        panelSetting.SetActive(!initializeCheck);
        Cursor.visible = panelSetting.activeSelf;

        if (initializeCheck)
            ConnectArduino();
    }

    private void Update()
    {
        // 重新啟動遊戲
        if (Input.GetKey(KeyCode.Escape))
            resetTimes++;
        if (Input.GetKeyUp(KeyCode.Escape))
            resetTimes = 0;
        if (resetTimes > 150)
        {
            if (connectAruidnoCompleted)
                arduinoSerialPort.Write("R");
            msgBox.Keyword("<color=lime>遊戲重新啟動</color>");
            resetTimes = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (Time.time > unloadTime)
        {
            unloadTime += 60;
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.F10)|| Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            panelSetting.SetActive(!panelSetting.activeSelf);
            Cursor.visible = panelSetting.activeSelf;
        }
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.DeleteAll();
            msgBox.Keyword("<color=lime>已刪除設定檔</color>");
        }

        /* 若只有ArduinoController時使用*/
        if (!msgQueueCombine)
        {
            for (int i = 0; i < msgQueue.Count; i++)
            {
                ArduinoMsg(msgQueue.Dequeue());
            }
        }
        if (synchronize)
        {
            receiveTime = (int)Time.time;
            stop = false;
            synchronize = false;
        }
        textTime.text = bootTime + "\n" + receiveTime + "\n" + (int)Time.time;
    }
    private void LateUpdate()
    {
        //if (Time.time - receiveTime > 60 && !stop)
        //    BreakArduino();
    }
    public void ArduinoMsg(string msg)
    {
        msgBox.AddNewMsg(msg.Replace("Wakaka/", ""));
    }
    
    #region Arduino
    public void ConnectArduino()
    {
        arduinoSerialPort = new SerialPort(serialPort, serialBaud);
        try
        {
            arduinoSerialPort.Open();
            connectAruidnoCompleted = true;
            myThread = new Thread(new ThreadStart(GetArduino));
            myThread.Start();
            bootTime = (int)Time.time;
            textTime.text = bootTime + "\n---\n"+(int)Time.time;
            arduinoSerialPort.WriteLine("R");
            msgBox.Keyword("<color=lime>已開始接受訊號</color>");
        }
        catch (System.Exception ex)
        {
            Debug.Log("Start Error : " + ex);
        }
    }
    private void GetArduino()
    {        
        while (myThread.IsAlive && !threadBuffer)
        {
            if (arduinoSerialPort.IsOpen)
            {
                try
                {
                    arduinoMsg = arduinoSerialPort.ReadLine();
                   // bool emptyMsg = true;
                    if (string.IsNullOrEmpty(arduinoMsg))
                        Debug.Log("empty");
                    msgQueue.Enqueue(arduinoMsg);
                    //commands = arduinoMsg.Split('/');
                    //int countCommands = commands.Length;
                    //if (countCommands == commandsCount)
                    //{
                    //    if (commands[0] == "Wakaka" || commands[0] == "Reset")
                    //    {
                    //        synchronize = true;
                    //        for (int i = 1; i < countCommands; i++)
                    //        {
                    //            if (commands[i] != "")
                    //                emptyMsg = false;
                    //        }
                    //        if (!emptyMsg)
                    //            msgQueue.Enqueue(arduinoMsg);
                    //    }
                    //}
                }
                catch (System.Exception ex)
                {
                    Debug.Log("Run Error : " + ex);
                }
            }
        }
    }

    public void Quit()
    {
        if (connectAruidnoCompleted)
        {
            if (myThread.IsAlive)
            {
                threadBuffer = true;
                arduinoSerialPort.Close();
                Thread.Sleep(1000);
                myThread.Abort();
                Debug.Log("Thread isAlive ? " + myThread.IsAlive);
                connectAruidnoCompleted = false;
            }
            else
            {
                Debug.Log("Aborting thread failed");
            }
        }
    }
    void OnApplicationQuit()
    {
        Quit();
    }

    public void BreakArduino()
    {
        stop = true;
        Quit();
        msgBox.Keyword("<color=lime>已與Arduino斷開，請等待5秒重新連結</color>");

        StartCoroutine(Restart());
    }

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(1.0f);
        threadBuffer = false;
        yield return new WaitForSeconds(4.0f);
        ConnectArduino();
    }
    #endregion
}