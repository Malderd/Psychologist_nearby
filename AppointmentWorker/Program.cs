
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

const string PipeName = "appointment_pz03";
const int MaxMessageLength = 1024;

bool closeAfterFirst =
    args.Contains("--close-after-first");

Console.WriteLine("AppointmentWorker запущен.");
Console.WriteLine($"PID: {Process.GetCurrentProcess().Id}");
Console.WriteLine($"Именованный канал: {PipeName}");

Console.WriteLine(
    "Ожидание подключения AppointmentClient...");

using var server = new NamedPipeServerStream(
    PipeName,
    PipeDirection.In,
    1,
    PipeTransmissionMode.Byte,
    PipeOptions.None);

try
{
    server.WaitForConnection();

    Console.WriteLine("AppointmentClient подключился.");

    int messageNumber = 0;

    while (server.IsConnected)
    {
        byte[]? data = ReadMessage(
            server,
            MaxMessageLength);

        if (data is null)
        {
            Console.WriteLine(
                "Отправитель завершил передачу.");

            break;
        }

        messageNumber++;

        string message =
            Encoding.UTF8.GetString(data);

        Console.WriteLine(
            $"Получено сообщение №{messageNumber}: {message}");

        if (closeAfterFirst && messageNumber == 1)
        {
            Console.WriteLine(
                "Тестовый режим: закрытие канала после первого сообщения.");

            break;
        }
    }
}
catch (IOException ex)
{
    Console.WriteLine(
        $"Ошибка чтения канала: {ex.Message}");
}

Console.WriteLine("AppointmentWorker завершён.");

static byte[]? ReadMessage(
    Stream stream,
    int maxLength)
{
    byte[] header = new byte[4];

    int first = stream.ReadByte();

    if (first == -1)
        return null;

    header[0] = (byte)first;

    ReadExactly(stream, header, 1, 3);

    int length =
        BinaryPrimitives.ReadInt32LittleEndian(header);

    if (length < 0 || length > maxLength)
    {
        throw new InvalidDataException(
            $"Недопустимая длина сообщения: {length}");
    }

    byte[] data = new byte[length];

    ReadExactly(stream, data, 0, length);

    return data;
}

static void ReadExactly(
    Stream stream,
    byte[] buffer,
    int offset,
    int count)
{
    while (count > 0)
    {
        int read = stream.Read(
            buffer,
            offset,
            count);

        if (read == 0)
        {
            throw new EndOfStreamException(
                "Канал закрыт до получения полного сообщения.");
        }

        offset += read;
        count -= read;
    }
}
