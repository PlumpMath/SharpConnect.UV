﻿//MIT, https://github.com/dotnet/corefxlab
using System; 
using System.Runtime.InteropServices;
using System.Net.Libuv.Internal;
namespace System.Net.Libuv
{
    public class UVBuffer
    {
        static NativeBufferPool _pool = new NativeBufferPool(65536);
        public readonly static UVBuffer Default = new UVBuffer();

        public static UVInterop.alloc_callback_unix AllocateUnixBuffer { get; set; }
        public static UVInterop.alloc_callback_win AllocWindowsBuffer { get; set; }

        private UVBuffer() { }

        static UVBuffer()
        {
            AllocateUnixBuffer = OnAllocateUnixBuffer;
            AllocWindowsBuffer = OnAllocateWindowsBuffer;
        }

        internal static void FreeBuffer(Span<byte> buffer)
        {
            _pool.Return(buffer);
        }

        static void OnAllocateUnixBuffer(IntPtr memoryBuffer, uint length, out Unix buffer)
        {
            var memory = _pool.Rent((int)length);
            unsafe
            {
                buffer = new Unix((IntPtr)memory.UnsafePointer, (uint)memory.Length);
            }
        }

        static void OnAllocateWindowsBuffer(IntPtr memoryBuffer, uint length, out Windows buffer)
        {
            var memory = _pool.Rent((int)length);
            unsafe
            {
                buffer = new Windows((IntPtr)memory.UnsafePointer, (uint)memory.Length);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Windows
        {
            internal uint Length;
            internal IntPtr Buffer;

            internal Windows(IntPtr buffer, uint length)
            {
                Buffer = buffer;
                Length = length;
            }

            internal void Dispose()
            {
                unsafe
                {
                    var readSlice = new Span<byte>((byte*)Buffer, (int)Length);
                    FreeBuffer(readSlice);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Unix
        {
            internal IntPtr Buffer;
            internal IntPtr Length;

            internal Unix(IntPtr buffer, uint length)
            {
                Buffer = buffer;
                Length = (IntPtr)length;
            }

            internal void Dispose()
            {
                unsafe
                {
                    var readSlice = new Span<byte>((byte*)Buffer, (int)Length);
                    FreeBuffer(readSlice);
                }
            }
        }
    }
}
