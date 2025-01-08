using System;
using System.IO;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Serializers.SnapshotSerializers
{
	public class SpcSerializer : SnapshotSerializerBase
	{
		// State machine for "full" RLE compression
		enum State { NO_STATE, COLLECT_SEQUENCE, COUNT_RLE };

		enum RleVersion
		{
			None = 0x00,
			Version1 = 0xFF, // Zeros RLE
			Version2 = 0xFE, // Full RLE
			VersionDebug = 0x01, // No RLE, for debugging only
		}

		public SpcSerializer(Spectrum spec) : base(spec)
		{
		}

		public override string FormatExtension => "SPC";
		
		public override bool CanDeserialize => true;
		public override bool CanSerialize => true;

		private void WriteMemory(IMemoryDevice memory, ushort addr, byte value)
		{
			var segment = addr / 0x4000;
			var segPos = addr % 0x4000;
			
			memory.RamPages[memory.Map48[segment]][segPos] = value;
		}
		
		private byte ReadMemory(IMemoryDevice memory, ushort addr)
		{
			var segment = addr / 0x4000;
			var segPos = addr % 0x4000;
			return memory.RamPages[memory.Map48[segment]][segPos];
		}
		
		public override void Deserialize(Stream stream)
		{
			var version = RleVersion.None;
			byte imMode = 0;
			int length = 0xC000;
			ushort stackPointer = 0;

			var mem = _spec.BusManager.FindDevice<IMemoryDevice>();
			var ula = _spec.BusManager.FindDevice<IUlaDevice>();

			// SPC is for 48k only, so prepare memory mapping
			mem.DOSEN = false;
			mem.SYSEN = false;
			mem.CMR0 = 0x17;

			var buf = new byte[0x80];
			
			int pos = 0x4000;
			var maxPos = pos + length;
			while (pos < maxPos)
			{
				stream.Read(buf, 0, 0x80);
				var bufPos = 0;

				if (version == RleVersion.None)
				{
					try
					{
						version = (RleVersion)buf[bufPos++];
					}
					catch
					{
						throw new Exception($"Unknown RLE method for SPC: {buf[0]}");
					}
					imMode = buf[bufPos++];
					length = buf[bufPos++] + 256 * buf[bufPos++];
					stackPointer = (ushort)(buf[bufPos++] + 256 * buf[bufPos++]);
					
					maxPos = pos + length;
				}
				
				if (version == RleVersion.Version2)
					pos = UnpackFullRle(buf, bufPos, pos, maxPos, mem);
				else if (version == RleVersion.Version1)
					pos = UnpackZerosRle(buf, bufPos, pos, maxPos, mem);
				else if (version == RleVersion.VersionDebug)
				{
					var data = new byte[length];
					for (int i = 6; i < 0x80; i++)
						data[i - 6] = buf[i];
					stream.Read(data, 0x80 - 6, length - (0x80 - 6));
					for (int i = 0; i < length; i++)
						WriteMemory(mem, (ushort)(pos++), data[i]);
				}
				else
					throw new NotImplementedException("Invalid SPC file version");
			}

			_spec.CPU.IM = Math.Min(imMode, (byte)2);

			stackPointer++;
			_spec.CPU.regs.R = ReadMemory(mem, stackPointer++);
			stackPointer++; // some flag. Depending on it the 0x00 port is been written in Quorum
			_spec.CPU.regs.I = ReadMemory(mem, stackPointer++);
			_spec.CPU.regs._AF = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));
			_spec.CPU.regs._HL = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));
			_spec.CPU.regs._DE = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));
			_spec.CPU.regs._BC = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));
			_spec.CPU.regs.IY = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));
			_spec.CPU.regs.IX = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));
			_spec.CPU.regs.HL = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));
			_spec.CPU.regs.DE = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));
			_spec.CPU.regs.BC = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));

			// Remaining AF on the stack will be restored when we jump to POP AF at 0x0050
			// _spec.CPU.regs.AF = (ushort)(ReadMemory(mem, stackPointer++) + 0x100 * ReadMemory(mem, stackPointer++));

			// Set border from BASIC variable
			var border = (ReadMemory(mem, 0x5C48) & 0x38) >> 3;
			ula.PortFE = (byte)border;

			_spec.CPU.regs.SP = stackPointer;
			_spec.CPU.regs.PC = 0x50; // POP AF : EI : RET
		}

		public override void Serialize(Stream stream)
		{
			// Set default algorithm version to latest
			var version = RleVersion.Version2;

			var mem = _spec.BusManager.FindDevice<IMemoryDevice>();
			var memoryDump = new byte[0x10000];
			for (var s = 0; s < mem.Map48.Length; s++)
				if (mem.Map48[s] >= 0)
					mem.RamPages[mem.Map48[s]].CopyTo(memoryDump, 0x4000 * s);

			var sp = _spec.CPU.regs.SP;
			memoryDump[--sp] = (byte)(_spec.CPU.regs.PC >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.PC & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.AF >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.AF & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.BC >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.BC & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.DE >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.DE & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.HL >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.HL & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.IX >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.IX & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.IY >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs.IY & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs._BC >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs._BC & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs._DE >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs._DE & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs._HL >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs._HL & 0xFF);
			memoryDump[--sp] = (byte)(_spec.CPU.regs._AF >> 8);
			memoryDump[--sp] = (byte)(_spec.CPU.regs._AF & 0xFF);
			memoryDump[--sp] = _spec.CPU.regs.I;
			memoryDump[--sp] = (byte)(_spec.CPU.regs._AF & 0xFF);
			memoryDump[--sp] = _spec.CPU.regs.R;
			memoryDump[--sp] = (byte)(_spec.CPU.regs._AF & 0xFF);

			var buf = new byte[0x80];
			var bufPos = 0;

			buf[bufPos++] = (byte)version;
			buf[bufPos++] = _spec.CPU.IM;
			buf[bufPos++] = 0x00;
			buf[bufPos++] = 0xC0;
			buf[bufPos++] = (byte)(sp & 0xFF);
			buf[bufPos++] = (byte)(sp >> 8);

			if (version == RleVersion.VersionDebug)
			{
				stream.Write(buf, 0, bufPos);
				stream.Write(memoryDump, 0x4000, 0xC000);
			}
			else
				PackRle(version, stream, memoryDump, buf, bufPos);
		}
		
		private static int PackRle(RleVersion version, Stream stream, byte[] memoryDump, byte[] buf, int bufPos)
		{
			int memAddress = 0x4000;
			while (memAddress < 0x10000)
			{
				if (version == RleVersion.Version1)
					memAddress = FillBufferZerosRle(memoryDump, memAddress, buf, ref bufPos);
				else if (version == RleVersion.Version2)
					memAddress = FillBufferFullRle(memoryDump, memAddress, buf, ref bufPos);
				stream.Write(buf, 0, bufPos);

				bufPos = 0;
			}
			return bufPos;
		}

		public static int FillBufferFullRle(byte[] memoryDump, int memAddress, byte[] buf, ref int bufPos)
		{
			var state = State.NO_STATE;
			int countPos = -1;
			while ((bufPos < 0x80 || (bufPos == 0x80 && state == State.COUNT_RLE)) && memAddress < 0x10000)
			{
				var val = memoryDump[memAddress];

				switch (state)
				{
					case State.NO_STATE:
						countPos = bufPos++;
						buf[countPos] = 0x00;
						if (bufPos == 0x80)
							return memAddress;

						if (memoryDump[(ushort)(memAddress + 1)] == val && memoryDump[(ushort)(memAddress + 2)] == val)
						{
							buf[countPos] |= 0x80;
							buf[bufPos++] = val;
							state = State.COUNT_RLE;
						}
						else
						{
							buf[bufPos++] = val;
							state = State.COLLECT_SEQUENCE;
						}
						break;
					case State.COUNT_RLE:
						if (val == buf[bufPos - 1] && buf[bufPos - 2] != 0xFF)
							buf[bufPos - 2]++;
						else
						{
							if (bufPos != 0x80)
							{
								state = State.NO_STATE;
								goto case State.NO_STATE;
							}
							else
								return memAddress;
						}
						break;
					case State.COLLECT_SEQUENCE:
						if ((buf[countPos] >= 0x80) ||
							(memoryDump[memAddress] == memoryDump[(ushort)(memAddress + 1)] && memoryDump[memAddress] == memoryDump[(ushort)(memAddress + 2)]))
						{
							state = State.NO_STATE;
							goto case State.NO_STATE;
						}

						buf[bufPos++] = val;
						buf[countPos]++;

						break;
				}
				memAddress++;
			}

			// There is a non-critical bug in SPC format: if we hit 0x80 buf boundary while collecting non-rle sequence bytes, then bytes count is not decremented
			// Mimic this behaviour so we have the same output as GAMMA.COM SAVE command
			if (state == State.COLLECT_SEQUENCE)
				buf[countPos]++;

			return memAddress;
		}

		public static int FillBufferZerosRle(byte[] memoryDump, int memAddress, byte[] buf, ref int bufPos)
		{
			while (bufPos < 0x80)
			{
				var val = memoryDump[(ushort)(memAddress++)];
				buf[bufPos++] = val;

				if (val == 0 && bufPos < 0x80 && memAddress < 0x10000)
				{
					buf[bufPos] = 1;
					while (memAddress < 0x10000 && buf[bufPos] != 0 && memoryDump[(ushort)memAddress] == val)
					{
						buf[bufPos]++;
						memAddress++;
					}

					bufPos++;
				}
				else if (memAddress >= 0x10000)
					break;
			}

			return memAddress;
		}

		private int UnpackFullRle(byte[] buf, int bufOffset, int memoryPos, int maxMemoryPos, IMemoryDevice mem)
		{
			while (memoryPos < maxMemoryPos && bufOffset < 0x80)
			{
				var b = buf[bufOffset++];
				if ((b & 0x80) != 0)
				{
					// Unpack rle
					b &= 0x7f;
					var v = buf[bufOffset++];

					for (var i = 0; i < b + 1 && memoryPos < maxMemoryPos; i++)
						WriteMemory(mem, (ushort)memoryPos++, v);
				}
				else
				{
					// Not RLE, so simply copy a series of bytes to the mem
					for (var i = 0; i < b + 1 && memoryPos < maxMemoryPos && bufOffset < 0x80; i++)
						WriteMemory(mem, (ushort)memoryPos++, buf[bufOffset++]);
				}
			}

			return memoryPos;
		}

		private int UnpackZerosRle(byte[] buf, int bufOffset, int memoryPos, int maxMemoryPos, IMemoryDevice mem)
		{
			while (memoryPos < maxMemoryPos && bufOffset < 0x80)
			{
				var b = buf[bufOffset++];

				WriteMemory(mem, (ushort)memoryPos++, b);
				
				if (b == 0 && bufOffset < 0x80)
				{
					// Unpack rle for 0x00 bytes
					short count = buf[bufOffset++];
					if (count == 0)
						count = 256;

					// We do (count - 1) copies because first of 0x00s has been written to the mem already
					for (var i = 1; i < count && memoryPos < maxMemoryPos; i++)
						WriteMemory(mem, (ushort)memoryPos++, b);
				}
			}

			return memoryPos;
		}
	}
}