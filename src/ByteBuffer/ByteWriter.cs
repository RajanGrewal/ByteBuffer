using System;
using DotNetCross.Memory;

namespace ByteBuffer
{
	public unsafe class ByteWriter : IByteWriter
	{
		public static int DefaultBufferSize = 1024;
 
		protected byte[] m_buffer;
		protected int m_size;

		/// <summary>
		/// Clear the old buffer when we resize internally
		/// so the contents doesn't stick around in memory
		/// </summary>
		public bool ClearBuffer { get; set; }
		public int Length { get { return m_size; } }

		public ByteWriter(int bufferSize)
		{
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException();
        	
			m_buffer = new byte[bufferSize];
			m_size = 0;
		}
		public ByteWriter()
			: this(DefaultBufferSize)
		{
		}

		protected int Advance(int count)
		{
			var idx = m_size;
			var bufferSize = m_buffer.Length;
			var required = m_size + count;

			if(bufferSize < required)
			{
				while (bufferSize < required)
					bufferSize *= 2;
				
				fixed (byte* src = m_buffer)
				{
					m_buffer = new byte[bufferSize];
                	
					fixed (byte* dst = m_buffer)
					{
						Unsafe.CopyBlock(dst, src, (uint)m_size);
	
						if (ClearBuffer)
							Unsafe.InitBlock(src, 0, (uint)m_size);
					}
				}
			}
			
			m_size += count;

			return idx;
		}

		public void WriteByte(byte value)
		{
			var idx = Advance(1);
			m_buffer[idx] = value;
		}
		public void WriteBool(bool value)
		{
			var idx = Advance(1);
			m_buffer[idx] = value ? (byte)1 : (byte)0;
		}
		public void WriteBytes(byte[] buffer, int start, int length)
		{
			
        	if(buffer == null)
        		throw new ArgumentNullException();
        	
        	if(start < 0 || length <= 0)
        		throw new ArgumentOutOfRangeException();
        	
        	if(buffer.Length < start || start + length > buffer.Length)
        		throw new ArgumentOutOfRangeException();

			var idx = Advance(length);

			fixed (byte* src = buffer)
			fixed (byte* dst = m_buffer)
				Unsafe.CopyBlock(dst + idx, src + start, (uint)length);
		}
		public void WriteShort(short value)
		{
			var idx = Advance(2);

			fixed (byte* ptr = &m_buffer[idx])
				*(short*)ptr = value;
		}
		public void WriteInt(int value)
		{
			var idx = Advance(4);

			fixed (byte* ptr = &m_buffer[idx])
				*(int*)ptr = value;
		}
		public void WriteLong(long value)
		{
			var idx = Advance(8);

			fixed (byte* ptr = &m_buffer[idx])
				*(long*)ptr = value;
		}
		public void WriteString(string value = "")
		{	
		        var wvalue = Encoding.Default.GetBytes(value);
                        var length = wvalue.Length;	
			WriteShort((short)(value == "" ? 0 : length));
			
			fixed (byte* bufPtr = m_buffer)
			fixed (byte* strPtr = wvalue)
			{
				
				var buf = bufPtr + Advance(length);
				var str = strPtr;

				for (int i = 0; i < length; i++)
					*buf++ = *str++;
			}
		}

		public ArraySegment<byte> GetArraySegment()
		{
			return new ArraySegment<byte>(m_buffer, 0, m_size);
		}
		public byte[] GetBuffer()
		{
			var newBuffer = new byte[m_size];

			fixed (byte* src = m_buffer)
			fixed (byte* dst = newBuffer)
				Unsafe.CopyBlock(dst, src, (uint)m_size);

			return newBuffer;
		}
	}
}
