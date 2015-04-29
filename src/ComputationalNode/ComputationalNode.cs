﻿using System;
using System.Collections.Generic;
using System.Net;
using _15pl04.Ucc.Commons;
using _15pl04.Ucc.Commons.Computations;
using _15pl04.Ucc.Commons.Messaging.Models;
using _15pl04.Ucc.Commons.Messaging.Models.Base;
using UCCTaskSolver;

namespace _15pl04.Ucc.ComputationalNode
{
    public sealed class ComputationalNode : ComputationalComponent
    {
        public override ComponentType ComponentType
        {
            get { return ComponentType.ComputationalNode; }
        }


        /// <summary>
        /// Creates ComputationalNode which looks for task solvers in current directory.
        /// </summary>
        /// <param name="threadManager">The thread manager. Cannot be null.</param>
        /// <param name="serverAddress">The primary server address. Cannot be null.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ComputationalNode(ThreadManager threadManager, IPEndPoint serverAddress)
            : base(threadManager, serverAddress)
        {
        }

        /// <summary>
        /// Creates ComputationalNode.
        /// </summary>
        /// <param name="threadManager">The thread manager. Cannot be null.</param>
        /// <param name="serverAddress">The primary server address. Cannot be null.</param>
        /// <param name="taskSolversDirectoryRelativePath">The relative path to directory with task solvers.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        public ComputationalNode(ThreadManager threadManager, IPEndPoint serverAddress, string taskSolversDirectoryRelativePath)
            : base(threadManager, serverAddress, taskSolversDirectoryRelativePath)
        {
        }


        /// <summary>
        /// Handles any message received from server after registration process completes successfully.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <remarks>
        /// RegisterResponse is handled in base class.
        /// </remarks>
        protected override void HandleReceivedMessage(Message message)
        {
            switch (message.MessageType)
            {
                case MessageClass.NoOperation:
                    NoOperationMessageHandler((NoOperationMessage)message);
                    break;
                case MessageClass.SolvePartialProblems:
                    PartialProblemsMessageHandler((PartialProblemsMessage)message);
                    break;
                case MessageClass.Error:
                    ErrorMessageHandler((ErrorMessage)message);
                    break;
                default:
                    throw new InvalidOperationException("Received not supported message type.");
            }
        }

        private void NoOperationMessageHandler(NoOperationMessage message)
        {
            // nothing to do, backuping is handled by MessageSender
        }

        /// <exception cref="System.InvalidOperationException">Thrown when:
        /// - problem type can't be solved with this ComputationalNode,
        /// - received more partial problems than can be currently started.</exception>
        private void PartialProblemsMessageHandler(PartialProblemsMessage message)
        {
            if (!TaskSolvers.ContainsKey(message.ProblemType))
            {
                // shouldn't ever get here - received unsolvable problem
                throw new InvalidOperationException(string.Format("\"{0}\" problem type can't be solved with this ComputationalNode.", message.ProblemType));
            }
            var taskSolverType = TaskSolvers[message.ProblemType];
            var timeout = message.SolvingTimeout.HasValue ? TimeSpan.FromMilliseconds((double)message.SolvingTimeout.Value) : TimeSpan.MaxValue;
            foreach (var partialProblem in message.PartialProblems)
            {
                /* each partial problem should be started properly cause server sends at most 
                 * as many partial problems as count of component's tasks in idle state */
                bool started = ThreadManager.StartInNewThread(() =>
                {
                    // not sure if TaskSolver can change CommonData during computations so recreate it for each partial problem
                    var taskSolver = (TaskSolver)Activator.CreateInstance(taskSolverType, message.CommonData);

                    // measure time using DateTime cause StopWatch is not guaranteed to be thread safe
                    var start = DateTime.UtcNow;
                    var partialProblemSolutionData = taskSolver.Solve(partialProblem.Data, timeout);
                    var stop = DateTime.UtcNow;

                    var solutions = new List<SolutionsMessage.Solution>();
                    solutions.Add(new SolutionsMessage.Solution()
                    {
                        PartialProblemId = partialProblem.PartialProblemId,
                        TimeoutOccured = taskSolver.State == TaskSolver.TaskSolverState.Timeout,
                        Type = SolutionsMessage.SolutionType.Partial,
                        ComputationsTime = (ulong)(stop - start).TotalMilliseconds,
                        Data = partialProblemSolutionData,
                    });
                    var solutionsMessage = new SolutionsMessage()
                    {
                        ProblemType = message.ProblemType,
                        ProblemInstanceId = message.ProblemInstanceId,
                        CommonData = message.CommonData,
                        Solutions = solutions,
                    };

                    EnqueueMessageToSend(solutionsMessage);

                }, message.ProblemType, message.ProblemInstanceId, partialProblem.PartialProblemId);
                if (!started)
                {
                    // tragedy, CommunicationServer surprised us like the Spanish Inquisition
                    throw new InvalidOperationException("Received more partial problems than can be currently started.");
                }
            }
        }

        private void ErrorMessageHandler(ErrorMessage message)
        {
            switch (message.ErrorType)
            {
                case ErrorType.UnknownSender:
                    Register();
                    return;
                case ErrorType.InvalidOperation:
                case ErrorType.ExceptionOccured:
                    throw new NotImplementedException();
            }
        }
    }
}
