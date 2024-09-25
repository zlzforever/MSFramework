// See https://aka.ms/new-console-template for more information

using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark;

public class MyClass
{
    static readonly byte[] Data = Encoding.UTF8.GetBytes("Hello World!");
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Stream _stream1;
    private Stream _stream2;
    private Stream _stream3;
    private Stream _stream4;
    private Stream _stream5;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [GlobalSetup]
    public void Setup()
    {
        _stream1 = new MemoryStream(Data);
        _stream2 = new MemoryStream(Data);
        _stream3 = new MemoryStream(Data);
        _stream4 = new MemoryStream(Data);
        _stream5 = new MemoryStream(Data);
    }

    [Benchmark]
    public void MemoryStream()
    {
        _stream5.Seek(0, SeekOrigin.Begin);
        if (_stream1 is MemoryStream ms)
        {
            ms.ToArray();
        }
    }

    [Benchmark]
    public void CopyToMemoryStream()
    {
        _stream5.Seek(0, SeekOrigin.Begin);
        using var memoryStream = new MemoryStream();
        _stream2.CopyTo(memoryStream);
        memoryStream.ToArray();
    }

    [Benchmark]
    public void UsePipeReader()
    {
        _stream5.Seek(0, SeekOrigin.Begin);
        var pipeline = PipeReader.Create(_stream3);
        pipeline.TryRead(out var result);
        result.Buffer.ToArray();
    }

    [Benchmark]
    public void UseRead()
    {
        _stream5.Seek(0, SeekOrigin.Begin);
        var bytes = new byte[_stream4.Length];
        _ = _stream4.Read(bytes, 0, bytes.Length);
        _stream4.Seek(0, SeekOrigin.Begin);
    }

    [Benchmark]
    public void BinaryReader()
    {
        _stream5.Seek(0, SeekOrigin.Begin);
        var reader = new BinaryReader(_stream5);
        reader.ReadBytes((int)_stream5.Length);
    }
}

internal class Prograom
{
    public static void Main()
    {
        BenchmarkRunner.Run<MyClass>();
        Console.WriteLine("Bye!");
    }
}
