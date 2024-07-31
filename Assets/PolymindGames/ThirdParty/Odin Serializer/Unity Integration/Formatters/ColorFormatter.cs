//-----------------------------------------------------------------------
// <copyright file="ColorFormatter.cs" company="Sirenix IVS">
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

using PolymindGames.OdinSerializer;

[assembly: RegisterFormatter(typeof(ColorFormatter))]

namespace PolymindGames.OdinSerializer
{
    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Color"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.Color}" />
    public sealed class ColorFormatter : MinimalBaseFormatter<Color>
    {
        private static readonly Serializer<float> s_FloatSerializer = Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Color value, IDataReader reader)
        {
            value.r = s_FloatSerializer.ReadValue(reader);
            value.g = s_FloatSerializer.ReadValue(reader);
            value.b = s_FloatSerializer.ReadValue(reader);
            value.a = s_FloatSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Color value, IDataWriter writer)
        {
            s_FloatSerializer.WriteValue(value.r, writer);
            s_FloatSerializer.WriteValue(value.g, writer);
            s_FloatSerializer.WriteValue(value.b, writer);
            s_FloatSerializer.WriteValue(value.a, writer);
        }
    }
}