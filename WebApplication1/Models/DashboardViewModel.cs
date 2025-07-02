// في ملف اسمه DashboardViewModel.cs
using System.Collections.Generic;

public class DashboardViewModel
{
    // 1. بيانات البطاقات العلوية (KPIs)
    public int TotalPatients { get; set; }
    public int TotalTests { get; set; }
    public int PositiveCases { get; set; }
    public int NegativeCases { get; set; }

    // 2. بيانات الرسم البياني الدائري (Positive vs Negative)
    public List<string> PieChartLabels { get; set; } = new List<string>();
    public List<int> PieChartData { get; set; } = new List<int>();

    // 3. بيانات الرسم البياني الخطي (Tests per Day)
    public List<string> LineChartLabels { get; set; } = new List<string>();
    public List<int> LineChartData { get; set; } = new List<int>();
}