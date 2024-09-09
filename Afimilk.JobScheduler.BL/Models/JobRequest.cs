using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class JobRequest
{
    public string Type { get; set; }

    public TimeSpan DailyExecutionTime { get; set; } // Use TimeSpan to store the time of day

    [Range(1, int.MaxValue, ErrorMessage = "Occurrences must be greater than 0.")]
    [DefaultValue(1)]
    [Required]
    public int Occurrences { get; set; } = 1; // Default value is 1
}
