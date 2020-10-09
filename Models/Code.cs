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
            PairTrackerIdCode1Navigation = new HashSet<PairTracker>();
            PairTrackerIdCode2Navigation = new HashSet<PairTracker>();
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
        [Column("taken")]
        public bool Taken { get; set; }

        [InverseProperty("IdCodeNavigation")]
        public virtual ICollection<Group> Group { get; set; }
        [InverseProperty(nameof(PairTracker.IdCode1Navigation))]
        public virtual ICollection<PairTracker> PairTrackerIdCode1Navigation { get; set; }
        [InverseProperty(nameof(PairTracker.IdCode2Navigation))]
        public virtual ICollection<PairTracker> PairTrackerIdCode2Navigation { get; set; }
    }
}
