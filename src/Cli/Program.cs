using Core;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
Console.WriteLine("Студент: Келемен Ярослав, група ФЕІ-33с");
Console.WriteLine(new string('-', 52));

Console.WriteLine($"ОС (OSDescription) : {report.OsDescription}");
Console.WriteLine($"Runtime : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено) : {report.DetectedRid}");
Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
Console.WriteLine($"Каталог застосунку : {report.BaseDirectory}");

Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Замовлення (клієнт, товар, замовлення, рядок замовлення)");