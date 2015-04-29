﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using _15pl04.Ucc.Commons;
using _15pl04.Ucc.Commons.Computations;
using _15pl04.Ucc.Commons.Messaging;
using _15pl04.Ucc.Commons.Messaging.Models;
using _15pl04.Ucc.MinMaxTaskSolver;

namespace _15pl04.Ucc.ComputationalClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            var appSettings = ConfigurationManager.AppSettings;
            var primaryCSaddress = appSettings["primaryCSaddress"];
            var primaryCSport = appSettings["primaryCSport"];
            var serverAddress = IPEndPointParser.Parse(primaryCSaddress, primaryCSport);
            Console.WriteLine("server address from App.config: " + serverAddress);

            var computationalClient = new ComputationalClient(serverAddress);

            computationalClient.MessageSendingException += computationalClient_MessageSendingException;
            computationalClient.MessageReceived += computationalClient_MessageReceived;
            computationalClient.MessageSent += computationalClient_MessageSent;

            var problemType = "UCC.MinMax";

            string line;
            while ((line = Console.ReadLine()) != "exit")
            {
                // input handling
                if (line == "solve")
                {
                    var numbers = GenerateNumbers(10, 0, 50);
                    var minMaxProblem = new MMProblem(numbers);
                    var problemData = GenerateProblemData(minMaxProblem);
                    computationalClient.SendSolveRequest(problemType, problemData, null);

                }
                if (line == "solution")
                {
                    Console.Write("Enter problem id: ");
                    uint id;
                    uint.TryParse(Console.ReadLine(), out id);
                    var solutionsMessages = computationalClient.SendSolutionRequest(id);
                }
            }
        }

        static void computationalClient_MessageSent(object sender, MessageEventArgs e)
        {
            ColorfulConsole.WriteMessageInfo("Sent", e.Message);
        }

        static void computationalClient_MessageReceived(object sender, MessageEventArgs e)
        {
            ColorfulConsole.WriteMessageInfo("Received", e.Message);
        }

        static void computationalClient_MessageSendingException(object sender, MessageExceptionEventArgs e)
        {
            ColorfulConsole.WriteMessageExceptionInfo("Message sending exception", e.Message, e.Exception);
        }

        private static byte[] GenerateProblemData(MMProblem minMaxProblem)
        {
            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, minMaxProblem);
                var problemData = memoryStream.ToArray();
                return problemData;
            }
        }


        private static List<int> GenerateNumbers(int numbersCount, int min, int max)
        {
            var rand = new Random();
            var result = new List<int>();
            for (int i = 0; i < numbersCount; i++)
            {
                result.Add(rand.Next(min, max));
            }
            return result;
        }
    }
}
