//-----------------------------------------------------------------------
// <copyright file="SerializationOptions.cs" company="Sirenix IVS">
// Copyright (c) 2018 Sirenix IVS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace PolymindGames.OdinSerializer
{
    /// <summary>
    /// Defines the configuration during serialization and deserialization. This class is thread-safe.
    /// </summary>
    public sealed class SerializationConfig
    {
        private readonly object m_Lock = new();
        private volatile ISerializationPolicy m_SerializationPolicy;
        private volatile DebugContext m_DebugContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationConfig"/> class.
        /// </summary>
        public SerializationConfig()
        {
            ResetToDefault();
        }

        /// <summary>
        /// <para>
        /// Setting this member to true indicates that in the case where, when expecting to deserialize an instance of a certain type, 
        /// but encountering an incompatible, uncastable type in the data being read, the serializer should attempt to deserialize an 
        /// instance of the expected type using the stored, possibly invalid data.
        /// </para>
        /// <para>
        /// This is equivalent to applying the <see cref="SerializationConfig.AllowDeserializeInvalidData"/> attribute, except global 
        /// instead of specific to a single type. Note that if this member is set to false, individual types may still be deserialized
        /// with invalid data if they are decorated with the <see cref="SerializationConfig.AllowDeserializeInvalidData"/> attribute.
        /// </para>
        /// </summary>
        public bool AllowDeserializeInvalidData;

        /// <summary>
        /// Gets or sets the serialization policy. This value is never null; if set to null, it will default to <see cref="SerializationPolicies.Unity"/>.
        /// </summary>
        /// <value>
        /// The serialization policy.
        /// </value>
        public ISerializationPolicy SerializationPolicy
        {
            get
            {
                if (m_SerializationPolicy == null)
                {
                    lock (m_Lock)
                    {
                        if (m_SerializationPolicy == null)
                        {
                            m_SerializationPolicy = SerializationPolicies.Unity;
                        }
                    }
                }

                return m_SerializationPolicy;
            }

            set
            {
                lock (m_Lock)
                {
                    m_SerializationPolicy = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the debug context. This value is never null; if set to null, a new default instance of <see cref="DebugContext"/> will be created upon the next get.
        /// </summary>
        /// <value>
        /// The debug context.
        /// </value>
        public DebugContext DebugContext
        {
            get
            {
                if (m_DebugContext == null)
                {
                    lock (m_Lock)
                    {
                        if (m_DebugContext == null)
                        {
                            m_DebugContext = new DebugContext();
                        }
                    }
                }

                return m_DebugContext;
            }

            set
            {
                lock (m_Lock)
                {
                    m_DebugContext = value;
                }
            }
        }

        /// <summary>
        /// Resets the configuration to a default configuration, as if the constructor had just been called.
        /// </summary>
        public void ResetToDefault()
        {
            lock (m_Lock)
            {
                AllowDeserializeInvalidData = false;
                m_SerializationPolicy = null;
                m_DebugContext?.ResetToDefault();
            }
        }
    }

    /// <summary>
    /// Defines a context for debugging and logging during serialization and deserialization. This class is thread-safe.
    /// </summary>
    public sealed class DebugContext
    {
        private readonly object m_Lock = new();

        private volatile ILogger m_Logger;
        private volatile LoggingPolicy m_LoggingPolicy;
        private volatile ErrorHandlingPolicy m_ErrorHandlingPolicy;

        /// <summary>
        /// The logger to use for logging messages.
        /// </summary>
        public ILogger Logger
        {
            get
            {
                if (m_Logger == null)
                {
                    lock (m_Lock)
                    {
                        if (m_Logger == null)
                        {
                            m_Logger = DefaultLoggers.UnityLogger;
                        }
                    }
                }

                return m_Logger;
            }
            set
            {
                lock (m_Lock)
                {
                    m_Logger = value;
                }
            }
        }

        /// <summary>
        /// The logging policy to use.
        /// </summary>
        public LoggingPolicy LoggingPolicy
        {
            get => m_LoggingPolicy;
            set => m_LoggingPolicy = value;
        }

        /// <summary>
        /// The error handling policy to use.
        /// </summary>
        public ErrorHandlingPolicy ErrorHandlingPolicy
        {
            get => m_ErrorHandlingPolicy;
            set => m_ErrorHandlingPolicy = value;
        }

        /// <summary>
        /// Log a warning. Depending on the logging policy and error handling policy, this message may be suppressed or result in an exception being thrown.
        /// </summary>
        public void LogWarning(string message)
        {
            if (m_ErrorHandlingPolicy == ErrorHandlingPolicy.ThrowOnWarningsAndErrors)
            {
                throw new SerializationAbortException("The following warning was logged during serialization or deserialization: " + (message ?? "EMPTY EXCEPTION MESSAGE"));
            }

            if (m_LoggingPolicy == LoggingPolicy.LogWarningsAndErrors)
            {
                Logger.LogWarning(message);
            }
        }

        /// <summary>
        /// Log an error. Depending on the logging policy and error handling policy, this message may be suppressed or result in an exception being thrown.
        /// </summary>
        public void LogError(string message)
        {
            if (m_ErrorHandlingPolicy != ErrorHandlingPolicy.Resilient)
            {
                throw new SerializationAbortException("The following error was logged during serialization or deserialization: " + (message ?? "EMPTY EXCEPTION MESSAGE"));
            }

            if (m_LoggingPolicy != LoggingPolicy.Silent)
            {
                Logger.LogError(message);
            }
        }

        /// <summary>
        /// Log an exception. Depending on the logging policy and error handling policy, this message may be suppressed or result in an exception being thrown.
        /// </summary>
        public void LogException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            // We must always rethrow abort exceptions
            if (exception is SerializationAbortException)
            {
                throw exception;
            }

            var policy = m_ErrorHandlingPolicy;

            if (policy != ErrorHandlingPolicy.Resilient)
            {
                throw new SerializationAbortException("An exception of type " + exception.GetType().Name + " occurred during serialization or deserialization.", exception);
            }

            if (m_LoggingPolicy != LoggingPolicy.Silent)
            {
                Logger.LogException(exception);
            }
        }

        public void ResetToDefault()
        {
            lock (m_Lock)
            {
                m_Logger = null;
                m_LoggingPolicy = default(LoggingPolicy);
                m_ErrorHandlingPolicy = default(ErrorHandlingPolicy);
            }
        }
    }
}