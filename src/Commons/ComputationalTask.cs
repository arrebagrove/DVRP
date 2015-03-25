﻿using System;
using System.Threading.Tasks;
using _15pl04.Ucc.Commons.Messaging.Models;

namespace _15pl04.Ucc.Commons
{
    /// <summary>
    /// Represents a task running in ComputationalNode or TaskManager.
    /// Provides information needed to create StatusMessage.
    /// </summary>
    public class ComputationalTask
    {
        private StatusThreadState _state;
        private Task _task;


        public DateTime LastStateChange { get; private set; }

        public StatusThreadState State
        {
            get { return _state; }
            private set
            {
                if (_state == value)
                    return;
                _state = value;
                LastStateChange = DateTime.UtcNow;
            }
        }

        public Task Task
        {
            get { return _task; }
            set
            {
                if (_task == value)
                    return;
                _task = value;
                State = _task == null ? StatusThreadState.Idle : StatusThreadState.Busy;
            }
        }


        public ulong? ProblemInstanceId { get; set; }
        public ulong? PartialProblemId { get; set; }
        public string ProblemType { get; set; }



        public ComputationalTask() : this(null) { }
        public ComputationalTask(Task task)
        {
            Task = task;
            LastStateChange = DateTime.UtcNow;
        }
    }
}