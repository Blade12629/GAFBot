using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GAFBot.API
{
    public class APIServerClient
    {
        private TcpClient _client;
        private NetworkStream _networkStream;
        private bool _stopped;
        private Task _readerTask;

        public event Action<byte[], APIServerClient> OnMessageRead;
        public event Action<byte[], APIServerClient> OnBeforeMessageSent;

        public Encoding Encoding { get; set; }
        public bool StopReading { get; set; }
        public bool Disposed { get; private set; }

        public APIServerClient(TcpClient client, Encoding encoding)
        {
            _client = client;
            _networkStream = _client.GetStream();
            Encoding = encoding;
        }

        public APIServerClient(TcpClient client) : this(client, Encoding.UTF8)
        {

        }

        public void ReadOnce()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            byte[] buffer = new byte[4096];
            int length = _networkStream.Read(buffer, 0, buffer.Length);

            if (length <= 0)
                return;

            Array.Resize(ref buffer, length);
            OnMessageRead?.Invoke(buffer, this);
        }

        public void Read()
        {
            _readerTask = Task.Run(() =>
            {
                if (Disposed)
                    throw new ObjectDisposedException(this.GetType().FullName);

                _stopped = false;

                while (!StopReading)
                    ReadOnce();

                _stopped = true;
                StopReading = false;
            });
        }

        public void Write(Packets.Packet p)
        {

        }

        public void Write(byte[] data)
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (data == null || data.Length == 0)
                return;

            OnBeforeMessageSent?.Invoke(data, this);

            _networkStream.Write(data);
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;
            StopReading = true;

            while (!_stopped)
                System.Threading.Tasks.Task.Delay(5).Wait();

            _networkStream.Dispose();
        }
    }
}
