using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Grupo1.AgendaDeTurnos.Database;
using Grupo1.AgendaDeTurnos.Models;
using Grupo1.AgendaDeTurnos.Extensions;
using Grupo1.AgendaDeTurnos.EnumList;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Grupo1.AgendaDeTurnos.Controllers
{
    public class Profesionales : Controller
    {
        private readonly AgendaDeTurnosDbContext _context;

        public Profesionales(AgendaDeTurnosDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = nameof(RolesEnum.ADMINISTRADOR))]
        public async Task<IActionResult> Index()
        {
            var agendaDeTurnosDbContext = _context.Profesionales
                .Include(p => p.Centro)
                .Include(p => p.Prestacion)
                .Include(p => p.Turnos);
            var lista = _context.Profesionales.ToList();
            return View(await agendaDeTurnosDbContext.ToListAsync());
        }

        [Authorize(Roles = nameof(RolesEnum.ADMINISTRADOR))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesional = await _context.Profesionales
                .Include(p => p.Centro)
                .Include(p => p.Prestacion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profesional == null)
            {
                return NotFound();
            }

            return View(profesional);
        }

        [Authorize(Roles = nameof(RolesEnum.ADMINISTRADOR))]
        public IActionResult Create()

        {

            List<Disponibilidad> disponibilidades = _context.Disponibilidades.Where(d => d.IdProfesional == 0).ToList();
            ViewData["Disponibilidades"] = new MultiSelectList(disponibilidades, "Id", "Descripcion");
            ViewData["DiasSemana"] = new SelectList(Enum.GetValues(typeof(DiasEnum)).Cast<DiasEnum>());
            ViewData["CentroId"] = new SelectList(_context.Centros, "Id", "Nombre");
            ViewData["PrestacionId"] = new SelectList(_context.Prestaciones, "Id", "Nombre");
            if (disponibilidades.Count != 0)
            {
                ViewBag.estaLleno = "1";
            }


            return View();
        }


        public bool verificarExistenciaDeUsuario(string usuario)
        {
            if (_context.Profesionales.Any(p => p.Username.Equals(usuario))
                || _context.Pacientes.Any(p => p.Username.Equals(usuario))
                || _context.Administradores.Any(a => a.Username.Equals(usuario)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [Authorize(Roles = nameof(RolesEnum.ADMINISTRADOR))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrestacionId,CentroId,Id,Nombre,Apellido,Dni,Rol,Username")] Profesional profesional, string password, List<int> listDias)
        {
            string username = profesional.Username;
            if (verificarExistenciaDeUsuario(username))
            {
                ViewBag.Error = "El usuario ya existe";
                return View();
            }


            if (ModelState.IsValid)
            {
                profesional.Username = profesional.Username.ToUpper();
                profesional.Password = password.Encriptar();
                profesional.Rol = RolesEnum.PROFESIONAL;
                profesional.Disponibilidades = new List<Disponibilidad>();
                foreach (int index in listDias)
                {
                    Disponibilidad dis = await _context.Disponibilidades.FindAsync(index);
                    profesional.Disponibilidades.Add(dis);
                }
                _context.Profesionales.Add(profesional);
                await _context.SaveChangesAsync();
                //-------------------------
                borrarDisponibilidadesNoRelacionadas();
                //-------------------------
                return RedirectToAction(nameof(Index));
            }
            ViewData["DiasSemana"] = new SelectList(Enum.GetValues(typeof(DiasEnum)).Cast<DiasEnum>());
            ViewData["Disponibilidades"] = new SelectList(_context.Disponibilidades, "Id", "Nombre", profesional.Disponibilidades);
            ViewData["CentroId"] = new SelectList(_context.Centros, "Id", "Nombre", profesional.CentroId);
            ViewData["PrestacionId"] = new SelectList(_context.Prestaciones, "Id", "Nombre", profesional.PrestacionId);
            return View(profesional);
        }
        public async void borrarDisponibilidadesNoRelacionadas()
        {
            List<Disponibilidad> disponibilidades = await _context.Disponibilidades
                  .Where(d => d.IdProfesional == 0)
              .ToListAsync();
            if(disponibilidades.Count != 0)
            {
                foreach (Disponibilidad d in disponibilidades)
                {
                    _context.Disponibilidades.Remove(d);
                }
                _context.SaveChanges();
            }

        }

        [Authorize(Roles = nameof(RolesEnum.ADMINISTRADOR))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesional = await _context.Profesionales.FindAsync(id);
            if (profesional == null)
            {
                return NotFound();
            }
            ViewData["CentroId"] = new SelectList(_context.Centros, "Id", "Nombre", profesional.CentroId);
            ViewData["PrestacionId"] = new SelectList(_context.Prestaciones, "Id", "Nombre", profesional.PrestacionId);
            return View(profesional);
        }

        [HttpGet]
        [Authorize(Roles = nameof(RolesEnum.PROFESIONAL))]
        public async Task<IActionResult> Disponibilidades()
        {
            int profesionalId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var profesional = await _context.Profesionales
                .Include(p => p.Disponibilidades)
                .Where(p => p.Id == profesionalId)
                .SingleOrDefaultAsync();
            if (profesional == null)
            {
                return NotFound();
            }
            ViewData["DiasSemana"] = new SelectList(Enum.GetValues(typeof(DiasEnum)).Cast<DiasEnum>());
            ViewData["Disponibilidades"] = new MultiSelectList(_context.Disponibilidades.Where(d => d.IdProfesional == 0 || d.IdProfesional == profesionalId), 
                "Id", "Descripcion", 
                profesional.Disponibilidades.Select(d=>d.Id).ToList());
        return View(profesional);
        }
        public async Task<IActionResult> Disponibilidades(List<int> nuevasDisponibilidades)
        {
            int profesionalId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var profesional = await _context.Profesionales
                .Include(p => p.Disponibilidades)
                .Where(p => p.Id == profesionalId)
                .SingleOrDefaultAsync();
            if (profesional == null)
            {
                return NotFound();
            }
            ViewData["DiasSemana"] = new SelectList(Enum.GetValues(typeof(DiasEnum)).Cast<DiasEnum>());
            ViewData["Disponibilidades"] = new MultiSelectList(_context.Disponibilidades.Where(d => d.IdProfesional == 0 || d.IdProfesional == profesionalId),
                "Id", "Descripcion",
                profesional.Disponibilidades.Select(d => d.Id).ToList());
            return View(profesional);
        }
        public async Task<IActionResult> EditarDisponibilidades(List<int> DisponibilidadesId)
        {
            int profesionalId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            List<Disponibilidad> disponibilidadesNuevas = new List<Disponibilidad>();
            foreach (int idDis in DisponibilidadesId)
            {
                Disponibilidad d = _context.Disponibilidades.Find(idDis);
                disponibilidadesNuevas.Add(d);
            }
            Profesional profesional = await _context.Profesionales
                .Include(p => p.Disponibilidades)
                .Where(p => p.Id == profesionalId)
                .SingleOrDefaultAsync();

            profesional.Disponibilidades = disponibilidadesNuevas;
            _context.Profesionales.Update(profesional);
           await _context.SaveChangesAsync();
            borrarDisponibilidadesNoRelacionadas();
            return RedirectToAction("Index","Home");
        }

        [Authorize(Roles = nameof(RolesEnum.ADMINISTRADOR))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PrestacionId,CentroId,Id,Nombre,Apellido,Dni,Rol,Username")] Profesional profesional)
        {
            if (id != profesional.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profesional);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfesionalExists(profesional.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CentroId"] = new SelectList(_context.Centros, "Id", "Nombre", profesional.CentroId);
            ViewData["PrestacionId"] = new SelectList(_context.Prestaciones, "Id", "Nombre", profesional.PrestacionId);
            return View(profesional);
        }

        [Authorize(Roles = nameof(RolesEnum.ADMINISTRADOR))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesional = await _context.Profesionales
                .Include(p => p.Centro)
                .Include(p => p.Prestacion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profesional == null)
            {
                return NotFound();
            }

            return View(profesional);
        }

        [Authorize(Roles = nameof(RolesEnum.ADMINISTRADOR))]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profesional = await _context.Profesionales.FindAsync(id);
            _context.Profesionales.Remove(profesional);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfesionalExists(int id)
        {
            return _context.Profesionales.Any(e => e.Id == id);
        }
    }
}
