using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyDemo.Entities
{
    public class CommonDBEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public long Id { get; set; }

        [Column("Valid")]
        public Int16 Valid { get; set; } = 1;

        [Column("Deleted")]
        public Int16 Deleted { get; set; }

        [Column("Remark")]
        [MaxLength(500)]
        public string Remark { get; set; } = string.Empty;

        [Column("Version")]
        public int Version { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Column("Sort")]
        public Int32 Sort { get; set; } = 0;

        [Column("CreatedUser")]
        [MaxLength(50)]
        public string CreatedUser { get; set; } = string.Empty;

        [Column("CreatedTime")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        [Column("CreatedDate")]
        [MaxLength(10)]
        public string CreatedDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        [Column("LastModifiedUser")]
        [MaxLength(50)]
        public string LastModifiedUser { get; set; } = string.Empty;

        [Column("LastModifiedTime")]
        public DateTime LastModifiedTime { get; set; } = DateTime.Now;

        [Column("LastModifiedDate")]
        [MaxLength(10)]
        public string LastModifiedDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        [Column("CreatedUserName")]
        [MaxLength(200)]
        public string CreatedUserName { get; set; } = string.Empty;

        [Column("LastModifiedUserName")]
        [MaxLength(200)]
        public string LastModifiedUserName { get; set; } = string.Empty;
    }
}
