using Microsoft.Data.SqlClient;

namespace EasySchedule.Importer;

/// <summary>
/// ด่านกันเขียนฐานเดิม (CLAUDE.md กฎเหล็กข้อ 1): Importer อ่านได้เฉพาะฐาน *_Import
/// และ connection string ต้องเป็น ApplicationIntent=ReadOnly — ตรวจก่อนเปิด connection ทุกครั้ง
/// </summary>
public static class ImportSourceGuard
{
    public static readonly string[] AllowedDatabases = ["Sch69_Import", "SchKUS_Import"];

    public static SqlConnectionStringBuilder Validate(string connectionString)
    {
        SqlConnectionStringBuilder b;
        try
        {
            b = new SqlConnectionStringBuilder(connectionString);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or KeyNotFoundException)
        {
            throw new InvalidOperationException("connection string ของฐานนำเข้าไม่ถูกต้อง", ex);
        }

        if (b.ApplicationIntent != ApplicationIntent.ReadOnly)
            throw new InvalidOperationException("connection string ของฐานนำเข้าต้องมี ApplicationIntent=ReadOnly");

        if (!AllowedDatabases.Contains(b.InitialCatalog, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"อ่านได้เฉพาะฐาน {string.Join(" / ", AllowedDatabases)} เท่านั้น (ได้รับ '{b.InitialCatalog}') — ห้ามต่อฐานเดิมโดยตรง");

        return b;
    }
}
