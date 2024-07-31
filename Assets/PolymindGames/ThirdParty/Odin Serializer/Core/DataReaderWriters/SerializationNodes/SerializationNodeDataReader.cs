//-----------------------------------------------------------------------
// <copyright file="SerializationNodeDataReader.cs" company="Sirenix IVS">
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
namespace PolymindGames.OdinSerializer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public class SerializationNodeDataReader : BaseDataReader
    {
        private string peekedEntryName;
        private EntryType? peekedEntryType;
        private string peekedEntryData;

        private int currentIndex = -1;
        private List<SerializationNode> nodes;
        private Dictionary<Type, Delegate> primitiveTypeReaders;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public SerializationNodeDataReader(DeserializationContext context) : base(null, context)
        {
            primitiveTypeReaders = new Dictionary<Type, Delegate>()
        {
            { typeof(char), (Func<char>)(() => {
                ReadChar(out char v); return v; }) },
            { typeof(sbyte), (Func<sbyte>)(() => {
                ReadSByte(out sbyte v); return v; }) },
            { typeof(short), (Func<short>)(() => {
                ReadInt16(out short v); return v; }) },
            { typeof(int), (Func<int>)(() => {
                ReadInt32(out int v); return v; })  },
            { typeof(long), (Func<long>)(() => {
                ReadInt64(out long v); return v; })  },
            { typeof(byte), (Func<byte>)(() => {
                ReadByte(out byte v); return v; })  },
            { typeof(ushort), (Func<ushort>)(() => {
                ReadUInt16(out ushort v); return v; })  },
            { typeof(uint),   (Func<uint>)(() => {
                ReadUInt32(out uint v); return v; })  },
            { typeof(ulong),  (Func<ulong>)(() => {
                ReadUInt64(out ulong v); return v; })  },
            { typeof(decimal),   (Func<decimal>)(() => {
                ReadDecimal(out decimal v); return v; })  },
            { typeof(bool),  (Func<bool>)(() => {
                ReadBoolean(out bool v); return v; })  },
            { typeof(float),  (Func<float>)(() => {
                ReadSingle(out float v); return v; })  },
            { typeof(double),  (Func<double>)(() => {
                ReadDouble(out double v); return v; })  },
            { typeof(Guid),  (Func<Guid>)(() => {
                ReadGuid(out var v); return v; })  }
        };
        }

        private bool IndexIsValid => nodes != null && currentIndex >= 0 && currentIndex < nodes.Count;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public List<SerializationNode> Nodes
        {
            get
            {
                if (nodes == null)
                {
                    nodes = new List<SerializationNode>();
                }

                return nodes;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                nodes = value;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override Stream Stream
        {
            get => throw new NotSupportedException("This data reader has no stream.");
            set => throw new NotSupportedException("This data reader has no stream.");
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override void Dispose()
        {
            nodes = null;
            currentIndex = -1;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override void PrepareNewSerializationSession()
        {
            base.PrepareNewSerializationSession();
            currentIndex = -1;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override EntryType PeekEntry(out string name)
        {
            if (peekedEntryType != null)
            {
                name = peekedEntryName;
                return peekedEntryType.Value;
            }

            currentIndex++;

            if (IndexIsValid)
            {
                var node = nodes[currentIndex];

                peekedEntryName = node.Name;
                peekedEntryType = node.Entry;
                peekedEntryData = node.Data;
            }
            else
            {
                peekedEntryName = null;
                peekedEntryType = EntryType.EndOfStream;
                peekedEntryData = null;
            }

            name = peekedEntryName;
            return peekedEntryType.Value;
        }

        /// <summary>
        /// Tries to enters an array node. This will succeed if the next entry is an <see cref="EntryType.StartOfArray" />.
        /// <para />
        /// This call MUST (eventually) be followed by a corresponding call to <see cref="IDataReader.ExitArray(DeserializationContext)" /><para />
        /// This call will change the values of the <see cref="IDataReader.IsInArrayNode" />, <see cref="IDataReader.CurrentNodeName" />, <see cref="IDataReader.CurrentNodeId" /> and <see cref="IDataReader.CurrentNodeDepth" /> properties to the correct values for the current array node.
        /// </summary>
        /// <param name="length">The length of the array that was entered.</param>
        /// <returns>
        ///   <c>true</c> if an array was entered, otherwise <c>false</c>
        /// </returns>
        public override bool EnterArray(out long length)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.StartOfArray)
            {
                PushArray();

                if (!long.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out length))
                {
                    length = 0;
                    Context.Config.DebugContext.LogError("Failed to parse array length from data '" + peekedEntryData + "'.");
                }

                ConsumeCurrentEntry();
                return true;
            }
            else
            {
                SkipEntry();
                length = 0;
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool EnterNode(out Type type)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.StartOfNode)
            {
                string data = peekedEntryData;
                int id = -1;
                type = null;

                if (!string.IsNullOrEmpty(data))
                {
                    string typeName = null;
                    int separator = data.IndexOf(SerializationNodeDataReaderWriterConfig.NodeIdSeparator, StringComparison.InvariantCulture);
                    int parsedId;

                    if (separator >= 0)
                    {
                        typeName = data.Substring(separator + 1);

                        string idStr = data.Substring(0, separator);

                        if (int.TryParse(idStr, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedId))
                        {
                            id = parsedId;
                        }
                        else
                        {
                            Context.Config.DebugContext.LogError("Failed to parse id string '" + idStr + "' from data '" + data + "'.");
                        }
                    }
                    else if (int.TryParse(data, out parsedId))
                    {
                        id = parsedId;
                    }
                    else
                    {
                        typeName = data;
                    }

                    if (typeName != null)
                    {
                        type = Context.Binder.BindToType(typeName, Context.Config.DebugContext);
                    }
                }

                ConsumeCurrentEntry();
                PushNode(peekedEntryName, id, type);

                return true;
            }
            else
            {
                SkipEntry();
                type = null;
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ExitArray()
        {
            PeekEntry();

            // Read to next end of array
            while (peekedEntryType != EntryType.EndOfArray && peekedEntryType != EntryType.EndOfStream)
            {
                if (peekedEntryType == EntryType.EndOfNode)
                {
                    Context.Config.DebugContext.LogError("Data layout mismatch; skipping past node boundary when exiting array.");
                    ConsumeCurrentEntry();
                }

                SkipEntry();
            }

            if (peekedEntryType == EntryType.EndOfArray)
            {
                ConsumeCurrentEntry();
                PopArray();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ExitNode()
        {
            PeekEntry();

            // Read to next end of node
            while (peekedEntryType != EntryType.EndOfNode && peekedEntryType != EntryType.EndOfStream)
            {
                if (peekedEntryType == EntryType.EndOfArray)
                {
                    Context.Config.DebugContext.LogError("Data layout mismatch; skipping past array boundary when exiting node.");
                    ConsumeCurrentEntry();
                }

                SkipEntry();
            }

            if (peekedEntryType == EntryType.EndOfNode)
            {
                ConsumeCurrentEntry();
                PopNode(CurrentNodeName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadBoolean(out bool value)
        {
            PeekEntry();

            try
            {
                if (peekedEntryType == EntryType.Boolean)
                {
                    value = peekedEntryData == "true";
                    return true;
                }

                value = false;
                return false;
            }
            finally
            {
                ConsumeCurrentEntry();
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadByte(out byte value)
        {

            if (ReadUInt64(out ulong ulongValue))
            {
                checked
                {
                    try
                    {
                        value = (byte)ulongValue;
                    }
                    catch (OverflowException)
                    {
                        value = default(byte);
                    }
                }

                return true;
            }

            value = default(byte);
            return false;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadChar(out char value)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.String)
            {
                try
                {
                    if (peekedEntryData.Length == 1)
                    {
                        value = peekedEntryData[0];
                        return true;
                    }
                    else
                    {
                        Context.Config.DebugContext.LogWarning("Expected string of length 1 for char entry.");
                        value = default(char);
                        return false;
                    }
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                value = default(char);
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadDecimal(out decimal value)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.FloatingPoint || peekedEntryType == EntryType.Integer)
            {
                try
                {
                    if (!decimal.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    {
                        Context.Config.DebugContext.LogError("Failed to parse decimal value from entry data '" + peekedEntryData + "'.");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                value = default(decimal);
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadDouble(out double value)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.FloatingPoint || peekedEntryType == EntryType.Integer)
            {
                try
                {
                    if (!double.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    {
                        Context.Config.DebugContext.LogError("Failed to parse double value from entry data '" + peekedEntryData + "'.");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                value = default(double);
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadExternalReference(out Guid guid)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.ExternalReferenceByGuid)
            {
                try
                {
                    if ((guid = new Guid(peekedEntryData)) != Guid.Empty)
                    {
                        return true;
                    }

                    guid = Guid.Empty;
                    return false;
                }
                catch (FormatException)
                {
                    guid = Guid.Empty;
                    return false;
                }
                catch (OverflowException)
                {
                    guid = Guid.Empty;
                    return false;
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                guid = Guid.Empty;
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadExternalReference(out string id)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.ExternalReferenceByString)
            {
                id = peekedEntryData;
                ConsumeCurrentEntry();
                return true;
            }
            else
            {
                SkipEntry();
                id = null;
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadExternalReference(out int index)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.ExternalReferenceByIndex)
            {
                try
                {
                    if (!int.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out index))
                    {
                        Context.Config.DebugContext.LogError("Failed to parse external index reference integer value from entry data '" + peekedEntryData + "'.");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                index = default(int);
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadGuid(out Guid value)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.Guid)
            {
                try
                {
                    if ((value = new Guid(peekedEntryData)) != Guid.Empty)
                    {
                        return true;
                    }

                    value = Guid.Empty;
                    return false;
                }
                catch (FormatException)
                {
                    value = Guid.Empty;
                    return false;
                }
                catch (OverflowException)
                {
                    value = Guid.Empty;
                    return false;
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                value = Guid.Empty;
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadInt16(out short value)
        {

            if (ReadInt64(out long longValue))
            {
                checked
                {
                    try
                    {
                        value = (short)longValue;
                    }
                    catch (OverflowException)
                    {
                        value = default(short);
                    }
                }

                return true;
            }

            value = default(short);
            return false;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadInt32(out int value)
        {

            if (ReadInt64(out long longValue))
            {
                checked
                {
                    try
                    {
                        value = (int)longValue;
                    }
                    catch (OverflowException)
                    {
                        value = default(int);
                    }
                }

                return true;
            }

            value = default(int);
            return false;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadInt64(out long value)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.Integer)
            {
                try
                {
                    if (!long.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    {
                        Context.Config.DebugContext.LogError("Failed to parse integer value from entry data '" + peekedEntryData + "'.");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                value = default(long);
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadInternalReference(out int id)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.InternalReference)
            {
                try
                {
                    if (!int.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out id))
                    {
                        Context.Config.DebugContext.LogError("Failed to parse internal reference id integer value from entry data '" + peekedEntryData + "'.");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                id = default(int);
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadNull()
        {
            PeekEntry();

            if (peekedEntryType == EntryType.Null)
            {
                ConsumeCurrentEntry();
                return true;
            }
            else
            {
                SkipEntry();
                return false;
            }
        }

        /// <summary>
        /// Reads a primitive array value. This call will succeed if the next entry is an <see cref="EntryType.PrimitiveArray" />.
        /// <para />
        /// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="EntryType.EndOfNode" /> or an <see cref="EntryType.EndOfArray" />.
        /// </summary>
        /// <typeparam name="T">The element type of the primitive array. Valid element types can be determined using <see cref="FormatterUtilities.IsPrimitiveArrayType(Type)" />.</typeparam>
        /// <param name="array">The resulting primitive array.</param>
        /// <returns>
        ///   <c>true</c> if reading a primitive array succeeded, otherwise <c>false</c>
        /// </returns>
        /// <exception cref="System.ArgumentException">Type  + typeof(T).Name +  is not a valid primitive array type.</exception>
        public override bool ReadPrimitiveArray<T>(out T[] array)
        {
            if (FormatterUtilities.IsPrimitiveArrayType(typeof(T)) == false)
            {
                throw new ArgumentException("Type " + typeof(T).Name + " is not a valid primitive array type.");
            }

            if (peekedEntryType != EntryType.PrimitiveArray)
            {
                SkipEntry();
                array = null;
                return false;
            }

            if (typeof(T) == typeof(byte))
            {
                array = (T[])(object)ProperBitConverter.HexStringToBytes(peekedEntryData);
                return true;
            }
            else
            {
                PeekEntry();

                if (peekedEntryType != EntryType.PrimitiveArray)
                {
                    Context.Config.DebugContext.LogError("Expected entry of type '" + EntryType.StartOfArray + "' when reading primitive array but got entry of type '" + peekedEntryType + "'.");
                    SkipEntry();
                    array = Array.Empty<T>();
                    return false;
                }

                if (!long.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out long length))
                {
                    Context.Config.DebugContext.LogError("Failed to parse primitive array length from entry data '" + peekedEntryData + "'.");
                    SkipEntry();
                    array = Array.Empty<T>();
                    return false;
                }

                ConsumeCurrentEntry();
                PushArray();

                array = new T[length];

                Func<T> reader = (Func<T>)primitiveTypeReaders[typeof(T)];

                for (int i = 0; i < length; i++)
                {
                    array[i] = reader();
                }

                ExitArray();
                return true;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadSByte(out sbyte value)
        {

            if (ReadInt64(out long longValue))
            {
                checked
                {
                    try
                    {
                        value = (sbyte)longValue;
                    }
                    catch (OverflowException)
                    {
                        value = default(sbyte);
                    }
                }

                return true;
            }

            value = default(sbyte);
            return false;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadSingle(out float value)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.FloatingPoint || peekedEntryType == EntryType.Integer)
            {
                try
                {
                    if (!float.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    {
                        Context.Config.DebugContext.LogError("Failed to parse float value from entry data '" + peekedEntryData + "'.");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                value = default(float);
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadString(out string value)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.String)
            {
                value = peekedEntryData;
                ConsumeCurrentEntry();
                return true;
            }
            else
            {
                SkipEntry();
                value = default(string);
                return false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadUInt16(out ushort value)
        {

            if (ReadUInt64(out ulong ulongValue))
            {
                checked
                {
                    try
                    {
                        value = (ushort)ulongValue;
                    }
                    catch (OverflowException)
                    {
                        value = default(ushort);
                    }
                }

                return true;
            }

            value = default(ushort);
            return false;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadUInt32(out uint value)
        {

            if (ReadUInt64(out ulong ulongValue))
            {
                checked
                {
                    try
                    {
                        value = (uint)ulongValue;
                    }
                    catch (OverflowException)
                    {
                        value = default(uint);
                    }
                }

                return true;
            }

            value = default(uint);
            return false;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override bool ReadUInt64(out ulong value)
        {
            PeekEntry();

            if (peekedEntryType == EntryType.Integer)
            {
                try
                {
                    if (!ulong.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    {
                        Context.Config.DebugContext.LogError("Failed to parse integer value from entry data '" + peekedEntryData + "'.");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    ConsumeCurrentEntry();
                }
            }
            else
            {
                SkipEntry();
                value = default(ulong);
                return false;
            }
        }

        public override string GetDataDump()
        {
            var sb = new System.Text.StringBuilder();

            sb.Append("Nodes: \n\n");

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                sb.Append("    - Name: " + node.Name);

                if (i == currentIndex)
                {
                    sb.AppendLine("    <<<< READ POSITION");
                }
                else
                {
                    sb.AppendLine();
                }

                sb.AppendLine("      Entry: " + (int)node.Entry);
                sb.AppendLine("      Data: " + node.Data);
            }

            return sb.ToString();
        }

        private void ConsumeCurrentEntry()
        {
            if (peekedEntryType != null && peekedEntryType != EntryType.EndOfStream)
            {
                peekedEntryType = null;
            }
        }

        /// <summary>
        /// Peeks the current entry.
        /// </summary>
        /// <returns>The peeked entry.</returns>
        protected override EntryType PeekEntry()
        {
            string name;
            return PeekEntry(out name);
        }

        /// <summary>
        /// Consumes the current entry, and reads to the next one.
        /// </summary>
        /// <returns>The next entry.</returns>
        protected override EntryType ReadToNextEntry()
        {
            string name;
            ConsumeCurrentEntry();
            return PeekEntry(out name);
        }
    }
}