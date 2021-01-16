using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
      
        [Authorize(Roles = "User, Organizer, Admin")] 
        public ActionResult Index()
        {
            var tasks = db.Tasks.Include("Project");
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Find(currentUserId);

            ViewBag.UserTeams = user.Teams;
            ViewBag.Tasks = tasks;
            if (User.IsInRole("Admin")) ViewBag.isAdmin = 1;

            //if (TempData.ContainsKey("message"))
            //{
            //    ViewBag.message = TempData["message"].ToString();
            //}

            return View();
        }

        [Authorize(Roles = "User, Organizer, Admin")]
        public ActionResult Show(int id)
        {
            Task task = db.Tasks.Find(id);
            Team team = task.Project.Team;
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Find(currentUserId);

            if (team.TeamUsers.Contains(user) || User.IsInRole("Admin"))
            {
                if (team.OrganizerId == currentUserId) ViewBag.isOrg = 1;
                if (User.IsInRole("Admin")) ViewBag.isAdmin = 1;

                ViewBag.UserID = currentUserId;
                if (task.UserId != null)
                {
                    ApplicationUser pUser = db.Users.Find(task.UserId);
                    ViewBag.pUserName = pUser.UserName;
                }
                else ViewBag.pUserName = "unassigned";

                return View(task);
            }
            else return RedirectToAction("Index");
        }

        [Authorize(Roles = "Organizer, Admin")] 
        public ActionResult New()
        {
            Task task = new Task();
            task.Projects = GetAllProjects();
            return View(task);
        }

        [HttpPost]
        [Authorize(Roles = "Organizer, Admin")]
        public ActionResult New(Task task)
        {
            task.TaskDateStart = DateTime.Now;
            task.TaskStatus = Status.NotStarted;
            try
            {
                if (ModelState.IsValid)
                {
                    db.Tasks.Add(task);
                    db.SaveChanges();
                    return Redirect("/Projects/Show/" + task.ProjectId);
                }
                else
                {
                    task.Projects = GetAllProjects();
                    return View(task);
                }
            }
            catch (Exception e)
            {
                task.Projects = GetAllProjects();
                return View(task);
            }
        }

        [Authorize(Roles = "User, Organizer, Admin")]
        public ActionResult Edit(int id)
        {
            Task task = db.Tasks.Find(id);
            Project project = db.Projects.Find(task.ProjectId);
            Team team = project.Team;
            task.Users = GetAllUsers(team);

            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Find(currentUserId);
            
            if (team.TeamUsers.Contains(user) || User.IsInRole("Admin"))
            {
                ViewBag.isOrg = 0;
                if (team.OrganizerId == currentUserId) 
                    ViewBag.isOrg = 1;
                ViewBag.isAdmin = 0;
                if (User.IsInRole("Admin")) ViewBag.isAdmin = 1;
                return View(task);
            }
            else
                return RedirectToAction("Index");
        }

        [HttpPut]
        [Authorize(Roles = "User, Organizer, Admin")]
        public ActionResult Edit(int id, Task requestTask)
        {
            try
            {
                Task task = db.Tasks.Find(id);
                if (TryUpdateModel(task))
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                Project project = db.Projects.Find(task.ProjectId);
                Team team = project.Team;
                requestTask.Users = GetAllUsers(team);
                var currentUserId = User.Identity.GetUserId();

                ViewBag.isOrg = 0;
                if (team.OrganizerId == currentUserId)
                    ViewBag.isOrg = 1;
                ViewBag.isAdmin = 0;
                if (User.IsInRole("Admin")) ViewBag.isAdmin = 1;
                requestTask.Projects = GetAllProjects();
                return View(requestTask); 
            }
            catch (Exception e)
            {
                Project project = db.Projects.Find(requestTask.ProjectId);
                Team team = project.Team;
                requestTask.Users = GetAllUsers(team);
                var currentUserId = User.Identity.GetUserId();

                ViewBag.isOrg = 0;
                if (team.OrganizerId == currentUserId)
                    ViewBag.isOrg = 1;
                ViewBag.isAdmin = 0;
                if (User.IsInRole("Admin")) ViewBag.isAdmin = 1;
                requestTask.Projects = GetAllProjects();
                return View(requestTask);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Organizer, Admin")]
        public ActionResult Delete(int id)
        {
            Task task = db.Tasks.Find(id);

            //TempData["message"] = "Taskul " + task.TaskTitle + " a fost sters din baza de date";

            db.Tasks.Remove(task);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllProjects()
        {
            var selectList = new List<SelectListItem>();
            var currentUserId = User.Identity.GetUserId();
            var projects = from p in db.Projects
                           where p.UserId == currentUserId
                           select p;
         
            foreach (var project in projects)
            {
                selectList.Add(new SelectListItem
                {
                    Value = project.ProjectId.ToString(),
                    Text = project.ProjectName.ToString()
                });
            }
            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllUsers(Team team)
        {
            var selectList = new List<SelectListItem>();
            foreach (ApplicationUser user in team.TeamUsers.ToList())
            {
                selectList.Add(new SelectListItem
                {
                    Value = user.Id.ToString(),
                    Text = user.UserName.ToString()
                });

            }
            return selectList;
        }
    }
}

