using System.IO.Pipes;
using System.Buffers.Binary;
using System.Text;

const string PipeName = "psychologist_pz03";
const int MaxFrameSize = 1024;

bool testClose = args.Contains("--test-close");
bool keepOpen = args.Contains("--keep-open");

try
{
    // Подключаемся к каналу получателя.
    using var client = new NamedPipeClientStream(
        ".",
        PipeName,
        PipeDirection.Out,
        PipeOptions.Asynchronous);

    Console.WriteLine("Подключение к получателю...");

    await client.ConnectAsync(10000);

    Console.WriteLine("Подключение установлено!");

    string[] messages = testClose
        ? new[] { "Первое сообщение", "Второе сообщение" }
        : new[] { "Привет!", "Первое сообщение", "Второе сообщение" };

    for (int i = 0; i < messages.Length; i++)
    {
        await SendFrameAsync(client, messages[i]);

        Console.WriteLine($"Отправлено: {messages[i]}");

        // Пауза для проверки закрытия получателя.
        if (testClose && i == 0)
        {
            Console.WriteLine(
                "Закройте получатель, затем нажмите Enter.");

            Console.ReadLine();
        }
    }

    if (keepOpen)
    {
        Console.WriteLine("Канал остаётся открытым. Нажмите Enter для завершения.");
        Console.ReadLine();
    }

    Console.WriteLine("Передача завершена.");
}
catch (IOException ex)
{
    Console.WriteLine($"Ошибка передачи: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка отправителя: {ex.Message}");
}

// Формируем и отправляем одно сообщение.
static async Task SendFrameAsync(Stream stream, string message)
{
    byte[] payload = Encoding.UTF8.GetBytes(message);

    if (payload.Length > MaxFrameSize)
        throw new InvalidDataException("Сообщение слишком большое.");

    byte[] header = new byte[4];

    BinaryPrimitives.WriteInt32LittleEndian(
        header,
        payload.Length);

    // Отправляем заголовок частями.
    await stream.WriteAsync(header.AsMemory(0, 2));
    await Task.Delay(50);

    await stream.WriteAsync(header.AsMemory(2, 2));

    // Отправляем содержимое сообщения небольшими частями.
    for (int i = 0; i < payload.Length; i += 3)
    {
        int count = Math.Min(3, payload.Length - i);

        await stream.WriteAsync(
            payload.AsMemory(i, count));

        await Task.Delay(20);
    }

    await stream.FlushAsync();
}