using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("product")]
    public partial class Product
    {
        public Product()
        {
            Group = new HashSet<Group>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("name")]
        public string Name { get; set; }

        [InverseProperty("IdProductNavigation")]
        public virtual ICollection<Group> Group { get; set; }
    }
}
