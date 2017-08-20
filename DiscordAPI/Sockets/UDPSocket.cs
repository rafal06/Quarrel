﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Discord_UWP.Sockets
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public object Message { get; set; }
    }

    public class UDPSocket
    {
        public event EventHandler<PacketReceivedEventArgs> MessageReceived;

        DatagramSocket _socket;
        DataWriter _dataWriter;

        public UDPSocket()
        {
            _socket = GetDatagramSocket();
            _dataWriter = GetDataWriter();
        }

        private DatagramSocket GetDatagramSocket()
        {
            var socket = new DatagramSocket();
            socket.MessageReceived += HandleMessage;
            return socket;
        }

        private DataWriter GetDataWriter()
        {
            return new DataWriter(_socket.OutputStream);
        }

        public async Task ConnectAsync(string connectionUrl, string port)
        {
            await _socket.ConnectAsync(new Windows.Networking.HostName(connectionUrl), port);
        }

        public async Task SendMessageAsync(string message)
        {
            _dataWriter.WriteString(message);
            await _dataWriter.StoreAsync();
        }

        public async Task SendDiscovery(int ssrc)
        {
            var packet = new byte[70];
            packet[0] = (byte)(ssrc >> 24);
            packet[1] = (byte)(ssrc >> 16);
            packet[2] = (byte)(ssrc >> 8);
            packet[3] = (byte)(ssrc >> 0);
            _dataWriter.WriteBytes(packet);
            await _dataWriter.StoreAsync();
        }

        private void HandleMessage(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs e)
        {
            using (var dataReader = e.GetDataReader())
            {
                dataReader.ByteOrder = ByteOrder.LittleEndian;
                byte[] fileContent = new byte[dataReader.UnconsumedBufferLength];
                dataReader.ReadBytes(fileContent); //TODO: Don't recieve sound as a string!!!
                OnMessageReceived(fileContent);
            }
        }

        private void OnMessageReceived(object message)
        {
            var messageReceivedEvent = new PacketReceivedEventArgs
            {
                Message = message
            };

            MessageReceived?.Invoke(this, messageReceivedEvent);
        }
    }
}
