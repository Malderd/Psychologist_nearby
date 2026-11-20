using System.IO.Pipes;
using System.Buffers.Binary;
using System.Text;

const string PipeName = "psychologist_pz03";
const int MaxFrameSize = 1024;

try
{
    // Создаём именованный канал для получения данных.
    using var server = new NamedPipeServerStream(
        PipeName,
        PipeDirection.In,
        1,
        PipeTransmissionMode.Byte,
        PipeOptions.Asynchronous);

    Console.WriteLine("Ожидание подключения отправителя...");

    await server.WaitForConnectionAsync();

    Console.WriteLine("Отправитель подключился!");

    int messageCount = 0;

    while (true)
    {
        string? message = await ReadFrameAsync(server);

        // null означает, что отправитель закрыл канал
        // между сообщениями.
        if (message == null)
        {
            Console.WriteLine("Отправитель завершил передачу.");
            break;
        }

        messageCount++;

        Console.WriteLine($"Получено сообщение №{messageCount}: {message}");

        // Режим для проверки закрытия получателя.
        if (args.Contains("--close-after-first") && messageCount == 1)
        {
            Console.WriteLine("Получатель закрывает канал.");
            break;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка получателя: {ex.Message}");
}

// Читаем одно сообщение из канала.
static async Task<string?> ReadFrameAsync(Stream stream)
{
    byte[] header = new byte[4];

    int headerBytes = await ReadUpToAsync(stream, header);

    // EOF до начала следующего сообщения — нормальное завершение.
    if (headerBytes == 0)
        return null;

    // Заголовок должен содержать ровно 4 байта.
    if (headerBytes < 4)
        throw new EndOfStreamException("Получен неполный заголовок.");

    int length = BinaryPrimitives.ReadInt32LittleEndian(header);

    if (length < 0 || length > MaxFrameSize)
        throw new InvalidDataException("Недопустимый размер сообщения.");

    byte[] payload = new byte[length];

    int payloadBytes = await ReadUpToAsync(stream, payload);

    if (payloadBytes < length)
        throw new EndOfStreamException("Получено неполное сообщение.");

    return new UTF8Encoding(false, true).GetString(payload);
}

// Читаем данные, пока не заполним буфер или не получим EOF.
static async Task<int> ReadUpToAsync(Stream stream, byte[] buffer)
{
    int total = 0;

    while (total < buffer.Length)
    {
        int read = await stream.ReadAsync(
            buffer.AsMemory(total, buffer.Length - total));

        if (read == 0)
            break;

        total += read;
    }

    return total;
}