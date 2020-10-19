using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("log")]
    public partial class Log
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("user_agent")]
        public string UserAgent { get; set; }
        [Required]
        [Column("accept_language")]
        public string AcceptLanguage { get; set; }
        [Required]
        [Column("device_type")]
        public string DeviceType { get; set; }
        [Column("id_group")]
        public int IdGroup { get; set; }
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; }
        [Required]
        [Column("ip")]
        public string Ip { get; set; }

        [ForeignKey(nameof(IdGroup))]
        [InverseProperty(nameof(Group.Log))]
        public virtual Group IdGroupNavigation { get; set; }
    }
}
