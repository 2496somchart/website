using EasySchedule.Importer;
using FluentAssertions;

namespace EasySchedule.Api.Tests;

/// <summary>CLAUDE.md กฎเหล็กข้อ 1: Importer อ่านได้เฉพาะ *_Import แบบ ReadOnly</summary>
public sealed class ImportSourceGuardTests
{
    [Theory]
    [InlineData("Server=x;Database=Sch69_Import;ApplicationIntent=ReadOnly;Integrated Security=true")]
    [InlineData("Server=x;Database=SchKUS_Import;ApplicationIntent=ReadOnly;Integrated Security=true")]
    public void Accepts_readonly_import_databases(string cs) =>
        ImportSourceGuard.Validate(cs).ApplicationIntent.Should().Be(Microsoft.Data.SqlClient.ApplicationIntent.ReadOnly);

    [Fact]
    public void Rejects_missing_readonly_intent() =>
        FluentActions.Invoking(() => ImportSourceGuard.Validate("Server=x;Database=Sch69_Import;Integrated Security=true"))
            .Should().Throw<InvalidOperationException>().WithMessage("*ApplicationIntent=ReadOnly*");

    [Theory]
    [InlineData("Sch69")]
    [InlineData("EasyScheduleWeb")]
    [InlineData("SchKUS")]
    public void Rejects_original_or_new_databases(string database) =>
        FluentActions.Invoking(() => ImportSourceGuard.Validate($"Server=x;Database={database};ApplicationIntent=ReadOnly;Integrated Security=true"))
            .Should().Throw<InvalidOperationException>().WithMessage("*ห้ามต่อฐานเดิมโดยตรง*");
}
