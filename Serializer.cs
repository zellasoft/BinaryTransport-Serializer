using System;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;

namespace ZellaSoft.BinaryTransport {
    public static class Serializer {
        public static byte[] Serialize(object obj) {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                var props = obj.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (PropertyInfo prop in props) {
                    Type type = prop.PropertyType;
                    object val = prop.GetValue(obj, null);

                    if (type.IsEnum) {
                        Type underlying = Enum.GetUnderlyingType(type);
                        if (underlying == typeof(byte)) writer.Write((byte)val);
                        else if (underlying == typeof(sbyte)) writer.Write((sbyte)val);
                        else if (underlying == typeof(short)) writer.Write((short)val);
                        else if (underlying == typeof(ushort)) writer.Write((ushort)val);
                        else if (underlying == typeof(int)) writer.Write((int)val);
                        else if (underlying == typeof(uint)) writer.Write((uint)val);
                        else if (underlying == typeof(long)) writer.Write((long)val);
                        else if (underlying == typeof(ulong)) writer.Write((ulong)val);
                        else throw new NotSupportedException("Enum base type not supported");
                    } else if (type == typeof(byte)) writer.Write((byte)val);
                    else if (type == typeof(sbyte)) writer.Write((sbyte)val);
                    else if (type == typeof(short)) writer.Write((short)val);
                    else if (type == typeof(ushort)) writer.Write((ushort)val);
                    else if (type == typeof(int)) writer.Write((int)val);
                    else if (type == typeof(uint)) writer.Write((uint)val);
                    else if (type == typeof(long)) writer.Write((long)val);
                    else if (type == typeof(ulong)) writer.Write((ulong)val);
                    else if (type == typeof(float)) writer.Write((float)val);
                    else if (type == typeof(double)) writer.Write((double)val);
                    else if (type == typeof(bool)) writer.Write((bool)val);
                    else if (type == typeof(char)) writer.Write((char)val);
                    else if (type == typeof(string)) writer.WriteString((string)val);
                    else if (type == typeof(byte[])) writer.WriteSizedBlock((byte[])val);
                    else if (type.IsClass || (type.IsValueType && !type.IsPrimitive)) {
                        byte[] nested = Serialize(val);
                        writer.WriteSizedBlock(nested);
                    } else throw new NotSupportedException("Type " + type + " not supported");
                }

                return stream.ToArray();
            }
        }

        private static int WriteSizedBlock(this BinaryWriter instance, byte[] buff) {
            int total = 0;
            instance.Write((int)buff.Length); total += sizeof(int);
            instance.Write(buff, 0, buff.Length); total += buff.Length;
            return total;
        }
        private static int WriteString(this BinaryWriter instance, string text, Encoding encoding = null) {
            encoding = encoding ?? Encoding.UTF8;
            int total = 0;
            byte[] buff = encoding.GetBytes(text);
            instance.Write((int)buff.Length); total += sizeof(int);
            instance.Write(buff, 0, buff.Length); total += buff.Length;
            return total;
        }

    }
}
