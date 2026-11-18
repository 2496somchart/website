using EasySchedule.Domain;
using EasySchedule.Domain.Entities;
using EasySchedule.Domain.Enums;
using FluentAssertions;

namespace EasySchedule.Solver.Tests;

public sealed class DomainTests
{
    [Theory]
    [InlineData(1, "บริหาร")]
    [InlineData(2, "กิจกรรม")]
    [InlineData(3, "สอนคณะ")]
    [InlineData(4, "ส่วนบุคคล")]
    [InlineData(5, "สอน")]
    [InlineData(6, "เลือกเสรี")]
    [InlineData(7, "พัก")]
    public void LoadType_ids_match_spec(int id, string thai) => ((LoadType)id).ToThai().Should().Be(thai);

    [Fact]
    public void LoadType_has_exactly_values_1_to_7() =>
        Enum.GetValues<LoadType>().Select(v => (int)v).Should().Equal(Enumerable.Range(1, 7));

    [Fact]
    public void LoadType_3_and_4_are_teacher_reservations() =>
        Enum.GetValues<LoadType>().Where(t => t.IsTeacherReservation()).Should().Equal(LoadType.TeamTeaching, LoadType.Personal);

    [Fact]
    public void New_lessons_default_to_teaching_and_teacher_max_consec_is_2()
    {
        new Lesson().LoadType.Should().Be(LoadType.Teaching);
        new Teacher { Code = "T", Name = "ครู 001" }.MaxConsecPeriods.Should().Be(2);
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(20, true)]
    [InlineData(30, true)]
    [InlineData(11, false)]
    [InlineData(36, false)]
    [InlineData(99, false)]
    public void ClassLevel_codes_ending_in_zero_are_aggregates(int code, bool aggregate) =>
        new ClassLevel { Code = code, Name = "x" }.IsAggregate.Should().Be(aggregate);

    [Theory]
    [InlineData(2, "จันทร์")]
    [InlineData(6, "ศุกร์")]
    public void DayNo_2_to_6_is_monday_to_friday(int dayNo, string name) => Calendar.DayNameThai(dayNo).Should().Be(name);
}

public sealed class ArchitectureTests
{
    [Fact]
    public void Solver_does_not_reference_ef_core_or_infrastructure()
    {
        var refs = typeof(ISolver).Assembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();
        refs.Should().NotContain(n => n.StartsWith("Microsoft.EntityFrameworkCore") || n.StartsWith("EasySchedule.Infrastructure")
                                      || n.StartsWith("Microsoft.Data.SqlClient") || n.StartsWith("Dapper"));
    }
}
