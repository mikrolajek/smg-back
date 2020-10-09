using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("pair_tracker")]
    public partial class PairTracker
    {
        public PairTracker()
        {
            Group = new HashSet<Group>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("id_code_1")]
        public int IdCode1 { get; set; }
        [Column("id_code_2")]
        public int IdCode2 { get; set; }

        [ForeignKey(nameof(IdCode1))]
        [InverseProperty(nameof(Code.PairTrackerIdCode1Navigation))]
        public virtual Code IdCode1Navigation { get; set; }
        [ForeignKey(nameof(IdCode2))]
        [InverseProperty(nameof(Code.PairTrackerIdCode2Navigation))]
        public virtual Code IdCode2Navigation { get; set; }
        [InverseProperty("IdPairTrackerNavigation")]
        public virtual ICollection<Group> Group { get; set; }
    }
}
