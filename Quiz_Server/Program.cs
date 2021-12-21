/*
 * FILE            : Program.cs
 * PROJECT         : Quiz_Server - Demo Day
 * PROGRAMMER     : Edward Boado
 * FIRST VERSION   : 2021 - 12 - 20
 * DESCRIPTION     : This file contains the Main method, which will instantiate a server, and use an async method
 *                   to allow the server to listen and run indefinitely.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            // Instantiate the server
            Server gameServer = Server.getServerInstance;

            // Call method to start async listening
            Listen();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }


        /*
        *	NAME	:	Listen
        *	PURPOSE	:	This asynchronous method will call the server's listener method, and run it in an asyncrhonous loop.
        *	INPUTS	:	None
        *	RETURNS	:	void Task
        */
        public static async void Listen()
        {
            // Always listen for requests
            while (true)
            {
                await Server.Listen();
            }
        }
    }
}