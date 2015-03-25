﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using _15pl04.Ucc.Commons.Messaging.Models;

namespace _15pl04.Ucc.Commons.Messaging
{
    interface IMessageValidator
    {
        bool Validate(string xmlDocumentContent);

        bool Validate(XDocument xDocument);
    }
    /// <summary>
    /// Provides validating messages of <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">Type derived from Message.</typeparam>
    public class MessageValidatorHelper<T> : IMessageValidator
        where T : Message
    {
        private readonly XmlSchemaSet _xmlSchemaSet;

        public MessageValidatorHelper()
        {
            var xsdFileContent = Message.GetXsdFileContent(typeof(T));
            _xmlSchemaSet = new XmlSchemaSet();
            _xmlSchemaSet.Add(null, XmlReader.Create(new StringReader(xsdFileContent)));
        }

        /// <summary>
        /// Validates given <paramref name="xmlDocumentContent"/> with .xsd file corresponding to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="xmlDocumentContent">Content of XML document to validate.</param>
        /// <returns>True if content of document is valid; false otherwise.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public bool Validate(string xmlDocumentContent)
        {
            if (xmlDocumentContent == null)
            {
                throw new ArgumentNullException("xmlDocumentContent");
            }

            var xDocument = XDocument.Parse(xmlDocumentContent);
            var result = Validate(xDocument);
            return result;
        }

        /// <summary>
        /// Validates given <paramref name="xDocument"/> with .xsd file corresponding to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="xDocument">XDocument to validate.</param>
        /// <returns>True if document is valid; false otherwise.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public bool Validate(XDocument xDocument)
        {
            if (xDocument == null)
            {
                throw new ArgumentNullException("xDocument");
            }

            bool result;
            try
            {
                xDocument.Validate(_xmlSchemaSet, null);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
    }

    public static class MessageValidator
    {
        static readonly Dictionary<Message.MessageClassType, IMessageValidator> _messageValidatorForMessageTypeDictionary;

        static MessageValidator()
        {
            _messageValidatorForMessageTypeDictionary = new Dictionary<Message.MessageClassType, IMessageValidator>
                {
                    {Message.MessageClassType.NoOperation, new MessageValidatorHelper<NoOperationMessage>()},
                    {Message.MessageClassType.DivideProblem, new MessageValidatorHelper<DivideProblemMessage>()},
                    {Message.MessageClassType.Error, new MessageValidatorHelper<ErrorMessage>()},
                    {Message.MessageClassType.PartialProblems, new MessageValidatorHelper<PartialProblemsMessage>()},
                    {Message.MessageClassType.Register, new MessageValidatorHelper<RegisterMessage>()},
                    {Message.MessageClassType.RegisterResponse, new MessageValidatorHelper<RegisterResponseMessage>()},
                    {Message.MessageClassType.SolutionRequest, new MessageValidatorHelper<SolutionRequestMessage>()},
                    {Message.MessageClassType.Solutions, new MessageValidatorHelper<SolutionsMessage>()},
                    {Message.MessageClassType.SolveRequest, new MessageValidatorHelper<SolveRequestMessage>()},
                    {Message.MessageClassType.SolveRequestResponse, new MessageValidatorHelper<SolveRequestResponseMessage>()},
                    {Message.MessageClassType.Status, new MessageValidatorHelper<StatusMessage>()}
                };
        }
        static IMessageValidator GetValidatorForMessageClassType(Message.MessageClassType type)
        {
            return _messageValidatorForMessageTypeDictionary[type];
        }
        public static bool Validate(XDocument xDocument, Message.MessageClassType type)
        {
            return GetValidatorForMessageClassType(type).Validate(xDocument);
        }

        public static bool Validate(string xDocument, Message.MessageClassType type)
        {
            return GetValidatorForMessageClassType(type).Validate(xDocument);
        }
    }
}
