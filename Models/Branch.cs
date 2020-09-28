using System;
using System.ComponentModel.DataAnnotations;

public class Branch
{

    [Key]
    [Required]
    public Guid Id { get; set; }
    public string Location { get; set; }
    public string Url { get; set; }
    public string GuidLink { get; set; }

}

// id	lokacja	url	uidNfc
