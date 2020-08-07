using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using MSFramework.Domain;

namespace MSFramework.Shared
{
	public class ObjectId
	{
		private static readonly ObjectIdFactory Factory = new ObjectIdFactory();

		public ObjectId(byte[] hex)
		{
			Hex = hex;
			ReverseHex();
		}

		public ObjectId(string value)
		{
			if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
			if (value.Length != 24) throw new ArgumentOutOfRangeException($"{nameof(value)} should be 24 characters");
			Hex = new byte[12];
			for (var i = 0; i < value.Length; i += 2)
			{
				try
				{
					Hex[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
				}
				catch
				{
					Hex[i / 2] = 0;
				}
			}

			ReverseHex();
		}

		public override string ToString()
		{
			Hex ??= new byte[12];

			var hexText = new StringBuilder();
			foreach (var t in Hex)
			{
				hexText.Append(t.ToString("x2"));
			}

			return hexText.ToString();
		}

		public override int GetHashCode() => ToString().GetHashCode();

		private void ReverseHex()
		{
			var time = new byte[4];
			Array.Copy(Hex, 0, time, 0, 4);
			Array.Reverse(time);
			Timestamp = BitConverter.ToInt32(time, 0);
			var copyIdx = 4;
			var mid = new byte[4];
			Array.Copy(Hex, copyIdx, mid, 0, 3);
			Machine = BitConverter.ToInt32(mid, 0);
			copyIdx += 3;
			var pIds = new byte[4];
			Array.Copy(Hex, copyIdx, pIds, 0, 2);
			Array.Reverse(pIds);
			ProcessId = BitConverter.ToInt32(pIds, 0);
			copyIdx += 2;
			var inc = new byte[4];
			Array.Copy(Hex, copyIdx, inc, 0, 3);
			Array.Reverse(inc);
			Increment = BitConverter.ToInt32(inc, 0);
		}

		public static ObjectId NewId() => Factory.NewId();

		private int CompareTo(ObjectId other)
		{
			if (other is null)
			{
				return 1;
			}

			for (var i = 0; i < Hex.Length; i++)
			{
				if (Hex[i] < other.Hex[i])
				{
					return -1;
				}

				if (Hex[i] > other.Hex[i])
				{
					return 1;
				}
			}

			return 0;
		}

		public static bool operator <(ObjectId a, ObjectId b) => a.CompareTo(b) < 0;
		public static bool operator <=(ObjectId a, ObjectId b) => a.CompareTo(b) <= 0;

		public static bool operator ==(ObjectId a, ObjectId b)
		{
			if (!Equals(a, null))
			{
				return a.CompareTo(b) == 0;
			}
			else
			{
				return b == null;
			}
		}

		public static bool operator !=(ObjectId a, ObjectId b) => !(a == b);
		public static bool operator >=(ObjectId a, ObjectId b) => a.CompareTo(b) >= 0;
		public static bool operator >(ObjectId a, ObjectId b) => a.CompareTo(b) > 0;
		public static implicit operator string(ObjectId objectId) => objectId.ToString();
		public static implicit operator ObjectId(string objectId) => new ObjectId(objectId);

		public static ObjectId Empty => new ObjectId("000000000000000000000000");

		public override bool Equals(object obj)
		{
			if (!(obj is ObjectId))
			{
				return false;
			}

			//Same instances must be considered as equal
			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			var other = (ObjectId) obj;
			return CompareTo(other) == 0;
		}

		public byte[] Hex { get; private set; }
		public int Timestamp { get; private set; }
		public int Machine { get; private set; }
		public int ProcessId { get; private set; }
		public int Increment { get; private set; }
	}

	public class ObjectIdFactory
	{
		private int _increment;
		private readonly byte[] _pidHex;
		private readonly byte[] _machineHash;
		private readonly UTF8Encoding _utf8 = new UTF8Encoding(false);
		private readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public ObjectIdFactory()
		{
			var md5 = MD5.Create();
			_machineHash = md5.ComputeHash(_utf8.GetBytes(Dns.GetHostName()));
			_pidHex = BitConverter.GetBytes(Process.GetCurrentProcess().Id);
			Array.Reverse(_pidHex);
		}

		/// <summary>
		///  产生一个新的 24 位唯一编号
		/// </summary>
		/// <returns></returns>
		public ObjectId NewId()
		{
			var hex = new byte[12];
			var time = BitConverter.GetBytes(GetTimestamp());
			Array.Reverse(time);
			Array.Copy(time, 0, hex, 0, 4);
			var copyIdx = 4;

			Array.Copy(_machineHash, 0, hex, copyIdx, 3);
			copyIdx += 3;

			Array.Copy(_pidHex, 2, hex, copyIdx, 2);
			copyIdx += 2;

			var inc = BitConverter.GetBytes(GetIncrement());
			Array.Reverse(inc);
			Array.Copy(inc, 1, hex, copyIdx, 3);

			return new ObjectId(hex);
		}

		private int GetIncrement() => System.Threading.Interlocked.Increment(ref _increment);
		private int GetTimestamp() => Convert.ToInt32(Math.Floor((DateTime.UtcNow - _unixEpoch).TotalSeconds));
	}
}