﻿using System;
using System.Xml.Serialization;

namespace _15pl04.Ucc.Commons.Components
{
    /// <summary>
    /// Type of the cluster component.
    /// </summary>
    [Serializable]
    [XmlType("RegisterType", AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public enum ComponentType
    {
        CommunicationServer = 1,
        ComputationalClient,
        TaskManager,
        ComputationalNode
    }
}