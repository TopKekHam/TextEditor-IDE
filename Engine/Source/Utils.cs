using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace R
{
    public unsafe static class Utils
    {

        public static uint ClearBits(this uint b1, uint b2)
        {
            if ((b1 & b2) > 0)
            {
                return b1 & ~b2;
            }

            return b1;
        }

        public static byte ClearBits(this byte b1, byte b2)
        {
            return (byte)ClearBits((uint)b1, (uint)b2);
        }

        public static byte[] ToUtf8(string str)
        {
            int size = SDL.Utf8Size(str);
            byte[] buffer = new byte[size];


            fixed (byte* buffer_ptr = buffer)
            {
                char* s = (char*)SDL.Utf8Encode(str, buffer_ptr, size);
            }


            return buffer;
        }

        public static string FromUtf8(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static void PrintMat4(Matrix4x4 mat4)
        {

            float* ptr = &mat4.M11;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Console.Write(*(ptr + (x + (y * 4))));
                    Console.Write("|");
                }
                Console.WriteLine();
            }

        }

        public static Vector2 XY(this Vector3 vec)
        {
            return new Vector2(vec.X, vec.Y);
        }

        public static int DigitNumber(this int num)
        {
            int digits = 1;

            while (num > 10)
            {
                num /= 10;
                digits++;
            }

            return digits;
        }

        public static string Stringify(this object obj)
        {
            StringBuilder builder = new StringBuilder();

            Type type = obj.GetType();

            var fields = type.GetFields();

            foreach (var field in fields)
            {
                if (field.FieldType.IsValueType)
                {
                    builder.Append(field.Name);
                    builder.Append(" = ");
                    builder.Append(field.GetValue(obj));
                    builder.Append('\n');
                }
                else
                {
                    builder.Append(field.Name);
                    var value = field.GetValue(obj);

                    if (value == null)
                    {
                        builder.Append(" = null\n");
                    }
                    else
                    {
                        builder.Append(" = {\n");
                        builder.Append(Stringify(value));
                        builder.Append("}\n");
                    }
                }
            }

            return builder.ToString();
        }

        public unsafe static string ToString(this IntPtr char_str)
        {
            return Marshal.PtrToStringAnsi(char_str);
        }

        public static unsafe byte* ToCStr(this string str)
        {
            return (byte*)Marshal.StringToHGlobalAnsi(str).ToPointer();
        }

    }

    public unsafe struct ArrayList<T>
    {
        public int count;
        public T[] data;

        public T this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public ArrayList(int length = 32)
        {
            if (length <= 0) throw new Exception("you are trying to create list with 0 or less elements.");

            count = 0;
            data = new T[length];
        }

        public void AddItem(T item)
        {
            if(count == data.Length)
            {
                var temp = data;
                data = new T[data.Length * 2];
                Array.Copy(temp, data, temp.Length);
            }

            data[count] = item;
            count += 1;
        }

        public void RemoveItem(int index)
        {
            if (count >= index || 0 < index) throw new Exception($"you are trying to remove non-existing element(index={index}, count={count})");

            data[index] = data[count - 1];
            count -= 1;
        }

        public T[] ToArray()
        {
            var arr = new T[count];
            Array.Copy(data, arr, count);
            return arr;
        }
    }

    public unsafe struct PackedArrayList
    {
        public int size;
        public int next_offset;
        public byte* data;
        public ArrayList<int> item_offsets;

        // param size is buffered bytes in list;
        public static PackedArrayList Create(int _size = 1024)
        {
            PackedArrayList list = new PackedArrayList();
            list.data = Engine.Malloc(_size);
            list.size = _size;
            list.item_offsets = new ArrayList<int>();
            list.next_offset = 0;
            return list;
        }

        public static T* GetItem<T>(PackedArrayList list, int index) where T : unmanaged
        {
            return (T*)(list.data + list.item_offsets[index]);
        }

        public static void AddItem<T>(PackedArrayList list, void* data, int size) where T : unmanaged
        {
            if (list.next_offset + size > list.size)
            {
                byte* temp = list.data;
                list.data = Engine.Malloc(list.size * 2);
                Engine.Memcopy(list.data, temp, list.size);
                list.size *= 2;
                Engine.Free(temp);
            }

            Engine.Memcopy(list.data + list.next_offset, data, size);

            list.item_offsets.AddItem(list.next_offset);
            list.next_offset += size;
        }

        public static void Free(PackedArrayList list)
        {
            Engine.Free(list.data);
        }

    }

}
