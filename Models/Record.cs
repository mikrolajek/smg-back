using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Record

{
    [Key]
    public string Id { get; set; }
    [ForeignKey("Branch")]
    public Guid BranchId { get; set; }
    public string Tag { get; set; }
    public DateTime DateTime { get; set; }
}