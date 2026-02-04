using BCrypt.Net;
using BitacoraAlfipac.Data;
using BitacoraAlfipac.Models;
using BitacoraAlfipac.Models.Entidades;
using BitacoraAlfipac.Security;
using BitacoraAlfipac.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

[Authorize(Roles = Roles.Administrador)]
public class UsuariosController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // LISTADO
    public async Task<IActionResult> Index()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        return View(usuarios);
    }

    // FORM CREAR

    private List<SelectListItem> ObtenerRoles()
    {
        return new List<SelectListItem>
    {
        new SelectListItem { Value = Roles.Administrador, Text = "Administrador" },
        new SelectListItem { Value = Roles.Usuario, Text = "Usuario" },
        new SelectListItem { Value = Roles.Digitador, Text = "Digitador" },
        new SelectListItem { Value = Roles.Patio, Text = "Patio" }
    };
    }
    public IActionResult Create()
    {
        var model = new UsuarioCreateViewModel
        {
            RolesDisponibles = ObtenerRoles()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var passwordPlano = model.Password;

        var usuario = new Usuario
        {
            NombreUsuario = model.NombreUsuario,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlano),
            Rol = model.Rol,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        TempData["PasswordCreada"] = passwordPlano;
        TempData["UsuarioCreado"] = usuario.NombreUsuario;

        return RedirectToAction(nameof(CreateSuccess));
    }

    public IActionResult CreateSuccess()
    {
        return View();
    }

    // ACTIVAR / DESACTIVAR
    [HttpPost]
    public async Task<IActionResult> ToggleActivo(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        usuario.Activo = !usuario.Activo;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
