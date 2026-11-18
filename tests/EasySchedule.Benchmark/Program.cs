// รัน solver กับข้อมูล import แล้วพิมพ์ shortfall / violations / เวลา (CLAUDE.md ข้อ 3, 7)
// ตัวอย่าง: dotnet run --project tests/EasySchedule.Benchmark -- --school Sch69 --mode 0
// M0: โครงอย่างเดียว — solver และ importer มาใน M1/M3

using System.Text;

Console.OutputEncoding = Encoding.UTF8;

string? school = null;
var mode = 0;
for (var i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--school") school = args[i + 1];
    if (args[i] == "--mode" && !int.TryParse(args[i + 1], out mode))
    {
        Console.Error.WriteLine("--mode ต้องเป็น 0 หรือ 1");
        return 2;
    }
}

Console.WriteLine($"EasySchedule Benchmark — school={school ?? "(ไม่ระบุ)"} mode={mode}");
Console.WriteLine("ยังไม่มี solver (M3) และข้อมูล import (M1) — ยังไม่มีตัวเลขให้วัด");
return 0;
