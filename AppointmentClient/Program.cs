
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

const string PipeName = "appointment_pz03";
const int MaxMessageLength = 1024;

bool keepOpen = args.Contains("--keep-open");
bool testClose = args.Contains("--test-close");

Console.WriteLine("AppointmentClient запущен.");
Console.WriteLine($"PID: {Process.GetCurrentProcess().Id}");
Console.WriteLine($"Канал: {PipeName}");

using var client = new NamedPipeClientStream(
    ".",
    PipeName,
    PipeDirection.Out,
    PipeOptions.None);

try
{
    Console.WriteLine("Подключение к AppointmentWorker...");
    client.Connect(10000);

    Console.WriteLine("Подключение установлено.");

    if (testClose)
    {
        SendMessage(client, "Тестовое сообщение №1");

        Console.WriteLine(
            "Ожидание закрытия канала принимающим процессом...");

        Thread.Sleep(700);

        try
        {
            SendMessage(client, "Тестовое сообщение №2");

            Console.WriteLine("Вторая отправка выполнена.");
        }
        catch (IOException ex)
        {
            Console.WriteLine(
                $"Канал закрыт или недоступен: {ex.Message}");
        }
    }
    else if (keepOpen)
    {
        Console.WriteLine(
            "Введите сообщение. Пустая строка завершает передачу.");

        while (true)
        {
            Console.Write("> ");

            string? text = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(text))
                break;

            SendMessage(client, text);

            Console.WriteLine("Сообщение отправлено.");
        }
    }
    else
    {
        SendMessage(
            client,
            "Создана тестовая запись на приём к психологу.");

        SendMessage(
            client,
            "Дата и время приёма: 25.11.2026 15:00");

        SendMessage(
            client,
            "Статус записи: ожидает подтверждения");

        Console.WriteLine("Три сообщения отправлены.");
    }
}
catch (TimeoutException)
{
    Console.WriteLine(
        "AppointmentWorker не подключился за отведённое время.");
}
catch (IOException ex)
{
    Console.WriteLine($"Ошибка передачи данных: {ex.Message}");
}

Console.WriteLine("AppointmentClient завершён.");

static void SendMessage(
    Stream stream,
    string text,
    int maxLength = 1024)
{
    byte[] data = Encoding.UTF8.GetBytes(text);

    if (data.Length > maxLength)
    {
        throw new InvalidDataException(
            "Сообщение превышает 1024 байта.");
    }

    byte[] header = new byte[4];

    BinaryPrimitives.WriteInt32LittleEndian(
        header,
        data.Length);

    stream.Write(header, 0, header.Length);
    stream.Write(data, 0, data.Length);
    stream.Flush();
}
