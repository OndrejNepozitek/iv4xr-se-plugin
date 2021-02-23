using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace Iv4xr.PluginLib
{
    public class PluginServer
    {
        private ILog m_log;
        private readonly ISessionDispatcher m_sessionDispatcher;
        private readonly RequestQueue m_requestQueue;

        public PluginServer(ILog log, ISessionDispatcher sessionDispatcher, RequestQueue requestQueue)
        {
            m_log = log;
            m_sessionDispatcher = sessionDispatcher;
            m_requestQueue = requestQueue;
        }

        public void SetLog(ILog log)
        {
            m_log = log;
        }

        private bool m_shouldStop;

        public void Start(bool waitForFinish = false)
        {
            var thread = new Thread(() =>
            {
                var listener = Listen();

                while (!m_shouldStop)
                {
                    Serve(listener);
                }

                m_log.WriteLine("Ivxr server loop ended");
            })
            {
                    IsBackground = true,
                    Name = "Ivrx plugin server thread"
            };
            thread.Start();

            if (waitForFinish)
                thread.Join();
        }

        public void Stop()
        {
            m_shouldStop = true;
        }

        private TcpListener Listen()
        {
            var listener = new TcpListener(IPAddress.Any, 9678);
            listener.Start();
            m_log.WriteLine($"Listening at {listener.LocalEndpoint}");

            return listener;
        }

        private void Serve(TcpListener listener)
        {
            using (var client = listener.AcceptTcpClient())
            {
                m_log.WriteLine("Connected.");

                using (var stream = client.GetStream())
                {
                    try
                    {
                        ServeConnectedClient(stream);
                    }
                    catch (IOException e)
                    {
                        m_log.WriteLine($"Plugin server connection error: {e.Message}");
                    }
                }
            }
        }

        private void ServeConnectedClient(NetworkStream stream)
        {
            var buffer = new byte[4096];

            int readCount;
            while ((readCount = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, readCount);
                int indexOfNewLine = message.IndexOf('\n');
                // TODO(PP): Change to a warning (after validating it does not actually occur).
                if ((indexOfNewLine != -1) && (indexOfNewLine != message.Length - 1))
                    throw new NotImplementedException("Unexpected new line in the middle of message.");

                // TODO(PP): Implement this.
                if (indexOfNewLine == -1)
                    throw new NotImplementedException("Reading message in multiple parts not implemented.");

                m_log.WriteLine($"Read message: {message}");

                ProcessMessage(stream, message, out bool disconnected);
                if (disconnected)
                    break;
            }
        }

        private void ProcessMessage(NetworkStream clientStream, string message, out bool disconnected)
        {
            disconnected = false;

            if (!message.StartsWith("{\"Cmd\":"))
            {
                // throw new InvalidDataException("Unexpected message header: " + message);
                m_log.WriteLine("Unexpected message header: " + message);
                // We disconnect here, because the outer loop can't handle a request without a reply from the queue.
                Disconnect(clientStream, out disconnected, reply: false);
                return;
            }

            string command = message.Substring(startIndex: 7, length: 12);

            // ReSharper disable once StringLiteralTypo
            if (command.StartsWith("\"AGENTCOM")) // AGENTCOMMAND 
            {
                m_requestQueue.Requests.Enqueue(new RequestItem(clientStream, message));

                // TODO(PP): This tends to block when the message is not added to the queue. Rearchitecture, or at least add a timeout.
                WaitForReplyAndSendIt();
            }
            else if (command.StartsWith("\"SESSION\""))
            {
                m_sessionDispatcher.ProcessRequest(new RequestItem(clientStream, message));
            }
            else if (command.StartsWith("\"DISCONNECT\""))
            {
                Disconnect(clientStream, out disconnected, reply: true);
            }
            else
            {
                // throw new NotImplementedException("Command unknown or not implemented: " + command);
                m_log.WriteLine("Command unknown or not implemented: " + command);
                // We disconnect here, because the outer loop can't handle a request without a reply from the queue.
                Disconnect(clientStream, out disconnected, reply: false);
            }
        }

        private void Disconnect(NetworkStream clientStream, out bool disconnected, bool reply = true)
        {
            Reply(clientStream, reply.ToString());
            clientStream.Close(timeout: 100); // ms
            disconnected = true;
        }

        private void WaitForReplyAndSendIt()
        {
            // TODO(PP): consider adding a timeout (blocks when no reply ready)
            var reply = m_requestQueue.Replies.Take();
            Reply(reply.ClientStream, reply.Message);
        }

        public static void Reply(NetworkStream clientStream, string reply)
        {
            // TODO(PP): avoid allocation of a new buffer each time
            var replyBuffer = Encoding.ASCII.GetBytes(reply + '\n');
            clientStream.Write(replyBuffer, 0, replyBuffer.Length);
        }

        public static void ReplyOK(NetworkStream clientStream)
        {
            Reply(clientStream, "true");
        }

        public static void ReplyFalse(NetworkStream clientStream)
        {
            Reply(clientStream, "false");
        }
    }
}