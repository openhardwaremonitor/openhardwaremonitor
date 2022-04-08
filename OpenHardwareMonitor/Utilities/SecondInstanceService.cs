using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace OpenHardwareMonitor.Utilities
{
    /// <summary>
    /// This class handles communication between two instances form the already running instance
    /// </summary>
    internal sealed class SecondInstanceService : IDisposable
    {
        private CancellationTokenSource m_cancellationToken;
        private Task m_secondInstanceTask;
        private NamedPipeServerStream m_serverPipe;

        public event Action<SecondInstanceRequest> OnSecondInstanceRequest;

        public void Run()
        {
            m_cancellationToken = new CancellationTokenSource();
            m_secondInstanceTask = new Task(HandleRequestAsync, m_cancellationToken.Token);
            m_secondInstanceTask.Start();
        }

        /// <summary>
        /// Waits for incoming connections reads the content and distributes it via event
        /// </summary>
        private async void HandleRequestAsync()
        {
            m_serverPipe = InterprocessCommunicationFactory.GetServerPipe();
            while (m_cancellationToken.IsCancellationRequested == false)
            {
                await m_serverPipe.WaitForConnectionAsync(m_cancellationToken.Token);
                while (m_serverPipe.IsConnected)
                {
                    using (var stream = new MemoryStream())
                    {
                        byte[] buffer = new byte[32];
                        int bytesRead;
                        while ((bytesRead = m_serverPipe.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, bytesRead);
                        }
                        byte[] result = stream.ToArray();
                        if(result.Length == 1)
                        {
                            SecondInstanceRequest request = (SecondInstanceRequest)result[0];
                            new Task(() => OnSecondInstanceRequest?.Invoke(request)).Start();
                        }
                    }
                }
                m_serverPipe.Disconnect();
            }
        }

        /// <summary>
        /// Cancels communication and disconnects from other instance if connected
        /// </summary>
        public void Cancel()
        {
            m_cancellationToken.Cancel();
            while (m_secondInstanceTask.IsCompleted == false);

            if (m_serverPipe.IsConnected)
            {
                m_serverPipe.Disconnect();
            }
            m_serverPipe.Dispose();

            m_serverPipe = null;
            m_secondInstanceTask = null;
            m_cancellationToken = null;
        }

        public void Dispose()
        {
            if(m_cancellationToken != null)
            {
                Cancel();
            }
        }

        public enum SecondInstanceRequest : byte
        {
            None = 0,
            MaximizeWindow = 1
        }
    }
}
