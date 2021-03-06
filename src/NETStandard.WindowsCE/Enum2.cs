﻿// Ref: https://opennetcf.codeplex.com/
// Ref: https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Enum.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    public static class Enum2
    {
        private const string ValuesSeparator = ", ";
        private const string ZeroString = "0";
        private static readonly Type SystemEnumType = typeof(Enum);

        /// <summary>
        /// Converts the specified value of a specified enumerated type to its equivalent string representation according to the specified format. 
        /// </summary>
        /// <remarks>
        /// The valid format values are: 
        /// "G" or "g" - If value is equal to a named enumerated constant, the name of that constant is returned; otherwise, the decimal equivalent of value is returned.
        /// For example, suppose the only enumerated constant is named, Red, and its value is 1. If value is specified as 1, then this format returns "Red". However, if value is specified as 2, this format returns "2".
        /// "X" or "x" - Represents value in hexadecimal without a leading "0x". 
        /// "D" or "d" - Represents value in decimal form.
        /// "F" or "f" - Behaves identically to "G" or "g", except the FlagsAttribute is not required to be present on the Enum declaration. 
        /// </remarks>
        /// <param name="enumType">The enumeration type of the value to convert.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="format">The output format to use.</param>
        /// <returns>A string representation of value.</returns>
        public static string Format(Type enumType, object value, string format)
        {
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (!enumType.IsEnum) throw new ArgumentException("The argument enumType must be an System.Enum.", nameof(enumType));

            var valueType = value.GetType();
            if (valueType.IsEnum && enumType != valueType)
                throw new ArgumentException("The value must be of the same type of enum type.");

            if (!valueType.IsEnum)
            {
                var underlyingType = Enum.GetUnderlyingType(enumType);
                if (valueType != underlyingType)
                    throw new ArgumentException("The enum underlying type and the object type must be the same.");
            }

            if (format.Length != 1)
                throw new FormatException("Invalid format");

            char formatCh = format[0];
            if (formatCh == 'G' || formatCh == 'g')
                return InternalFormat(enumType, value);
            if (formatCh == 'F' || formatCh == 'f')
                return InternalValuesFormat(enumType, value);
            if (formatCh == 'X' || formatCh == 'x')
                return InternalFormattedHexString(value);
            if (formatCh == 'D' || formatCh == 'd')
                return Convert.ToUInt64(value).ToString();

            throw new FormatException("Invalid format");
        }

        /// <summary>
        /// Retrieves the name of the constant in the specified enumeration that has the specified value.
        /// </summary>
        /// <param name="enumType">An enumeration type.</param>
        /// <param name="value">The value of a particular enumerated constant in terms of its underlying type.</param>
        /// <returns> A string containing the name of the enumerated constant in enumType whose value is value, or null if no such constant is found.</returns>
        /// <exception cref="ArgumentException"> enumType is not an System.Enum.  -or-  value is neither of type enumType nor does it have the same underlying type as enumType.</exception>
        public static string GetName(Type enumType, object value)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Type valueType = value.GetType();
            if (!valueType.IsEnum && !Type2.IsIntegerNumber(valueType))
                throw new ArgumentException("The specified value should be enum base or an integer number", nameof(value));

            int length;
            bool isFlags;
            Type underlyingType;
            var valuesAndNames = GetValuesAndNames(enumType, out length, out isFlags, out underlyingType);

            object numValue = Convert.ChangeType(value, underlyingType, null);
            //cycle through the enum values
            foreach (var item in valuesAndNames)
            {
                //if value matches return the name
                if (numValue.Equals(item.ValueAsNumber))
                    return item.Name;
            }

            //if there is no match return null
            return null;
        }

        /// <summary>
        /// Retrieves an array of the names of the constants in a specified enumeration.
        /// </summary>
        /// <param name="enumType">An enumeration type.</param>
        /// <returns>A string array of the names of the constants in enumType. The elements of the array are sorted by the values of the enumerated constants.</returns>
        /// <exception cref="ArgumentException">enumType parameter is not an System.Enum</exception>
        public static string[] GetNames(Type enumType)
        {
            int length;
            bool isFlags;
            Type underlyingType;
            var valuesAndNames = GetValuesAndNames(enumType, out length, out isFlags, out underlyingType);

            //create a new enum array
            string[] names = new string[length];

            //populate with the values
            int index = 0;
            foreach (var item in valuesAndNames)
            {
                names[index] = item.Name;
                index++;
            }

            //return the array
            return names;
        }

        public static Type GetUnderlyingType(Type enumType)
            => Enum.GetUnderlyingType(enumType);

        /// <summary>
        /// Retrieves an array of the values of the constants in a specified enumeration.
        /// </summary>
        /// <param name="enumType">An enumeration type.</param>
        /// <returns>An System.Array of the values of the constants in enumType. The elements of the array are sorted by the values of the enumeration constants.</returns>
        /// <exception cref="ArgumentException">enumType parameter is not an System.Enum</exception>
        public static Array GetValues(Type enumType)
        {
            int length;
            bool isFlags;
            Type underlyingType;
            var valuesAndNames = GetValuesAndNames(enumType, out length, out isFlags, out underlyingType);

            //create a new enum array
            Array values = Array.CreateInstance(enumType, length);

            //populate with the values
            int index = 0;
            foreach (var item in valuesAndNames)
            {
                values.SetValue(item.Value, index);
                index++;
            }

            //return the array
            return values;
        }

        public static bool HasFlag(this Enum e, Enum flag)
        {
            if (flag == null)
                throw new ArgumentNullException(nameof(flag));

            {
                var t_e = e.GetType();
                var t_flag = flag.GetType();
                if (t_e != t_flag)
                    throw new ArgumentException($"The type '{t_flag.Name}' does not match '{t_e.Name}");
            }

            ulong num = Convert.ToUInt64(flag);
            return ((Convert.ToUInt64(e) & num) == num);
        }

        public static bool IsDefined(Type enumType, object value)
            => Enum.IsDefined(enumType, value);

        public static object Parse(Type enumType, string value)
            => Enum.Parse(enumType, value, false);

        public static object Parse(Type enumType, string value, bool ignoreCase)
            => Enum.Parse(enumType, value, ignoreCase);

        public static object ToObject(Type enumType, object value)
            => Enum.ToObject(enumType, value);

        public static bool TryParse<TEnum>(string value, out TEnum result)
            where TEnum : struct
        {
            bool retVal = false;
            try
            {
                object parsed = Enum.Parse(typeof(TEnum), value, false);
                result = (TEnum)parsed;
                retVal = true;
            }
            catch (FormatException) { result = (TEnum)(object)0; }
            catch (InvalidCastException) { result = (TEnum)(object)0; }

            return retVal;
        }

        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result)
            where TEnum : struct
        {
            bool retVal = false;
            try
            {
                object parsed = Enum.Parse(typeof(TEnum), value, ignoreCase);
                result = (TEnum)parsed;
                retVal = true;
            }
            catch (FormatException) { result = (TEnum)(object)0; }
            catch (InvalidCastException) { result = (TEnum)(object)0; }

            return retVal;
        }

        private static IEnumerable<TypeValueAndName> GetValuesAndNames(Type enumType, out int length, out bool isFlags, out Type underlyingType)
        {
            // Check that the type supplied inherits from System.Enum
            if (!enumType.IsEnum)
            {
                //the type supplied does not derive from enum
                throw new ArgumentException(
                    "Cannot get names from type not derived from System.Enum",
                    nameof(enumType));
            }

            // Get the public static fields (members of the enum)
            FieldInfo[] fi = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);

            isFlags = enumType.IsDefined(typeof(FlagsAttribute), false);
            length = fi.Length;
            underlyingType = Enum.GetUnderlyingType(enumType);

            return GetValuesAndNamesIterator(underlyingType, fi);
        }

        private static IEnumerable<TypeValueAndName> GetValuesAndNamesIterator(Type underlyingType, FieldInfo[] fi)
        {
            //populate with the values
            FieldInfo currField = null;
            for (int iEnum = 0; iEnum < fi.Length; iEnum++)
            {
                currField = fi[iEnum];
                yield return new TypeValueAndName(
                    underlyingType,
                    currField.GetValue(null),
                    currField.Name);
            }
        }

        private static string InternalFormat(Type enumType, object value)
        {
            if (enumType.IsDefined(typeof(FlagsAttribute), false))
                return InternalValuesFormat(enumType, value);

            string t = GetName(enumType, value);
            if (t == null)
                return value.ToString();

            return t;
        }

        private static string InternalFlagsFormat(Type enumType, object value)
        {
            string t = GetName(enumType, value);
            if (t == null)
                return value.ToString();

            return t;
        }

        private static string InternalValuesFormat(Type enumType, object value)
        {
            int length;
            bool isFlags;
            Type underlyingType;
            var valuesAndNames = GetValuesAndNames(enumType, out length, out isFlags, out underlyingType)
                .OrderBy(a => a.ValueAsNumber)
                .ToArray();

            if (valuesAndNames.Length == 0)
                return value.ToString();

            if (Type2.IsSignedNumber(underlyingType))
                return InternalSignedValuesFormat(enumType, value, valuesAndNames);
            else
                return InternalUnsignedValuesFormat(enumType, value, valuesAndNames);
        }

        private static string InternalSignedValuesFormat(Type enumType, object value, TypeValueAndName[] valuesAndNames)
        {
            long result = Convert.ToInt64(value);

            // For the case when we have zero
            if (result == 0L)
                return Enum.ToObject(enumType, 0L).ToString();

            int index = valuesAndNames.Length - 1;
            StringBuilder retVal = new StringBuilder();
            bool firstTime = true;

            while (index >= 0)
            {
                long currVal = Convert.ToInt64(valuesAndNames[index].Value);

                if ((index == 0) && (currVal == 0))
                    break;

                if ((result & currVal) == currVal)
                {
                    result -= currVal;

                    if (!firstTime)
                        retVal.Insert(0, ValuesSeparator);

                    retVal.Insert(0, valuesAndNames[index].Name);
                    firstTime = false;
                }

                index--;
            }

            if (result != 0L)
                return value.ToString();

            return retVal.ToString();
        }

        private static string InternalUnsignedValuesFormat(Type enumType, object value, TypeValueAndName[] valuesAndNames)
        {
            ulong result = Convert.ToUInt64(value);

            // For the case when we have zero
            if (result == 0UL)
                return Enum.ToObject(enumType, 0UL).ToString();

            int index = valuesAndNames.Length - 1;
            StringBuilder retVal = new StringBuilder();
            bool firstTime = true;

            while (index >= 0)
            {
                ulong currVal = Convert.ToUInt64(valuesAndNames[index].Value);

                if ((index == 0) && (currVal == 0))
                    break;

                if ((result & currVal) == currVal)
                {
                    result -= currVal;

                    if (!firstTime)
                        retVal.Insert(0, ValuesSeparator);

                    retVal.Insert(0, valuesAndNames[index].Name);
                    firstTime = false;
                }

                index--;
            }

            if (result != 0UL)
                return value.ToString();

            return retVal.ToString();
        }

        private static string InternalFormattedHexString(object value)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                    {
                        sbyte n = (sbyte)value;
                        return ((byte)n).ToString("X2", null);
                    }
                case TypeCode.Byte:
                    {
                        byte n = (byte)value;
                        return n.ToString("X2", null);
                    }
                case TypeCode.Boolean:
                    {
                        bool n = (bool)value;
                        return Convert.ToByte(n).ToString("X2", null);
                    }
                case TypeCode.Int16:
                    {
                        short n = (short)value;
                        return ((ushort)n).ToString("X4", null);
                    }
                case TypeCode.UInt16:
                    {
                        ushort n = (ushort)value;
                        return n.ToString("X4", null);
                    }
                case TypeCode.Char:
                    {
                        char n = (char)value;
                        return ((ushort)n).ToString("X4", null);
                    }
                case TypeCode.Int32:
                    {
                        int n = (int)value;
                        return ((uint)n).ToString("X8", null);
                    }
                case TypeCode.UInt32:
                    {
                        uint n = (uint)value;
                        return n.ToString("X8", null);
                    }
                case TypeCode.Int64:
                    {
                        int n = (int)value;
                        return ((ulong)n).ToString("X16", null);
                    }
                case TypeCode.UInt64:
                    {
                        uint n = (uint)value;
                        return n.ToString("X16", null);
                    }

            }

            throw new InvalidOperationException("Unknown enum type");
        }

        #region Definitions
        private class TypeValueAndName : IComparable<TypeValueAndName>
        {
            private object valueAsNumber;
            private readonly Type underlyingType;

            public object Value { get; set; }
            public string Name { get; set; }

            public object ValueAsNumber
            {
                get
                {
                    if (valueAsNumber == null)
                        valueAsNumber = Convert.ChangeType(Value, underlyingType, null);

                    return valueAsNumber;
                }
            }

            // Each entry contains a list of sorted pair of enum field names and values, sorted by values
            public TypeValueAndName(Type underlyingType, object value, string name)
            {
                Value = value;
                Name = name;
                this.underlyingType = underlyingType;
            }

            public override bool Equals(object obj)
            {
                TypeValueAndName other = obj as TypeValueAndName;
                if (obj == null)
                    return false;

                return Value == other.Value &&
                    Name == other.Name;
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public int CompareTo(object obj)
            {
                TypeValueAndName other = obj as TypeValueAndName;
                if (other == null)
                    throw new ArgumentException("Null or invalid parameter", nameof(obj));

                return CompareTo(other);
            }

            public int CompareTo(TypeValueAndName other)
            {
                var comparableValue = ValueAsNumber as IComparable;
                if (comparableValue == null)
                    return 0;

                return comparableValue.CompareTo(other.ValueAsNumber);
            }
        }
        #endregion
    }
}
