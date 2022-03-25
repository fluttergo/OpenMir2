﻿using System;
using System.IO;
using System.Text;

namespace SystemModule
{
    /// <summary>
    /// Reads primitive data types from an array of binary data.
    /// </summary>
    public abstract class Packets
    {
        private readonly BinaryReader binaryReader;

        public Packets() { }

        public Packets(byte[] segment)
        {
            if (segment == null)
            {
                throw new Exception("segment is null");
            }
            binaryReader = new BinaryReader(new MemoryStream(segment));
        }

        public string ReadPascalString(int size)
        {
            var packegeLen = binaryReader.ReadByte();
            if (size < packegeLen)
            {
                size = packegeLen;
            }
            var strbuff = binaryReader.ReadBytes(size);
            return Encoding.GetEncoding("gb2312").GetString(strbuff, 0, packegeLen);
        }

        public int ReadInt32()
        {
            return binaryReader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            return binaryReader.ReadUInt32();
        }

        public short ReadInt16()
        {
            return binaryReader.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            return binaryReader.ReadUInt16();
        }

        public double ReadDouble()
        {
            return binaryReader.ReadDouble();
        }

        public byte ReadByte()
        {
            return binaryReader.ReadByte();
        }

        public char ReadChar()
        {
            return binaryReader.ReadChar();
        }

        public int PeekChar()
        {
            return binaryReader.PeekChar();
        }

        public bool ReadBoolean()
        {
            return binaryReader.ReadBoolean();
        }

        public byte[] ReadBytes(int size)
        {
            return binaryReader.ReadBytes(size);
        }

        public ushort[] ReadUInt16(int size)
        {
            var shortarr = new ushort[size];
            for (var i = 0; i < shortarr.Length; i++)
            {
                shortarr[i] = binaryReader.ReadUInt16();
            }
            return shortarr;
        }

        public byte[] GetPacket()
        {
            byte[] data;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    WritePacket(writer);
                    stream.Seek(0, SeekOrigin.Begin);
                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                }
            }
            return data;
        }
        
        public Packets ToPacket()
        {
            try
            {
                this.ReadPacket(binaryReader);
            }
            catch
            {
                return null;
            }
            return this;
        }

        protected abstract void ReadPacket(BinaryReader reader);

        protected abstract void WritePacket(BinaryWriter writer);
    }
}

