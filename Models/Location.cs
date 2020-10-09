using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("location")]
    public partial class Location
    {
        public Location()
        {
            Group = new HashSet<Group>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("id_company")]
        public int IdCompany { get; set; }
        [Required]
        [Column("address")]
        public string Address { get; set; }

        [ForeignKey(nameof(IdCompany))]
        [InverseProperty(nameof(Company.Location))]
        public virtual Company IdCompanyNavigation { get; set; }
        [InverseProperty("IdLocationNavigation")]
        public virtual ICollection<Group> Group { get; set; }
    }
}
