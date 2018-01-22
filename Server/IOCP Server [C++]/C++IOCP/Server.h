#ifndef __SERVER_H__
#define __SERVER_H__

#define _WINSOCK_DEPRECATED_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS

/* flatbuffers ������ min, max ���� �ذ� ��� */
#define _WIN32_WINNT _WIN32_WINNT_XP
#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
/* http://bspfp.pe.kr/archives/591 */

#pragma comment(lib, "ws2_32")

#include <winsock2.h>
#include <stdio.h>
#include <iostream>
#include <vector>
#include <thread>
#include <mutex>
#include <queue>
#include <unordered_set> // ������ �� ��������. [������ ����������]
#include <random>
#include <chrono>
#include <windows.h>


using namespace std::chrono;

//---------------------------------------------------------------------------------------------
// ���� ����
#define SERVERPORT 9000
#define BUFSIZE    1024
#define MAX_BUFF_SIZE   4000
#define MAX_PACKET_SIZE  255
#define MAX_Client 999
//---------------------------------------------------------------------------------------------
// ���� ����
#define DebugMod FALSE
#define Game_Width 300														// ����
#define Game_Height 300														// ����
#define VIEW_RADIUS   15														// Viwe �Ÿ�
#define NPC_RADIUS   10														// Viwe �Ÿ�
#define NPC_START  1000
#define MAX_NPC 1790
#define MAX_STR_SIZE  100
//---------------------------------------------------------------------------------------------

void err_quit( char *msg );
void err_display( char *msg, int err_no );

void init();
void Worker_Thread();
void Accept_Thread();
void Shutdown_Server();
void SendPutPlayerPacket( int client, int object );
void SendPacket( int cl, void *packet );
void DisconnectClient( int ci );
void ProcessPacket( int ci, char *packet );
void SendPositionPacket( int client, int object );
void SendRemovePlayerPacket( int client, int object );
void Send_Flat_Packet( int client, int object );


enum OPTYPE { OP_SEND, OP_RECV, OP_DO_AI, E_PLAYER_MOVE_NOTIFY, OP_Attack_Move, OP_Responder };
enum Event_Type { E_MOVE, E_Attack_Move, E_Responder };

struct OverlappedEx {
	WSAOVERLAPPED over;
	WSABUF wsabuf;
	unsigned char IOCP_buf[MAX_BUFF_SIZE];
	OPTYPE event_type;
	int target_id;
};

struct xyz {
	float x;
	float y;
	float z;
};

struct CLIENT {
	xyz client_xyz;
	xyz view;
	char game_id[10]; // Ŭ�󿡼� �޾ƿ� ���Ӿ��̵� ����
	int hp;
	bool connect;

	SOCKET client_socket;
	OverlappedEx recv_over;
	unsigned char packet_buf[MAX_PACKET_SIZE];
	int prev_packet_data; // ���� ó������ �ʴ� ��Ŷ�� �󸶳�
	int curr_packet_size; // ���� ó���ϰ� �ִ� ��Ŷ�� �󸶳�
	std::unordered_set<int> view_list; //�� set���� �ξ�������!
	std::mutex vl_lock;
};

extern CLIENT g_clients[MAX_NPC];


#endif