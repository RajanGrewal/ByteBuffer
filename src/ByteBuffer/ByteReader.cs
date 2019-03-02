using System;
using System.Runtime.InteropServices;
using DotNetCross.Memory;

namespace ByteBuffer
{
    public unsafe class ByteReader : IByteReader, IDisposable
    {
        private readonly GCHandle m_handle;
        protected byte* m_buffer;
        protected readonly int m_length;
        protected int m_index;
        protected bool m_disposed;

        public int Length { get { return m_length; } }
        public int Position { get { return m_index; } }
        public bool Disposed { get { return m_disposed; } }

        public ByteReader(byte[] buffer, int start, int length)
        {        	
        	if(buffer == null)
        		throw new ArgumentNullException();
        	
        	if(start < 0 || length <= 0)
        		throw new ArgumentOutOfRangeException();
        	
        	if(buffer.Length < start || start + length > buffer.Length)
        		throw new ArgumentOutOfRangeException();        	
        	
            m_handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            m_buffer = (byte*)m_handle.AddrOfPinnedObject() + start;
            m_length = length;
            m_index = 0;
            m_disposed = false;
        }
        public ByteReader(byte[] buffer)
        	: this(buffer, 0, buffer.Length)
        {
        }

        protected byte* Advance(int count)
        {
        	if(m_disposed)
        		throw new ObjectDisposedException(GetType().FullName);

            if (m_index + count > m_length)
            	throw new IndexOutOfRangeException();

            var buf = m_buffer;

            m_buffer += count;
            m_index += count;

            return buf;
        }

        public byte ReadByte()
        {
            return *Advance(1);
        }
        public bool ReadBool()
        {
            return *Advance(1) != 0;
        }
        public byte[] ReadBytes(int count)
        {
            var src = Advance(count);
            var data = new byte[count];

            fixed (byte* dst = data)
                Unsafe.CopyBlock(dst, src, (uint)count);

            return data;
        }
        public short ReadShort()
        {
            return *(short*)Advance(2);
        }
        public int ReadInt()
        {
            return *(int*)Advance(4);
        }
        public long ReadLong()
        {
            return *(long*)Advance(8);
        }
        public string ReadString(int count = -1)
        {
            return Encoding.Default.GetString(ReadBytes(count == -1 ? ReadShort() : count));
        }
        
        public void Skip(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException();
            
            Advance(count);
        }

        public void Dispose()
        {
            if (!m_disposed)
            {
                m_handle.Free();
                m_disposed = true;
            }
        }
    }
}
