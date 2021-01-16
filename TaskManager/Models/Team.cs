using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TaskManager.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }
        [Required(ErrorMessage = "Team name is required")]
        [StringLength(20, ErrorMessage = "Name can't be longer than 20 characters")]
        public string TeamName { get; set; }

        public string OrganizerId { get; set; }

        [Required(ErrorMessage = "Users' list is required")]
        public string[] TeamUsersId { get; set; }
        public IEnumerable<SelectListItem> TUsers { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<ApplicationUser> TeamUsers { get; set; }
    }
}