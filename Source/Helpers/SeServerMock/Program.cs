﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Iv4xr.PluginLib;
using Iv4xr.PluginLib.Log;
using Iv4xr.SePlugin.Session;
using Iv4xr.SePlugin.Control;
using SeServerMock.Mocks;

namespace SeServerMock
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class Program
    {
        public static void Main(string[] args)
        {
            var log = new ConsoleLog();
			var sessionController = new MockSessionController() { Log = log };
			var sessionDispatcher = new SessionDispatcher(sessionController) { Log = log };

            using (var requestQueue = new RequestQueue())
            {
	            var server = new PluginServer(log, sessionDispatcher, requestQueue);
                server.Start();

				var observer = new MockObserver() { Log = log };
				var controller = new MockCharacterController() { Log = log };

				var dispatcher = new Dispatcher(requestQueue, observer, controller) { Log = log };

				while (true)
                {
					dispatcher.ProcessRequests();

					/*
                    while (requestQueue.Requests.TryDequeue(out Request request))
                    {
                        log.WriteLine("dequeued: " + request.Message);

                        requestQueue.Replies.Add(
                            new Request(request.ClientStream, $"Got {request.Message.Length} bytes, thanks!"));
                    }
					*/

                    Thread.Sleep(5);
                }

                // server.Start(waitForFinish: true);
            }
        }
    }
}