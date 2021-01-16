using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        [Authorize(Roles = "User, Organizer, Admin")]
        public ActionResult Index()
        {
            var projects = db.Projects.Include("Team").Include("User");
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Find(currentUserId);

            ViewBag.User = user;
            ViewBag.Projects = projects;
            if (User.IsInRole("Admin")) ViewBag.isAdmin = 1;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            return View();
        }

        [Authorize(Roles = "User, Organizer, Admin")]
        public ActionResult Show(int id)
        {
            Project project = db.Projects.Find(id);
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Find(currentUserId);

            if (project.Team.TeamUsers.Contains(user) || User.IsInRole("Admin"))
            {
                var tasks = from task in db.Tasks
                            where task.ProjectId == id
                            select task;
                ViewBag.Tasks = tasks;

                if (project.UserId == currentUserId) ViewBag.isOrg = 1;
                ApplicationUser organizer = db.Users.Find(project.UserId);
                ViewBag.Organizer = organizer;
                if (User.IsInRole("Admin")) ViewBag.isAdmin = 1;

                return View(project);
            }
            else return RedirectToAction("Index");
        }

        [Authorize(Roles = "User, Organizer, Admin")]
        public ActionResult New()
        {
            Project project = new Project();
            project.Teams = GetAllTeams();
            project.UserId = User.Identity.GetUserId();
           
            return View(project);
        }

        [HttpPost]
        [Authorize(Roles = "User, Organizer, Admin")]
        public ActionResult New(Project project)
        {
            project.UserId = User.Identity.GetUserId();
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationDbContext context = new ApplicationDbContext();
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                    UserManager.RemoveFromRole(project.UserId, "User");
                    UserManager.AddToRole(project.UserId, "Organizer");

                    db.Projects.Add(project);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    project.Teams = GetAllTeams();
                    project.UserId = User.Identity.GetUserId();
                    return View(project);
                }
            }
            catch (Exception e)
            {
                project.Teams = GetAllTeams();
                project.UserId = User.Identity.GetUserId();
                return View(project);
            }
        }

        [Authorize(Roles = "Organizer, Admin")]
        public ActionResult Edit(int id)
        {
            Project project = db.Projects.Find(id);
            var currentUserId = User.Identity.GetUserId();
            if (project.UserId == currentUserId || User.IsInRole("Admin"))
            {
                return View(project);
            }
            return RedirectToAction("Index");
        }

        [HttpPut]
        [Authorize(Roles = "Organizer, Admin")]
        public ActionResult Edit(int id, Project requestProject)
        {
            try
            {
                Project project = db.Projects.Find(id);
                if (TryUpdateModel(project))
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(requestProject);
            }
            catch (Exception e)
            {
                return View(requestProject);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Organizer, Admin")]
        public ActionResult Delete(int id)
        {
            Project project = db.Projects.Find(id);

            TempData["message"] = "Proiectul " + project.ProjectName + " a fost sters din baza de date";

            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllTeams()
        {
            var selectList = new List<SelectListItem>();
            var currentUserId = User.Identity.GetUserId();
            if (User.IsInRole("Admin"))
            {
                var teams = from t in db.Teams
                        select t;
                foreach (var team in teams)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = team.TeamId.ToString(),
                        Text = team.TeamName.ToString()
                    });
                }
            }
            else
            {
                var teams = from t in db.Teams
                        where t.OrganizerId == currentUserId
                        select t;
                foreach (var team in teams)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = team.TeamId.ToString(),
                        Text = team.TeamName.ToString()
                    });
                }
            }   
            
            return selectList;
        }
    }
}