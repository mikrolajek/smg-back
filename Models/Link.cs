using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("link")]
    public partial class Link
    {
        public Link()
        {
            Group = new HashSet<Group>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("url")]
        public string Url { get; set; }

        [InverseProperty("IdLinkNavigation")]
        public virtual ICollection<Group> Group { get; set; }
    }
}
