﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//---------------------------------------------------------------
// Network
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading; // ManualResetEvent
using System.Runtime.InteropServices; // sizeof

using System;
using System.ComponentModel;
using System.Linq;
using FlatBuffers;
using Game.TheLastOne; // Client, Vec3 을 불러오기 위해
using TheLastOne.SendFunction;
//---------------------------------------------------------------

public struct Client_Data
{
    public int id;
    public Vector3 position;  // 클라이언트 위치
    public Vector3 rotation;    // 클라이언트 보는 방향
    public bool prefab;    // 클라이언트 프리팹이 만들어졌는지 확인
    public bool connect;    // 클라이언트 접속
    public string name;     // 클라이언트 닉네임
    public GameObject Player;   // 프리팹을 위한 게임 오브젝트
    public OtherPlayerCtrl script;  // 프리팹 오브젝트 안의 함수를 호출하기 위한 스크립트

    public Client_Data(Vector3 position, Vector3 rotation)
    {
        this.id = -1;
        this.position = position;
        this.rotation = rotation;
        this.connect = false;
        this.prefab = false;
        this.Player = null;
        this.name = "";
        this.script = null;
    }
}

namespace TheLastOne.Game.Network
{
    public class NetworkCtrl : MonoBehaviour
    {
        public static Socket m_Socket;
        public GameObject Player;
        public GameObject PrefabPlayer;

        Vector3 Player_Position;
        Vector3 Player_Rotation;

        public string iPAdress = "127.0.0.1";
        public const int kPort = 9000;

        private const int MaxClient = 50;    // 최대 동접자수
        public static Client_Data[] client_data = new Client_Data[MaxClient];      // 클라이언트 데이터 저장할 구조체
        private static int Client_imei = 0;         // 자신의 클라이언트 아이디

        Socket_SendFunction sF = new Socket_SendFunction();

        private int LimitReceivebyte = 4000;                     // Receive Data Length. (byte)
        private byte[] Receivebyte = new byte[4000];    // Receive data by this array to save.
        private byte[] Sendbyte = new byte[4000];

        // 서버가 클라이언트에게 보내는 이벤트 타입
        private int SC_ID = 1;                          // 클라이언트 아이디를 보낸다.
        private int SC_PUT_PLAYER = 2;            // 클라이언트 추가
        private int SC_REMOVE_PLAYER = 3;     // 클라이언트 삭제
        private int SC_Client_Data = 4;             // 클라이언트 모든 데이터


        IEnumerator startPrefab()
        {
            do
            {
                for (int i = 0; i < MaxClient; ++i)
                {
                    if (Client_imei == i)
                        continue;
                    if (client_data[i].connect == true && client_data[i].prefab == false)
                    {
                        client_data[i].Player = Instantiate(PrefabPlayer, client_data[i].position, Quaternion.identity);
                        //client_data[i].script = GameObject.Find("OtherPlayerCtrl").GetComponent<OtherPlayerCtrl>();
                        client_data[i].script = client_data[i].Player.GetComponent<OtherPlayerCtrl>();
                        client_data[i].prefab = true;

                        // 처음 위치를 넣어 줘야 한다. 그러지 않을경우 다른 클라이언트 에서는 0,0 에서부터 천천히 올라오게 보인다.
                        client_data[i].Player.transform.position = client_data[i].position;
                    }
                    else if (client_data[i].prefab == true)
                    {
                        // 실제로 캐릭터를 움직이는 것은 코루틴 여기서 움직임을 진행 한다.
                        client_data[i].script.MovePos(client_data[i].position);

                        var rotationX = client_data[i].rotation.x;
                        var rotationY = client_data[i].rotation.y;
                        var rotationZ = client_data[i].rotation.z;
                        //Debug.Log("  :  "+ rotationX + ", " + rotationY + ", " + rotationZ);
                        client_data[i].Player.transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
                    }
                }

                yield return null;
            } while (true);
            yield return null;
        }

