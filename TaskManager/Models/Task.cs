using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TaskManager.Models
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }
        [Required(ErrorMessage = "Titlul taskului e obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 20 de caractere")]
        public string TaskTitle { get; set; }
        [DataType(DataType.MultilineText)]
        public string TaskDescription { get; set; }
        [Required(ErrorMessage = "Statusul este obligatoriu")]
        public Status TaskStatus { get; set; }
        public DateTime TaskDateStart { get; set; }
        public DateTime TaskDateEnd { get; set; }

        [Required(ErrorMessage ="Proiectul este obligatoriu")]
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public IEnumerable<SelectListItem> Projects { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }

    }

    public enum Status
    {
        NotStarted,
        InProgress,
        Completed
    }
}