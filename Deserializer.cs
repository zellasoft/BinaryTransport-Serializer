using System;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;

namespace ZellaSoft.BinaryTransport {
    public static class Deserializer {
        public static T Deserialize<T>(byte[] data) where T : new() {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream)) {
                T obj = new T();
                object boxed = obj; // Some struct related thing I don't seem to understand why this works.
                var props = typeof(T)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (PropertyInfo prop in props) {
                    Type type = prop.PropertyType;

                    if (type.IsEnum) {
                        Type underlying = Enum.GetUnderlyingType(type);
                        object value;
                        if (underlying == typeof(byte)) value = reader.ReadByte();
                        else if (underlying == typeof(sbyte)) value = reader.ReadSByte();
                        else if (underlying == typeof(short)) value = reader.ReadInt16();
                        else if (underlying == typeof(ushort)) value = reader.ReadUInt16();
                        else if (underlying == typeof(int)) value = reader.ReadInt32();
                        else if (underlying == typeof(uint)) value = reader.ReadUInt32();
                        else if (underlying == typeof(long)) value = reader.ReadInt64();
                        else if (underlying == typeof(ulong)) value = reader.ReadUInt64();
                        else throw new NotSupportedException("Enum base type not supported");
                        prop.SetValue(boxed, Enum.ToObject(type, value));
                    } else if (type == typeof(byte)) prop.SetValue(boxed, reader.ReadByte());
                    else if (type == typeof(sbyte)) prop.SetValue(boxed, reader.ReadSByte());
                    else if (type == typeof(short)) prop.SetValue(boxed, reader.ReadInt16());
                    else if (type == typeof(ushort)) prop.SetValue(boxed, reader.ReadUInt16());
                    else if (type == typeof(int)) prop.SetValue(boxed, reader.ReadInt32());
                    else if (type == typeof(uint)) prop.SetValue(boxed, reader.ReadUInt32());
                    else if (type == typeof(long)) prop.SetValue(boxed, reader.ReadInt64());
                    else if (type == typeof(ulong)) prop.SetValue(boxed, reader.ReadUInt64());
                    else if (type == typeof(float)) prop.SetValue(boxed, reader.ReadSingle());
                    else if (type == typeof(double)) prop.SetValue(boxed, reader.ReadDouble());
                    else if (type == typeof(bool)) prop.SetValue(boxed, reader.ReadBoolean());
                    else if (type == typeof(char)) prop.SetValue(boxed, reader.ReadChar());
                    else if (type == typeof(string)) {
                        reader.ReadString(out string str);
                        prop.SetValue(boxed, str);
                    } else if (type == typeof(byte[])) {
                        reader.ReadSizedBlock(out byte[] buff);
                        prop.SetValue(boxed, buff);
                    } else if (type.IsClass || (type.IsValueType && !type.IsPrimitive)) {
                        reader.ReadSizedBlock(out byte[] nestedBytes);
                        MethodInfo method = typeof(Deserializer)
                            .GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static)
                            .MakeGenericMethod(type);
                        object nested = method.Invoke(null, new object[] { nestedBytes });
                        prop.SetValue(boxed, nested);
                    } else throw new NotSupportedException("Type " + type + " not supported");
                }
                return (T)boxed;
            }
        }

        private static int ReadSizedBlock(this BinaryReader instance, out byte[] buff) {
            int total = 0;
            int blockSize = instance.ReadInt32(); total += sizeof(int);
            buff = instance.ReadBytes(blockSize); total += buff.Length;
            return total;
        }
        private static int ReadString(this BinaryReader instance, out string text, Encoding encoding = null) {
            encoding = encoding ?? Encoding.UTF8;
            int total = instance.ReadSizedBlock(out byte[] buff);
            text = encoding.GetString(buff);
            return total;
        }
    }
}