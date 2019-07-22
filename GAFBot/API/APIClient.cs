using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace GAFBot.API
{
    public class APIClient
    {
        private TcpClient _client;
        private NetworkStream _nstream;
        private Task _listenerTask;
        private CancellationTokenSource _listenerTokenSource;
        private CancellationToken _listenerToken;

        public bool Disconnected { get; private set; }
        public bool Valid { get { return _client != null; } }
        public long SessionId { get; private set; }

        public event Action<long, byte[]> OnDataRecieved;
        public event Action<long> OnClientDisconnected;

        private byte[] _getNewBuffer { get { return new byte[_newBufferLength]; } }
        private const int _newBufferLength = 4096;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sessionId">DateTime.UtcNow.Ticks</param>
        public APIClient(TcpClient client, long sessionId)
        {
            SessionId = sessionId;
            _client = client;
            _nstream = client.GetStream();
        }
        
        public void StartReading()
        {
            _listenerTokenSource = new CancellationTokenSource();
            _listenerToken = _listenerTokenSource.Token;

            _listenerTask = Task.Run(async () =>
            {
                while (!Disconnected && _client.Connected)
                    await ReadNext();
            }, _listenerToken);
        }

        public async Task ReadNext()
        {
            if (Disconnected || !_client.Connected)
                return;

            byte[] buffer = _getNewBuffer;
            int length = -1;

            try
            {
                length = await _nstream.ReadAsync(buffer, 0, buffer.Length);
            }
            //This means our client has forcefully disconnected
            catch (IOException ex)
            { 
                Disconnect();
            }

            if (length <= 0)
                return;

            Array.Resize(ref buffer, length);
            await Task.Run(() => OnDataRecieved?.Invoke(SessionId, buffer));
        }

        public void Write(byte[] data)
        {
            if (Disconnected || !_client.Connected)
                return;

            try
            {
                _nstream.Write(data, 0, data.Length);
                _nstream.Flush();
            }
            //This means our client has forcefully disconnected
            catch (IOException ex)
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            _listenerTokenSource?.Cancel();

            if (_client.Connected)
                _client.Close();

            Disconnected = true;
            OnClientDisconnected?.Invoke(SessionId);
        }
    }
}
