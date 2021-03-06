﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CmsCoreV2.Data;
using CmsCoreV2.Models;
using SaasKit.Multitenancy;
using Z.EntityFramework.Plus;
using Microsoft.AspNetCore.Authorization;

namespace CmsCoreV2.Areas.CmsCore.Controllers
{
    [Authorize(Roles = "ADMIN,MEDIA")]
    [Area("CmsCore")]
    public class MenuItemsController : ControllerBase
    {
        public MenuItemsController(ApplicationDbContext context, ITenant<AppTenant> tenant) : base(context, tenant)
        {
            
        }

        // GET: CmsCore/MenuItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SetFiltered<MenuItem>().Where(x => x.AppTenantId == tenant.AppTenantId).Include(m => m.Menu).Include(m => m.ParentMenuItem);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CmsCore/MenuItems/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Menu)
                .Include(m => m.ParentMenuItem)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // GET: CmsCore/MenuItems/Create
        public IActionResult Create()
        {
            ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name");
           
            var menuItem = new MenuItem();
            var parentMenuItem = _context.MenuItems.ToList();
            var result = "";
            recurseMenuItem(ref parentMenuItem, null, 0, null, ref result);
            ViewBag.ParentMenuItem = result;
            return View(menuItem);
        }

        // POST: CmsCore/MenuItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CssClass,Name,Url,Target,Position,IsPublished,ParentMenuItemId,MenuId,Id,CreateDate,CreatedBy,UpdateDate,UpdatedBy,AppTenantId")] MenuItem menuItem)
        {
            menuItem.CreatedBy = User.Identity.Name ?? "username";
            menuItem.CreateDate = DateTime.Now;
            menuItem.UpdatedBy = User.Identity.Name ?? "username";
            menuItem.UpdateDate = DateTime.Now;
            menuItem.AppTenantId = tenant.AppTenantId;
            if (ModelState.IsValid)
            {
                _context.Add(menuItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var parentMenuItem = _context.MenuItems.ToList();
            var result = "";
            recurseMenuItem(ref parentMenuItem, null, 0, menuItem.ParentMenuItemId, ref result);
            ViewBag.ParentMenuItem = result;

            ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name", menuItem.MenuId);
           
            return View(menuItem);
        }


        static void recurseMenuItem(ref List<MenuItem> cl, MenuItem start, int level, long? selected, ref string result)
        {
            foreach (MenuItem child in cl)
            {
                if (child.ParentMenuItem == start)
                {
                    result += "<option"+ (selected.HasValue && child.Id == selected.Value ? "selected" : "") +" value='" + child.Id.ToString() + "'>" + (new String(' ', level*2 )).Replace(" ", "&nbsp") + child.Name + "</option>";
                    recurseMenuItem(ref cl, child, level + 1,selected, ref result);
                }
            }

        }


        // GET: CmsCore/MenuItems/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems.SingleOrDefaultAsync(m => m.Id == id);
            // var parentMenuItem = await _context.MenuItems.SingleOrDefaultAsync(m => m.Id == id);
            if (menuItem == null)
            {
                return NotFound();
            }
          
            ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name", menuItem.MenuId);

            var parentMenuItem = _context.MenuItems.ToList();
            var result = "";
            recurseMenuItem(ref parentMenuItem, null, 0, menuItem.ParentMenuItemId, ref result);
            ViewBag.ParentMenuItem = result;

            ViewData["ParentMenuItemId"] = new SelectList(_context.MenuItems.Where(mi => mi.Id != id && mi.ParentMenuItem != menuItem).ToList(), "Id", "Name", menuItem.ParentMenuItemId);

            return View(menuItem);
        }

        // POST: CmsCore/MenuItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("CssClass,Name,Url,Target,Position,IsPublished,ParentMenuItemId,MenuId,Id,CreateDate,CreatedBy,UpdateDate,UpdatedBy,AppTenantId")] MenuItem menuItem)
        {
            if (id != menuItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                menuItem.UpdatedBy = User.Identity.Name ?? "username";
                menuItem.UpdateDate = DateTime.Now;
                menuItem.AppTenantId = tenant.AppTenantId;
                try
                {
                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuItemExists(menuItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name", menuItem.MenuId);
            
            var parentMenuItem = _context.MenuItems.ToList();
            var result = "";
            recurseMenuItem(ref parentMenuItem, null, 0, menuItem.ParentMenuItemId, ref result);
            ViewBag.ParentMenuItem = result;
            
            return View(menuItem);
        }

        // GET: CmsCore/MenuItems/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Menu)
                .Include(m => m.ParentMenuItem)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // POST: CmsCore/MenuItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var menuItem = await _context.MenuItems.SingleOrDefaultAsync(m => m.Id == id);
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool MenuItemExists(long id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}