        IEnumerator RecvCoroutine()
        {
            do
            {
                m_Socket.BeginReceive(Receivebyte, 0, LimitReceivebyte, 0, DataReceived, m_Socket);
                yield return null;
            } while (true);
            yield return null;
        }

        void ProcessPacket(int size, int type, byte[] recvPacket)
        {
            if (type == SC_ID)
            {
                // 클라이언트 아이디를 가져온다.
                byte[] t_buf = new byte[size + 1];
                System.Buffer.BlockCopy(recvPacket, 8, t_buf, 0, size); // 사이즈를 제외한 실제 패킷값을 복사한다.
                ByteBuffer revc_buf = new ByteBuffer(t_buf); // ByteBuffer로 byte[]로 복사한다.
                var Get_ServerData = Client_id.GetRootAsClient_id(revc_buf);
                Client_imei = Get_ServerData.Id;
                Debug.Log("클라이언트 아이디 : " + Client_imei);
            }
            else if (type == SC_PUT_PLAYER)
            {
                // 클라이언트 하나에 대한 데이터가 들어온다.
                byte[] t_buf = new byte[size + 1];
                System.Buffer.BlockCopy(recvPacket, 8, t_buf, 0, size); // 사이즈를 제외한 실제 패킷값을 복사한다.
                ByteBuffer revc_buf = new ByteBuffer(t_buf); // ByteBuffer로 byte[]로 복사한다.

                var data = new Offset<Client_info>[MaxClient];
                var Get_ServerData = Client_info.GetRootAsClient_info(revc_buf);

                // 클라이언트 데이터에 서버에서 받은 데이터를 넣어준다.
                client_data[Get_ServerData.Id].position = new Vector3(Get_ServerData.Position.Value.X, Get_ServerData.Position.Value.Y, Get_ServerData.Position.Value.Z);
                client_data[Get_ServerData.Id].rotation = new Vector3(Get_ServerData.Rotation.Value.X, Get_ServerData.Rotation.Value.Y, Get_ServerData.Rotation.Value.Z);
                client_data[Get_ServerData.Id].name = Get_ServerData.Name;

                if (client_data[Get_ServerData.Id].connect != true && Client_imei != Get_ServerData.Id)
                {
                    // 클라이언트가 처음 들어와서 프리팹이 없을경우 
                    client_data[Get_ServerData.Id].connect = true;
                }
            }
            else if (type == SC_Client_Data)
            {
                // 클라이언트 모든 데이터가 들어온다.
                byte[] t_buf = new byte[size + 1];
                System.Buffer.BlockCopy(recvPacket, 8, t_buf, 0, size); // 사이즈를 제외한 실제 패킷값을 복사한다.
                ByteBuffer revc_buf = new ByteBuffer(t_buf); // ByteBuffer로 byte[]로 복사한다.
                var Get_ServerData = All_information.GetRootAsAll_information(revc_buf);

                // 서버에서 받은 데이터 묶음을 확인하여 묶음 수 만큼 추가해준다.
                for (int i = 0; i < Get_ServerData.DataLength; i++)
                {
                    // 클라이언트 데이터에 서버에서 받은 데이터를 넣어준다.
                    var client_id = Get_ServerData.Data(i).Value.Id;
                    var position_x = Get_ServerData.Data(i).Value.Position.Value.X;
                    var position_y = Get_ServerData.Data(i).Value.Position.Value.Y;
                    var position_z = Get_ServerData.Data(i).Value.Position.Value.Z;
                    client_data[client_id].position = new Vector3(position_x, position_y, position_z);
                    // 캐릭터 이동 속도 변수

                    var rotation_x = Get_ServerData.Data(i).Value.Rotation.Value.X;
                    var rotation_y = Get_ServerData.Data(i).Value.Rotation.Value.Y;
                    var rotation_z = Get_ServerData.Data(i).Value.Rotation.Value.Z;
                    client_data[client_id].rotation = new Vector3(rotation_x, rotation_y, rotation_z);

                    client_data[client_id].name = Get_ServerData.Data(i).Value.Name;

                    if (Get_ServerData.Data(i).Value.Shotting == true && i != Client_imei)
                    {
                        // 자신이 아닌 다른 클라이언트가 총을 쏘면 해당 클라이언트의 script에 Fire을 호출한다.
                        //Debug.Log(client_id + "가 총을 쏘다.");
                        client_data[client_id].script.Fire();
                    }

                    if (client_data[client_id].connect != true && Client_imei != client_id)
                    {
                        // 클라이언트가 처음 들어와서 프리팹이 없을경우 
                        client_data[client_id].connect = true;
                    }

                }
            }



        }

