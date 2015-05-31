﻿using System;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace _15pl04.Ucc.Commons.Components
{
    /// <summary>
    /// Represents current state of a cluster component's thread.
    /// </summary>
    [Serializable]
    [DesignerCategory(@"code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public class ThreadStatus
    {
        /// <summary>
        /// Possible thread states.
        /// </summary>
        [Serializable]
        [XmlType(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
        public enum ThreadState
        {
            Idle,
            Busy
        }
        /// <summary>
        /// Current state of the thread.
        /// </summary>
        [XmlElement(Order = 0)]
        public ThreadState State { get; set; }
        /// <summary>
        /// Time this thread has been staying in the current state.
        /// </summary>
        [XmlElement(Order = 1, ElementName = "HowLong")]
        public ulong? TimeInThisState { get; set; }
        /// <summary>
        /// ID of the handled problem instance. Null in case of idle thread.
        /// </summary>
        [XmlElement(Order = 2)]
        public ulong? ProblemInstanceId { get; set; }
        /// <summary>
        /// ID of the currently processed partial problem (within a problem instance). Null if no partial problem is computed.
        /// </summary>
        [XmlElement(Order = 3, ElementName = "TaskId")]
        public ulong? PartialProblemId { get; set; } // PartialProblemId / ProblemInstanceId
        /// <summary>
        /// Type of the currently processed problem.
        /// </summary>
        [XmlElement(Order = 4)]
        public string ProblemType { get; set; }

        public bool ShouldSerializeTimeInThisState()
        {
            return TimeInThisState.HasValue;
        }

        public bool ShouldSerializeProblemInstanceId()
        {
            return ProblemInstanceId.HasValue;
        }

        public bool ShouldSerializePartialProblemId()
        {
            return PartialProblemId.HasValue;
        }

        public bool ShouldSerializeProblemType()
        {
            return ProblemType != null;
        }

        /// <summary>
        /// Gets string representation.
        /// </summary>
        /// <returns>String value that represents this object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("State(" + State + ")");

            if (TimeInThisState.HasValue)
                builder.Append(" HowLong(" + TimeInThisState + ")");

            if (ProblemInstanceId.HasValue)
                builder.Append(" ProblemInstanceId(" + ProblemInstanceId + ")");

            if (PartialProblemId.HasValue)
                builder.Append(" PartialProblemId(" + PartialProblemId + ")");

            if (ProblemType != null)
                builder.Append(" ProblemType(" + ProblemType + ")");

            return builder.ToString();
        }
    }
}