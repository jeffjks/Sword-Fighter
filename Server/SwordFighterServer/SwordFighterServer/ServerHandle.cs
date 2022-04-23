using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace SwordFighterServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            Server.clients[fromClient].SendIntoGame(username);
        }

        public static void PlayerInput(int fromClient, Packet packet)
        {
            bool[] inputs = new bool[packet.ReadInt()];
            for (int i = 0; i < inputs.Length; ++i)
            {
                inputs[i] = packet.ReadBool();
            }

            if (Server.clients[fromClient].player != null)
            {
                Server.clients[fromClient].player.SetInput(inputs);
            }
        }

        public static void PlayerMovement(int fromClient, Packet packet)
        {
            Vector2 movement = packet.ReadVector2();
            Vector3 position = packet.ReadVector3();
            Vector3 direction = packet.ReadVector3();
            //Quaternion rotation = packet.ReadQuaternion();

            if (Server.clients[fromClient].player != null)
            {
                Server.clients[fromClient].player.SetMovement(movement, position, direction);
            }
        }

        public static void ChangeHp(int fromClient, Packet packet)
        {
            int hitPoints = packet.ReadInt();
            int targetPlayer = packet.ReadInt();

            if (Server.clients[targetPlayer].player != null)
            {
                Server.clients[targetPlayer].player.ChangePlayerHp(fromClient, hitPoints);
            }
        }
    }
}