        void Awake()
        {
            Application.runInBackground = true; // 백그라운드에서도 Network는 작동해야한다.
            //=======================================================
            // Socket create.
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 10000);
            m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);

            //=======================================================
            // Socket connect.
            try
            {
                IPAddress ipAddr = System.Net.IPAddress.Parse(iPAdress);
                IPEndPoint ipEndPoint = new System.Net.IPEndPoint(ipAddr, kPort);
                m_Socket.Connect(ipEndPoint);
                m_Socket.BeginReceive(Receivebyte, 0, LimitReceivebyte, 0, DataReceived, m_Socket);
                StartCoroutine(startPrefab());
                StartCoroutine(RecvCoroutine());
            }
            catch (SocketException SCE)
            {
                Debug.Log("Socket connect error! : " + SCE.ToString());
                return;
            }

            //=======================================================
        }

        void DataReceived(IAsyncResult ar)
        {
            //-------------------------------------------------------------------------------------
            /*
             C++ itoa를 통한 char로 넣은것을 for문을 통하여 컨버팅 하여 가져온다.
             124는 C++에서 '|'값 이다.
             str_size로 실제 패킷 값을 계산해서 넣는다.
             */
            string str_size = "";
            string tmp_int = "";
            byte[] temp = new byte[8];
            int type_Pos = 0;

            for (type_Pos = 0; type_Pos < 8; ++type_Pos)
            {
                if (Receivebyte[type_Pos] == 124)
                    break;
                temp[0] = Receivebyte[type_Pos];
                tmp_int = Encoding.Default.GetString(temp);
                str_size += Int32.Parse(tmp_int);
            }
            //-------------------------------------------------------------------------------------

            int psize = Int32.Parse(str_size);
            int ptype = Receivebyte[type_Pos + 1]; // 패킷 타입
            Debug.Log("총 사이즈 : " + psize + ", 패킷 타입 : " + ptype);

            if (psize == m_Socket.EndReceive(ar))
            {
                ProcessPacket(psize, ptype, Receivebyte);
            }
            else
            {
                Debug.Log(m_Socket.EndReceive(ar) + "패킷 Error | " + psize);
            }
        }

        public void Send_Packet(byte[] packet)
        {
            try
            {
                m_Socket.Send(packet, packet.Length, 0);
            }
            catch (SocketException err)
            {
                Debug.Log("Socket send or receive error! : " + err.ToString());
            }
        }

        public void Player_Shot()
        {
            Sendbyte = sF.makeShot_PacketInfo(Client_imei);
            Send_Packet(Sendbyte);
        }

        void OnApplicationQuit()
        {
            m_Socket.Close();
            m_Socket = null;
        }

        void Update()
        {
            Player_Position.x = Player.transform.position.x;
            Player_Position.y = Player.transform.position.y;
            Player_Position.z = Player.transform.position.z;
            Player_Rotation.x = Player.transform.eulerAngles.x;
            Player_Rotation.y = Player.transform.eulerAngles.y;
            Player_Rotation.z = Player.transform.eulerAngles.z;
            //Debug.Log(Player_Position.x + ", " + Player_Position.y + ", " + Player_Position.z);
            //Debug.Log(Player_Rotation.x + ", " + Player_Rotation.y + ", " + Player_Rotation.z);
            Sendbyte = sF.makeClient_PacketInfo(Player_Position, Player_Rotation);
            Send_Packet(Sendbyte);
        }


    }
}