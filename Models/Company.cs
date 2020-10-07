using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("company")]
    public partial class Company
    {
        public Company()
        {
            Location = new HashSet<Location>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("name")]
        public string Name { get; set; }

        [InverseProperty("IdCompanyNavigation")]
        public virtual ICollection<Location> Location { get; set; }
    }
}
