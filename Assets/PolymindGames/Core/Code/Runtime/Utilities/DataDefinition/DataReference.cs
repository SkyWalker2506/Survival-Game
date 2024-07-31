using System;
using UnityEngine;

namespace PolymindGames
{
    /// <summary>
    /// Represents a reference to a data definition identified by an integer ID.
    /// </summary>
    /// <typeparam name="T">The type of data definition associated with the reference.</typeparam>
    [Serializable]
    public struct DataIdReference<T> : IEquatable<DataIdReference<T>> where T : DataDefinition<T>
    {
        [SerializeField]
        private int _value;

        
        /// <summary>
        /// A predefined null reference for initializing or representing absence of reference.
        /// </summary>
        public static readonly DataIdReference<T> NullRef = new(0);
        
        public readonly T Def => DataDefinition<T>.GetWithId(_value);
        public readonly bool IsNull => _value == NullRef;
        public readonly int Id => _value;
        public readonly string Name => IsNull ? DataNameReference<T>.NullRef.Name : Def.Name;
        public readonly Sprite Icon => IsNull ? null : Def.Icon;
        public readonly string Description => IsNull ? string.Empty : Def.Description;
        
		#region Constructors
        public DataIdReference(T def)
        {
            _value = def != null ? def.Id : NullRef._value;
        }

        public DataIdReference(int id)
        {
            _value = id;
        }

        public DataIdReference(string name)
        {
            _value = DataDefinition<T>.TryGetWithName(name, out var def)
                ? def.Id
                : NullRef._value;
        }

        public DataIdReference(DataNameReference<T> reference)
        {
            _value = reference.IsNull ? NullRef._value : reference.Id;
        }
		#endregion

		#region Operators
        public static bool operator ==(DataIdReference<T> x, DataIdReference<T> y) => x._value == y._value;
        public static bool operator ==(DataIdReference<T> x, DataNameReference<T> y) => x._value == y.Id;
        public static bool operator ==(DataIdReference<T> x, T y) => y != null && x._value == y.Id;
        public static bool operator ==(DataIdReference<T> x, int y) => x._value == y;
        public static bool operator ==(DataIdReference<T> x, string y) => x.Name == y;

        public static bool operator !=(DataIdReference<T> x, DataIdReference<T> y) => x._value != y._value;
        public static bool operator !=(DataIdReference<T> x, DataNameReference<T> y) => x._value != y.Id;
        public static bool operator !=(DataIdReference<T> x, T y) => y != null && x._value != y.Id;
        public static bool operator !=(DataIdReference<T> x, int y) => x._value != y;
        public static bool operator !=(DataIdReference<T> x, string y) => x.Name != y;

        public static implicit operator DataIdReference<T>(int value) => new(value);
        public static implicit operator DataIdReference<T>(string value) => new(value);
        public static implicit operator DataIdReference<T>(T value) => new(value);

        public static implicit operator int(DataIdReference<T> reference) => reference.Id;
        public static implicit operator T(DataIdReference<T> reference) => reference.Def;
		#endregion

		#region IEquatable Implementation
        public readonly bool Equals(DataIdReference<T> other) => _value == other._value;

        public override readonly bool Equals(object obj)
        {
            return obj switch
            {
                DataIdReference<T> reference => _value == reference._value,
                int val => _value == val,
                _ => false
            };

        }

        public override readonly int GetHashCode() => _value.GetHashCode();
        public override readonly string ToString() => _value != NullRef._value ? Name : string.Empty;
		#endregion
    }

    /// <summary>
    /// Represents a reference to a data definition identified by a string name.
    /// </summary>
    /// <typeparam name="T">The type of data definition associated with the reference.</typeparam>
    [Serializable]
    public struct DataNameReference<T> : IEquatable<DataNameReference<T>> where T : DataDefinition<T>
    {
        [SerializeField]
        private string _value;

        
        /// <summary>
        /// A predefined null reference for initializing or representing absence of reference.
        /// </summary>
        public static readonly DataNameReference<T> NullRef = new(string.Empty);
        
        public readonly T Def => DataDefinition<T>.GetWithName(_value);
        public readonly bool IsNull => _value == NullRef;
        public readonly string Name => _value;
        public readonly int Id => IsNull ? DataIdReference<T>.NullRef.Id : Def.Id;
        public readonly string Description => IsNull ? string.Empty : Def.Description;
        public readonly Sprite Icon => IsNull ? null : Def.Icon;

		#region Constructors
        public DataNameReference(T def)
        {
            _value = def != null ? def.Name : NullRef._value;
        }

        public DataNameReference(string name)
        {
            _value = name ?? NullRef._value;
        }

        public DataNameReference(int id)
        {
            _value = DataDefinition<T>.TryGetWithId(id, out var def)
                ? def.Name
                : NullRef._value;
        }

        public DataNameReference(DataIdReference<T> reference)
        {
            _value = reference.IsNull ? NullRef._value : reference.Name;
        }
		#endregion

		#region Operators
        public static bool operator ==(DataNameReference<T> x, DataNameReference<T> y) => x._value == y._value;
        public static bool operator ==(DataNameReference<T> x, DataIdReference<T> y) => x._value == y.Name;
        public static bool operator ==(DataNameReference<T> x, T y) => y != null && x._value == y.Name;
        public static bool operator ==(DataNameReference<T> x, string y) => x._value == y;
        public static bool operator ==(DataNameReference<T> x, int y) => x.Id == y;

        public static bool operator !=(DataNameReference<T> x, DataNameReference<T> y) => x._value != y._value;
        public static bool operator !=(DataNameReference<T> x, DataIdReference<T> y) => x._value != y.Name;
        public static bool operator !=(DataNameReference<T> x, T y) => y != null && x._value != y.Name;
        public static bool operator !=(DataNameReference<T> x, string y) => x._value != y;
        public static bool operator !=(DataNameReference<T> x, int y) => x.Id != y;

        public static implicit operator DataNameReference<T>(int value) => new(value);
        public static implicit operator DataNameReference<T>(string value) => new(value);
        public static implicit operator DataNameReference<T>(T value) => new(value);

        public static implicit operator string(DataNameReference<T> reference) => reference.Name;
        public static implicit operator T(DataNameReference<T> reference) => reference.Def;
		#endregion

		#region IEquatable Implementation
        public readonly bool Equals(DataNameReference<T> other) => _value == other._value;

        public override readonly bool Equals(object obj)
        {
            return obj switch
            {
                DataNameReference<T> nameRef => _value == nameRef._value,
                string str => _value == str,
                _ => false
            };

        }

        public override readonly int GetHashCode() => _value.GetHashCode();
        public override readonly string ToString() => _value;
		#endregion
    }
}