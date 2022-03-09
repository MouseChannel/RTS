using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using KCPNET;
using Pb;
using System.Threading;
using System.Threading.Tasks;

public class NetService : Singleton<NetService>
{
     
    public static readonly string queue_lock = "queueLock";
    private static KCPNet<ClientSession> client;
    private Queue<PbMessage> messageQueue = null;
    public static Task<bool> checkTask;
 



    public uint Sid
    {
        get
        {
            if (client.session.IsConnected)
                return client.session.GetSid();
            return 0;
        }
    }
    public override void InitInstance()
    {
        string ip = "127.0.0.1";
        client = new KCPNet<ClientSession>();
        client.StartClient(ip, 7777);
        checkTask = client.ConnectServer(200, 5000);
        messageQueue = new Queue<PbMessage>();
        
        
    }
    public void Init()
    {




    }
    public void Update( )
    {
        

        ConnectServer();

        if (client != null && client.session != null)
        {

            if (messageQueue.Count > 0)
            {
                lock (queue_lock)
                {
                    PbMessage message = messageQueue.Dequeue();
                    HandoutMessage(message);
                }

            }
        }
    }




    public void AddMessageQueue(PbMessage message)
    {
        lock (queue_lock)
        {
            messageQueue.Enqueue(message);
        }

    }
    private static int counter = 0;
    void ConnectServer()
    {
        if (checkTask != null && checkTask.IsCompleted)
        {
            if (checkTask.Result)
            {
                checkTask = null;
            }
            else
            {
                ++counter;
                if (counter > 4)
                {
                    Debug.Log(string.Format("Connect Failed {0} Times,Check Your Network Connection.", counter));
                    checkTask = null;

                }
                else
                {
                    Debug.Log(string.Format("Connect Faild {0} Times.Retry...", counter));
                    checkTask = client.ConnectServer(200, 5000);
                }

            }


        }
    }






    /// <summary>
    /// 向服务器发送数据
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cb">callback</param>
    public void SendMessage(PbMessage message, Action<bool> cb = null)
    {
     
        if (client.session != null && client.session.IsConnected)
        {
            client.session.SendMessage(message);

            cb?.Invoke(true);
            Debug.Log("Send ,message  " + message.ToString());
        }
        else
        {
            GameRoot.Instance.ShowTips("服务器未连接");
            // this.Error("服务器未连接");
            // cb?.Invoke(false);
        }
    }


    private void HandoutMessage(PbMessage message)
    {

        switch (message.Cmd)
        {
            case PbMessage.Types.CMD.Login:
                LoginSystem.Instance.ResponseLogin(message);
                break;

            case PbMessage.Types.CMD.Match:
                LobbySystem.Instance.ResponseMatch(message);
                break;

            case PbMessage.Types.CMD.Room:
                LobbySystem.Instance.ResponseRoom(message);
                break;

            case PbMessage.Types.CMD.Fight:
                ResponseNetSystem.Instance.ResponseFightOp(message);
                break;
            case PbMessage.Types.CMD.Chat:
                ResponseNetSystem.Instance.ResponseFightOp(message);
                break;


            default:
                break;




        }
    }


}
