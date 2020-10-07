using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("code")]
    public partial class Code
    {
        public Code()
        {
            Group = new HashSet<Group>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("type")]
        public string Type { get; set; }
        [Required]
        [Column("uid")]
        public string Uid { get; set; }

        [InverseProperty("IdCodeNavigation")]
        public virtual ICollection<Group> Group { get; set; }
    }
}
